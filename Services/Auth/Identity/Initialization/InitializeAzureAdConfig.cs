namespace Auth.Identity.Initialization
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Entities;
    using Microsoft.EntityFrameworkCore;

    public class InitializeAzureAdConfig : IInitializationStep
    {
        private readonly AppIdentityDbContext dbContext;
        private readonly InitialAzureAdConfigOptions azureAdConfigOptions;

        public InitializeAzureAdConfig(AppIdentityDbContext dbContext, InitialAzureAdConfigOptions azureAdConfigOptions)
        {
            this.dbContext = dbContext;
            this.azureAdConfigOptions = azureAdConfigOptions;
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            if (await dbContext.Users.AnyAsync(cancellationToken))
            {
                return;
            }
            
            foreach (var azAdConfig in azureAdConfigOptions.Entries)
            {
                await dbContext.AzureAdConfigs.AddAsync(new AzureAdConfig
                {
                    Id = Guid.NewGuid(),
                    Domain = azAdConfig.Domain,
                    ClientId = azAdConfig.ClientId,
                }, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}