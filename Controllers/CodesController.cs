using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using RS.Models;
using RS.Packages;
using System.Data;
using static RS.Models.ActivationCodess;

namespace RS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodesController : MainController
    {
        IPKG_CODES package;

        public CodesController(IPKG_CODES package)
        {
            this.package = package;
        }
       
        [HttpPost("CreateCode")]
        public async Task<IActionResult> CreateActivationCode([FromBody] ActivationCodeRequest request)
        {
            if (string.IsNullOrEmpty(request.UserEmail))
            {
                return BadRequest("User email is required.");
            }
            try
            {
                var activationCode = await package.CreateActivationCode(request);

                if (string.IsNullOrEmpty(activationCode))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to generate an activation code.");
                }

                return Ok(new { UserEmail = request.UserEmail, ActivationCode = activationCode });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error while generating activation code: {ex.Message}");
            }
        }


        //[HttpPost("RemoveExpiredCodes")]
        //public async Task<IActionResult> RemoveExpiredCodes()
        //{
        //    try
        //    {
        //        await package.RemoveExpiredCodesAsync();
        //        return Ok(new { Message = "Expired codes removed successfully." });
        //    }
        //    catch (System.Exception ex)
        //    {
               
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error while removing expired codes: {ex.Message}");
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> GetAllActivationCodes()
        {
            try
            {
                var activationCodes = await package.GetAllActivationCodes();
                return Ok(activationCodes);
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error while fetching codes: {ex.Message}");
            }
        }

        //[HttpGet("getCode")]
        //public IActionResult GetActivationCodeByEmail(string userEmail)
        //{
        //    try
        //    {
        //        var activationCode = package.GetActivationCodeByEmailAsync(userEmail);
        //        return Ok(activationCode);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error while fetching code: {ex.Message}");
        //    }

        //}

        [HttpGet("getCode")]
        public async Task<IActionResult> GetActivationCode(string userEmail)
        {
            try
            {
                var activationCode = await package.GetActivationCode(userEmail);
                return Ok(activationCode);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error while fetching code: {ex.Message}");
            }
        }


        [HttpPost]
        [Route("verify")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            try
            {
                bool IsValid = await package.VerifyActivationCode(request);
                return Ok(new { IsValid = IsValid });
            }
            catch (Exception ex)
            {
                 Console.WriteLine(ex.ToString());
                return StatusCode(500, "An error occurred while verifying the activation code.");
            }
        }


    }
}
