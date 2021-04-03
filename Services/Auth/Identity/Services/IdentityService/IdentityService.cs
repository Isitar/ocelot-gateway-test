namespace Auth.Identity.Services.IdentityService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using PermissionLib;
    using PermissionLib.Claims;
    using Security;

    public class IdentityService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<AppRole> roleManager;
        private readonly AppIdentityDbContext identityDbContext;

        public IdentityService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, AppIdentityDbContext identityDbContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.identityDbContext = identityDbContext;
        }

        public bool IsInitializingApplication { get; set; }

        public async Task<Result> CreateUserAsync(Guid userId, string username, string email, string password = null)
        {
            var user = new AppUser
            {
                UserName = username,
                Email = email,
                Id = userId,
            };
            var result = await userManager.CreateAsync(user, password);
            return result.ToResult();
        }

        public async Task<Result> DeleteUserAsync(Guid userId)
        {
            var user = await userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                return await DeleteUserAsync(user);
            }

            return Result.Success();
        }

        public async Task<Result> DeleteUserAsync(AppUser user)
        {
            var result = await userManager.DeleteAsync(user);

            return result.ToResult();
        }

        public async Task<Result<string>> UsernameByGuidAsync(Guid userId)
        {
            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result<string>.Failure(userResult.Errors);
            }

            var user = userResult.Data;

            return Result<string>.Success(user.UserName);
        }

        public async Task<Result<AuthenticationType>> AuthenticationTypeFromUserAsync(Guid userId)
        {
            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result<AuthenticationType>.Failure(userResult.Errors);
            }

            var domain = userResult.Data.Email.Split("@")[1];
            var azConfig = await identityDbContext.AzureAdConfigs
                .Where(azAdConfig => azAdConfig.Domain.ToLower().Equals(domain.ToLower(CultureInfo.InvariantCulture)))
                .FirstOrDefaultAsync();
            if (null == azConfig)
            {
                return Result<AuthenticationType>.Success(new PasswordAuthenticationType());
            }

            return Result<AuthenticationType>.Success(new AzureAuthenticationType(azConfig));
        }

        public async Task<Result<AuthenticationType>> AuthenticationTypeFromUserAsync(string username)
        {
            var user = await userManager.FindByEmailAsync(username);
            if (null == user)
            {
                return Result<AuthenticationType>.Failure(new[]
                {
                    @" Translation.NotFoundException
                               .Replace(""{name}"", Translation.User)
                               .Replace(""{key}"", username),"
                });
            }

            return await AuthenticationTypeFromUserAsync(user.Id);
        }

        public async Task<Result> SetPasswordAsync(Guid userId, string password)
        {
            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result.Failure(userResult.Errors);
            }

            var user = userResult.Data;

            await userManager.RemovePasswordAsync(user);
            if (null == password)
            {
                userManager.PasswordValidators.Clear();
            }

            var setPasswordResult = await userManager.AddPasswordAsync(user, password);
            return setPasswordResult.Succeeded ? Result.Success() : Result.Failure(setPasswordResult.Errors.Select(e => e.Description));
        }

        public async Task<Result> SetUsernameAsync(Guid userId, string username)
        {
            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result.Failure(userResult.Errors);
            }

            var user = userResult.Data;

            user.UserName = username;
            user.Email = username;
            await userManager.UpdateAsync(user);
            return Result.Success();
        }

        public async Task<Result> SetEmailAsync(Guid userId, string email)
        {
            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result.Failure(userResult.Errors);
            }

            var user = userResult.Data;
            user.Email = email;
            var res = await userManager.UpdateAsync(user);
            return res.ToResult();
        }

        private async Task<Result<bool>> CanAsync(Guid userId, string permission)
        {
            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result<bool>.Failure(userResult.Errors);
            }

            var user = userResult.Data;
            var claims = (await userManager.GetClaimsAsync(user)).ToList();
            foreach (var roleName in await userManager.GetRolesAsync(user))
            {
                var role = await roleManager.FindByNameAsync(roleName);
                claims.AddRange(await roleManager.GetClaimsAsync(role));
            }

            // var companyType = await dbContext.Users.Where(u => u.Id.Equals(userId)).Include(u => u.Company).Select(u => u.Company.Type).FirstOrDefaultAsync();
            // switch (companyType)
            // {
            //     case CompanyType.Customer:
            //         claims.Add(new Claim(CustomClaimTypes.PermissionClaimType, Permissions.Customer));
            //         break;
            //     case CompanyType.Internal:
            //         claims.Add(new Claim(CustomClaimTypes.PermissionClaimType, Permissions.Internal));
            //         break;
            //     case CompanyType.External:
            //         claims.Add(new Claim(CustomClaimTypes.PermissionClaimType, Permissions.External));
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }

            return Result<bool>.Success(claims.Any(c => c.Type.Equals(CustomClaimTypes.PermissionClaimType) && c.Value.Equals(permission)));
        }

        public async Task<bool> CanAndSuccessfulAsync(Guid? userId, string permission)
        {
            if (!userId.HasValue)
            {
                return false;
            }

            var can = await CanAsync(userId.Value, permission);
            return can.Successful && can.Data;
        }

        public async Task<Result> AssignRoleAsync(Guid userId, string roleName)
        {
            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result<bool>.Failure(userResult.Errors);
            }

            var assignableRoles = new[] {RoleNames.Admin};
            if (!assignableRoles.Contains(roleName))
            {
                return Result.Failure(new[] {"Translation.CannotAssignRole"});
            }

            var isCustomerResult = await CanAsync(userId, Permissions.Customer);
            if (isCustomerResult.Successful && isCustomerResult.Data)
            {
                // customers cannot have any roles
                return Result<bool>.Failure(new[] {"Translation.CannotAssignRoleToCustomers"});
            }

            var user = userResult.Data;
            var res = await userManager.AddToRoleAsync(user, roleName);
            return res.ToResult();
        }

        public async Task<Result> RevokeRoleAsync(Guid userId, string roleName)
        {
            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result<bool>.Failure(userResult.Errors);
            }

            var user = userResult.Data;

            var res = await userManager.RemoveFromRoleAsync(user, roleName);
            return res.ToResult();
        }

        public async Task<Result<IEnumerable<string>>> RolesAsync(Guid userId)
        {
            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result<IEnumerable<string>>.Failure(userResult.Errors);
            }

            var user = userResult.Data;
            var roles = await userManager.GetRolesAsync(user);


            return Result<IEnumerable<string>>.Success(roles);
        }

        public async Task<Result> AssignPermissionAsync(Guid userId, string permissionName)
        {
            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result<bool>.Failure(userResult.Errors);
            }

            var user = userResult.Data;

            var res = await userManager.AddClaimAsync(user, new Claim(CustomClaimTypes.PermissionClaimType, permissionName));
            return res.ToResult();
        }

        public async Task<Result> RevokePermissionAsync(Guid userId, string permissionName)
        {
            // check auth


            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result<bool>.Failure(userResult.Errors);
            }

            var user = userResult.Data;
            var claimToRemove = (await userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type.Equals(CustomClaimTypes.PermissionClaimType) && c.Value.Equals(permissionName));
            var res = await userManager.RemoveClaimAsync(user, claimToRemove);
            return res.ToResult();
        }

        public async Task<Result<IEnumerable<string>>> PermissionsAsync(Guid userId)
        {
            var userResult = await FindUserAsync(userId);
            if (!userResult.Successful)
            {
                return Result<IEnumerable<string>>.Failure(userResult.Errors);
            }

            var user = userResult.Data;
            var claims = await userManager.GetClaimsAsync(user);
            return Result<IEnumerable<string>>.Success(claims.Select(c => c.Value));
        }

        public Task<Result<string>> GenerateRandomPasswordAsync()
        {
            return Task.FromResult(Result<string>.Success(Password.Generate(12)));
        }


        private async Task<Result<AppUser>> FindUserAsync(Guid userId)
        {
            var user = await userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (null == user)
            {
                return Result<AppUser>.Failure(new[]
                {
                    @"Translation.NotFoundException
                               .Replace(""{name}"", Translation.User)
                               .Replace(""{key}"", userId.ToString())",
                });
            }

            return Result<AppUser>.Success(user);
        }
    }
}