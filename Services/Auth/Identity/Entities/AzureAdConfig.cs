namespace Auth.Identity.Entities
{
    using System;

    public class AzureAdConfig
    {
        public Guid Id { get; set; }
        public string Domain { get; set; }
        public string ClientId { get; set; }
    }
}