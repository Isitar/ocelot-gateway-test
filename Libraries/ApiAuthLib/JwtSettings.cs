namespace ApiAuthLib
{
    using NodaTime;

    public class JwtSettings
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string Secret { get; set; }

        public Duration TokenLifetime { get; set; }
    }
}