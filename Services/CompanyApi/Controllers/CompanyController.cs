namespace CompanyApi.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("/api/v1/company")]
    public class CompanyController : ControllerBase
    {
        private static readonly List<string> Companies = new()
        {
            "firma 1",
            "firma 2",
            "firma 3",
        };

        [HttpGet]
        public IActionResult AllCompanies()
        {
            return Ok(Companies);
        }

        [HttpGet("{id:int}/user")]
        public IActionResult AllCompanyUsers(int id)
        {
            var entry = HttpContext.User.Claims.Select(c => $"{c.Type}: {c.Value}");
            entry = entry.Concat(new[] {HttpContext.User.Identity?.Name});
            entry = entry.Concat(new[] {HttpContext.User?.FindFirstValue(new IdentityOptions().ClaimsIdentity.UserIdClaimType)});
            return Ok(entry);

            return Ok(new List<string>()
            {
                "hans", "fritz"
            });
        }
    }
}