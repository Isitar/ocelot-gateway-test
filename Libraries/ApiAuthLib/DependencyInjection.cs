namespace ApiAuthLib
{
    using System;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using PermissionLib;
    using PermissionLib.Claims;

    public static class DependencyInjection
    {
        private static IServiceCollection AddPermissions(this IServiceCollection services)
        {
            // add policy for permissions
            return services.AddAuthorization(options =>
            {
                foreach (var prop in typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                {
                    var propertyValue = prop.GetValue(null)?.ToString();
                    options.AddPolicy(propertyValue, policy => policy.RequireClaim(CustomClaimTypes.PermissionClaimType, propertyValue));
                }
            });
        }

        private static IServiceCollection AddJwtSettings(this IServiceCollection services, IConfiguration configuration, out JwtSettings jwtSettings)
        {
            jwtSettings = new JwtSettings();
            configuration.Bind(nameof(JwtSettings), jwtSettings);
            services.AddSingleton(jwtSettings);

            return services;
        }

        private static IServiceCollection AddTokenValidationParameters(this IServiceCollection services, JwtSettings jwtSettings, out TokenValidationParameters tokenValidationParameters)
        {
            tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero,
            };
            services.AddSingleton(tokenValidationParameters);
            return services;
        }

        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration, string schema = null)
        {
            services
                .AddPermissions()
                .AddJwtSettings(configuration, out var jwtSettings)
                .AddTokenValidationParameters(jwtSettings, out var tokenValidationParameters)
                .AddAuthentication(config =>
                {
                    config.DefaultScheme = schema ?? JwtBearerDefaults.AuthenticationScheme;
                    config.DefaultAuthenticateScheme = schema ?? JwtBearerDefaults.AuthenticationScheme;
                    config.DefaultChallengeScheme = schema ?? JwtBearerDefaults.AuthenticationScheme;
                })
                .AddCookie(options => { options.SlidingExpiration = true; })
                .AddJwtBearer(schema ?? JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = tokenValidationParameters;
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }

                            return Task.CompletedTask;
                        },
                    };
                });

            return services;
        }
    }
}