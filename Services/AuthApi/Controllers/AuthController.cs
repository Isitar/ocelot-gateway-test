namespace AuthApi.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Auth.Identity.Services.IdentityService;
    using Auth.Identity.Services.TokenService;
    using AuthApi;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Requests;

    public class AuthController : ApiController
    {
        private readonly ITokenService tokenService;
        private readonly IdentityService identityService;

        public AuthController(ITokenService tokenService, IdentityService identityService)
        {
            this.tokenService = tokenService;
            this.identityService = identityService;
        }

        [AllowAnonymous]
        [HttpPost(ApiRoutes.Auth.Refresh, Name = nameof(AuthController) + "/" + nameof(RefreshTokenAsync))]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var resp = await tokenService.RefreshAsync(refreshTokenRequest.RefreshToken, refreshTokenRequest.JwtToken);
            if (!resp.Successful)
            {
                return BadRequest(resp.Errors);
            }

            return Ok(resp.Data);
        }

       
        [AllowAnonymous]
        [HttpPost(ApiRoutes.Auth.Login, Name = nameof(AuthController) + "/" + nameof(TokenAsync))]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        
        public async Task<IActionResult> TokenAsync([FromBody] LoginRequest loginRequest)
        {
            var resp = await tokenService.LoginAsync(loginRequest.Username, loginRequest.Password);
            if (!resp.Successful)
            {
                return BadRequest(resp.Errors);
            }

            return Ok(resp.Data);
        }

        [AllowAnonymous]
        [HttpPost(ApiRoutes.Auth.LoginAz, Name = nameof(AuthController) + "/" + nameof(LoginAzAsync))]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        
        public async Task<IActionResult> LoginAzAsync([FromBody] LoginAzRequest loginRequest)
        {
            var resp = await tokenService.LoginAzureAsync(loginRequest.Username, loginRequest.Token);
            if (!resp.Successful)
            {
                return BadRequest(resp.Errors);
            }

            return Ok(resp.Data);
        }

        [HttpPost(ApiRoutes.Auth.Register, Name = nameof(AuthController) + "/" + nameof(RegisterAsync))]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            var res = await identityService.CreateUserAsync(Guid.NewGuid(), request.Username, request.Username, request.Password);
            if (res.Successful)
            {
                return Ok();
            }

            return BadRequest(res.Errors);
        }

        
    }
}