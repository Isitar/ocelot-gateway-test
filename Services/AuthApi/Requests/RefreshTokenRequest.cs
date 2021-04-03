namespace AuthApi.Requests
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
        public string JwtToken { get; set; }
    }
}