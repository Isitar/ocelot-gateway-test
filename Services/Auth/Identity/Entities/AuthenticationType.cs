namespace Auth.Identity.Entities
{
    public class PasswordAuthenticationType : AuthenticationType
    {
        public override string Name => "Password";
    }

    public class AzureAuthenticationType : AuthenticationType
    {
        public override string Name => "AzureAD";


        public string ClientId { get; }

        public AzureAuthenticationType(AzureAdConfig adConfig)
        {
            ClientId = adConfig.ClientId;
        }
    }
}