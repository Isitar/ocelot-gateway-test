namespace Auth
{
    using System.Linq;
    using System.Reflection;
    using Identity;
    using Identity.Entities;
    using Identity.Initialization;
    using Identity.Services.TokenService;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjection
    {
        public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppIdentityDbContext>(
                options =>
                    options
                        .UseLazyLoadingProxies()
                        .UseNpgsql(configuration.GetConnectionString("AppIdentityDbConnection"),
                            o => o
                                .MigrationsAssembly(Assembly.GetAssembly(typeof(DependencyInjection))!.FullName)
                                .UseNodaTime()
                        )
            );

            services.AddScoped<ITokenService, TokenService>();

            services.AddIdentity<AppUser, AppRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 8;

                    options.User.AllowedUserNameCharacters = "0123456789äöüÄÖÜabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-._@";
                })
                .AddEntityFrameworkStores<AppIdentityDbContext>();

            var initialAzureAdConfigOptions = new InitialAzureAdConfigOptions();
            configuration.Bind(nameof(InitialAzureAdConfigOptions), initialAzureAdConfigOptions);
            services.AddSingleton(initialAzureAdConfigOptions);

            // add initializers
            foreach (var type in Assembly.GetExecutingAssembly()
                .DefinedTypes.Where(x => x.IsClass
                                         && !x.IsAbstract
                                         && x.GetInterfaces().Any(i =>
                                             i == typeof(IInitializationStep))
                ))
            {
                foreach (var implementedInterface in type.ImplementedInterfaces)
                {
                    services.AddTransient(implementedInterface, type);
                }
            }


            services.AddTransient<Initializer>();

            

            // add httpclient for tokenservice
            services.AddHttpClient<ITokenService, TokenService>();
            return services;
        }
    }
}