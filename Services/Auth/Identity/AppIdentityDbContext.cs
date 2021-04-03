namespace Auth.Identity
{
    using System;
    using Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using PermissionLib;
    using PermissionLib.Claims;

    public class AppIdentityDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options) { }


        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AzureAdConfig> AzureAdConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppIdentityDbContext).Assembly);
            var adminRoleGuid = Guid.Parse("79690AED-86B2-4480-A726-9FE5F0D8B35B");

            var internalRoleGuid = Guid.Parse("00DCFEB1-8499-402E-BEEE-606078708677");
            var customerRoleGuid = Guid.Parse("BDC320AC-E6E8-48C2-B21C-C3300D6B65F8");

            builder.Entity<AppRole>()
                .HasData(new AppRole
                    {
                        Id = adminRoleGuid,
                        Name = RoleNames.Admin,
                        ConcurrencyStamp = adminRoleGuid.ToString(),
                        NormalizedName = RoleNames.Admin.ToUpper(),
                    },
                    new AppRole
                    {
                        Id = customerRoleGuid,
                        Name = RoleNames.Customer,
                        ConcurrencyStamp = customerRoleGuid.ToString(),
                        NormalizedName = RoleNames.Customer.ToUpper(),
                    }, new AppRole
                    {
                        Id = internalRoleGuid,
                        Name = RoleNames.Internal,
                        ConcurrencyStamp = internalRoleGuid.ToString(),
                        NormalizedName = RoleNames.Internal.ToUpper(),
                    }
                );

            var i = 0;
            builder.Entity<IdentityRoleClaim<Guid>>()
                .HasData(new IdentityRoleClaim<Guid>
                    {
                        Id = ++i,
                        RoleId = adminRoleGuid,
                        ClaimType = CustomClaimTypes.PermissionClaimType,
                        ClaimValue = Permissions.Admin,
                    },
                    new IdentityRoleClaim<Guid>
                    {
                        Id = ++i,
                        RoleId = adminRoleGuid,
                        ClaimType = CustomClaimTypes.PermissionClaimType,
                        ClaimValue = Permissions.MasterData,
                    },
                    new IdentityRoleClaim<Guid>
                    {
                        Id = ++i,
                        RoleId = adminRoleGuid,
                        ClaimType = CustomClaimTypes.PermissionClaimType,
                        ClaimValue = Permissions.TaskBoard,
                    }
                );
        }
    }
}