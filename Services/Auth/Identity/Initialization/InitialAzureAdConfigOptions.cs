namespace Auth.Identity.Initialization
{
    using System.Collections.Generic;

    public class AzureAdConfigOptionEntry
    {
        public string Domain { get; set; }
        public string ClientId { get; set; }
    }
    
    public class InitialAzureAdConfigOptions
    {
        public ICollection<AzureAdConfigOptionEntry> Entries { get; set; } = new HashSet<AzureAdConfigOptionEntry>();
    }
}