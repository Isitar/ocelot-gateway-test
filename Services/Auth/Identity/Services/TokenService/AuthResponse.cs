namespace Auth.Identity.Services.TokenService
{
    using System.Collections.Generic;

    public class AuthResponse : Result<TokenDto>
    {
        public AuthResponse(bool successful, IEnumerable<string> errors) : base(successful, errors)
        {
        }

        public AuthResponse(bool successful, IEnumerable<string> errors, TokenDto data) : base(successful, errors, data)
        {
        }


        public new static AuthResponse Success(TokenDto data)
        {
            return new AuthResponse(true, new string[0], data);
        }

        public new static AuthResponse Failure(IEnumerable<string> errors)
        {
            return new AuthResponse(false, errors);
        }
    }
}