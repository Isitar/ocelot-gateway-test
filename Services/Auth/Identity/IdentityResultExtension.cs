namespace Auth.Identity
{
    using System.Linq;
    using Microsoft.AspNetCore.Identity;

    public static class IdentityResultExtension
    {
        public static Result ToResult(this IdentityResult identityResult)
        {
            return identityResult.Succeeded ? Result.Success() : Result.Failure(identityResult.Errors.Select(e => e.Description));
        }
    }
}