namespace Auth.Identity.Services.TokenService
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using ApiAuthLib;
    using Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using NodaTime;

    public class TokenService : ITokenService
    {
        private readonly AppIdentityDbContext identityDbContext;
        private readonly JwtSettings jwtSettings;
        private readonly TokenValidationParameters tokenValidationParameters;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<AppRole> roleManager;
        private readonly ILogger<TokenService> logger;

        public TokenService(AppIdentityDbContext identityDbContext,
            JwtSettings jwtSettings,
            TokenValidationParameters tokenValidationParameters,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ILogger<TokenService> logger,
            HttpClient httpClient
        )
        {
            this.identityDbContext = identityDbContext;
            this.jwtSettings = jwtSettings;
            this.tokenValidationParameters = tokenValidationParameters;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.logger = logger;
        }

        /// <summary>
        ///     Generates a new AuthResponse with the given refreshToken and jwtToken
        /// </summary>
        /// <param name="refreshTokenString">the refresh token</param>
        /// <param name="jwtToken">the jwt token</param>
        /// <returns>A new AuthResponse containing either the tokens or error messages</returns>
        public async Task<AuthResponse> RefreshAsync(string refreshTokenString, string jwtToken)
        {
            var errorResponse = AuthResponse.Failure(new[] {"Translation.InvalidToken"});

            var validatedToken = PrincipalFromToken(jwtToken);
            if (null == validatedToken)
            {
                logger.LogError("No principal from token");
                return errorResponse;
            }


            var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
            var identityOptions = new IdentityOptions();
            var storedRefreshToken = await identityDbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == refreshTokenString);
            var userId = Guid.Parse(validatedToken.Claims.FirstOrDefault(x => x.Type == identityOptions.ClaimsIdentity.UserIdClaimType)?.Value ?? Guid.Empty.ToString());

            if (storedRefreshToken == null
                || SystemClock.Instance.GetCurrentInstant() > storedRefreshToken.Expires
                || storedRefreshToken.Invalidated
                || storedRefreshToken.Used
                || storedRefreshToken.JwtTokenId != jti
                || userId != storedRefreshToken.UserId
            )
            {
                logger.LogError($"invalid token in refresh, data: storedRefreshToken: {storedRefreshToken?.Id}, {storedRefreshToken?.Token}, expires: {storedRefreshToken?.Expires} (now: {SystemClock.Instance.GetCurrentInstant()}), "
                                + $"invalidated: {storedRefreshToken?.Invalidated}, used: {storedRefreshToken?.Used}, token id: {storedRefreshToken?.JwtTokenId} (should be: {jti}), " +
                                $"userId: {storedRefreshToken?.UserId} (should be: {userId})");
                return errorResponse;
            }

            storedRefreshToken.Used = true;
            logger.LogTrace($"Used refresh token: {storedRefreshToken.Id} {storedRefreshToken?.Token}");
            identityDbContext.RefreshTokens.Update(storedRefreshToken);
            await identityDbContext.SaveChangesAsync();

            var user = await userManager.FindByIdAsync(userId.ToString());
            return await GenerateAuthenticationResultForUserAsync(user);
        }

        /// <summary>
        ///     Generates a AuthResponse based on existing login data
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="password">the unhashed password</param>
        /// <returns>An AuthResponse with either the token inside or an error message</returns>
        public async Task<AuthResponse> LoginAsync(string username, string password)
        {
            var errorResponse = AuthResponse.Failure(new[] {"Translation.UsernamePasswordWrong"});

            var user = await userManager.FindByNameAsync(username) ?? await userManager.FindByEmailAsync(username);
            if (null == user || !await userManager.CheckPasswordAsync(user, password))
            {
                return errorResponse;
            }

            
            return await GenerateAuthenticationResultForUserAsync(user);
        }


        public async Task<AuthResponse> LoginAzureAsync(string username, string token)
        {
            var user = await userManager.FindByNameAsync(username) ?? await userManager.FindByEmailAsync(username);
            var domain = user.Email.Split("@")[1];
            var azConfig = await identityDbContext.AzureAdConfigs
                .Where(azAdConfig => azAdConfig.Domain.ToLower().Equals(domain.ToLower()))
                .FirstOrDefaultAsync();
            if (null == azConfig)
            {
                return AuthResponse.Failure(new[] {"Translation.UserNoAdConfig"});
            }

            
            // get public key
            var wellKnownUrl = $"https://login.microsoftonline.com/common/.well-known/openid-configuration?appid={azConfig.ClientId}";
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(wellKnownUrl, new OpenIdConnectConfigurationRetriever());
            var config = await configManager.GetConfigurationAsync();
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = false, // do not validate issuer in multi-tenant setup
                ValidAudience = azConfig.ClientId,
                ValidateAudience = true,

                IssuerSigningKeys = config.SigningKeys,
                ValidateLifetime = true,
            };


            SecurityToken azSecurityToken;
            try
            {
                var azClaims = tokenHandler.ValidateToken(token, validationParams, out azSecurityToken);
                if (null == azClaims)
                {
                    throw new Exception("Translation.InvalidToken");
                }
            }
            catch (Exception e)
            {
                return AuthResponse.Failure(new[] {e.Message});
            }


            if (!user.Email.ToLower().Equals((azSecurityToken as JwtSecurityToken)?.Payload["email"].ToString()?.ToLower()))
            {
                return AuthResponse.Failure(new[] {"Wrong email in token"});
            }

            return await GenerateAuthenticationResultForUserAsync(user);
        }

        public Task<Result> LogoutAsync(Guid userId)
        {
            return LogoutAsync(userId, null);
        }

        public async Task<Result> LogoutAsync(Guid userId, string jti)
        {
            var user = await userManager.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id.Equals(userId));
            if (null == user)
            {
                return Result.Failure(new[] {"Translation.NotFoundException"});
            }

            var affectedRefreshTokens = user.RefreshTokens.Where(rt => !(rt.Invalidated || rt.Used));
            if (!string.IsNullOrWhiteSpace(jti))
            {
                affectedRefreshTokens = affectedRefreshTokens.Where(rt => rt.JwtTokenId.Equals(jti));
            }

            foreach (var userRefreshToken in affectedRefreshTokens)
            {
                userRefreshToken.Invalidated = true;

                logger.LogTrace($"Invalidated refresh token: {userRefreshToken.Id} {userRefreshToken.Token}");
            }

            await identityDbContext.SaveChangesAsync();
            return Result.Success();
        }

        /// <summary>
        ///     Generates the Token for the given user. Saves the refresh token in the database
        /// </summary>
        /// <param name="user">the user the token should be generated for</param>
        /// <returns>The AuthResponse with the token</returns>
        private async Task<AuthResponse> GenerateAuthenticationResultForUserAsync(AppUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
            var singingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.Add(jwtSettings.TokenLifetime.ToTimeSpan());
            var claims = await ValidClaimsAsync(user);

            var token = new JwtSecurityToken(
                jwtSettings.Issuer,
                jwtSettings.Audience,
                claims,
                expires: expiry,
                signingCredentials: singingCredentials
            );

            var refreshToken = new RefreshToken
            {
                JwtTokenId = token.Id,
                Token = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Expires = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(90)),
            };

            await identityDbContext.RefreshTokens.AddAsync(refreshToken);
            await identityDbContext.SaveChangesAsync();

            return AuthResponse.Success(new TokenDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken.Token,
            });
        }

        private async Task<IEnumerable<Claim>> ValidClaimsAsync(AppUser user)
        {
            var identityOptions = new IdentityOptions();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(identityOptions.ClaimsIdentity.UserIdClaimType, user.Id.ToString()),
                new Claim(identityOptions.ClaimsIdentity.UserNameClaimType, user.UserName),
            };
           
            /*
            var companyType = await dbContext.Users.Where(u => u.Id.Equals(user.Id)).Include(u => u.Company).Select(u => u.Company.Type).FirstOrDefaultAsync();
            switch (companyType)
            {
                case CompanyType.Customer:
                    claims.Add(new Claim(CustomClaimTypes.PermissionClaimType, Permissions.Customer));
                    break;
                case CompanyType.Internal:
                    claims.Add(new Claim(CustomClaimTypes.PermissionClaimType, Permissions.Internal));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            */

            var userClaims = await userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);
            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await roleManager.FindByNameAsync(userRole);
                if (role == null)
                {
                    continue;
                }

                var roleClaims = await roleManager.GetClaimsAsync(role);
                claims.AddRange(roleClaims);
            }

            return claims;
        }

        private ClaimsPrincipal PrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var modifiedTokenValidationParameters = tokenValidationParameters.Clone();
                modifiedTokenValidationParameters.ValidateLifetime = false;
                var principal = tokenHandler.ValidateToken(token, modifiedTokenValidationParameters, out var validatedToken);
                if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }


        private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return validatedToken is JwtSecurityToken jwtSecurityToken &&
                   jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                       StringComparison.InvariantCultureIgnoreCase);
        }
    }
}