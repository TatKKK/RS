using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RS.Models;
using System.Security.Claims;

namespace RS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        protected User? AuthUser
        {
            get
            {
                User? user = null;
                var currentUser = HttpContext.User;

                if (currentUser != null && currentUser.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                {
                    user = new User();
                    user.Id = int.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
                    user.Discriminator = currentUser.FindFirst(ClaimTypes.Role)?.Value;
                }
                return user;
            }
        }

    }
}
