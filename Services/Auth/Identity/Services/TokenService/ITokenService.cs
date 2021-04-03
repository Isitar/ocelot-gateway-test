namespace Auth.Identity.Services.TokenService
{
    using System;
    using System.Threading.Tasks;

    public interface ITokenService
    {
        /// <summary>
        ///     Generates a new AuthResponse with the given refreshToken and jwtToken
        /// </summary>
        /// <param name="refreshTokenString">the refresh token</param>
        /// <param name="jwtToken">the jwt token</param>
        /// <returns>A new AuthResponse containing either the tokens or error messages</returns>
        Task<AuthResponse> RefreshAsync(string refreshTokenString, string jwtToken);

        /// <summary>
        ///     normal user loging using username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<AuthResponse> LoginAsync(string username, string password);

        /// <summary>
        ///     normal user loging using username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="token">the azure token</param>
        /// <returns></returns>
        Task<AuthResponse> LoginAzureAsync(string username, string token);

        Task<Result> LogoutAsync(Guid userId);
        Task<Result> LogoutAsync(Guid userId, string jti);
    }
}