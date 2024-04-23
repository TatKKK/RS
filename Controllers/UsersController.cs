using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RS.Auth;
using RS.Models;
using RS.Packages;

namespace RS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : MainController
    {
        IPKG_USER package;
        
        private readonly IJwtManager jwtManager;

        public UsersController(IPKG_USER package, IJwtManager jwtManager)
        {
            this.package = package;
            this.jwtManager = jwtManager;        
        }

        [HttpPost("Authenticate")]
        public IActionResult Authenticate(Login loginData)
        {
            Token? token = null;
            User? user = null;

            try
            {
                user = package.Authenticate(loginData);
                if (user == null) return Unauthorized("Invalid login credentials");

                token = jwtManager.GetToken(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Error uploading data");
            }
            return Ok(token);
        }

        [HttpPost("AddUser")]
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> AddUser([FromForm] UserDto userDto, [FromForm] IFormFile image)
        {
          
            if (string.IsNullOrEmpty(userDto.Fname) || string.IsNullOrEmpty(userDto.Lname) || string.IsNullOrEmpty(userDto.Email)
               || string.IsNullOrEmpty(userDto.Password) || string.IsNullOrEmpty(userDto.IdNumber))
            {
                return BadRequest("Invalid input");
            }

            try
            {
                var user = new User
                {
                    Fname = userDto.Fname,
                    Lname = userDto.Lname,
                    IdNumber = userDto.IdNumber,
                    Email = userDto.Email,
                    Password = userDto.Password, 
                    Discriminator = userDto.Discriminator
                };

                if (image != null)
                {                 
                    var imageUrl = await SaveImageAsync(userDto.Image);
                }

                package.Add_user(user); 
                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Error uploading data");
            }
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return null;
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return $"/images/{fileName}";
        }

        public class UpdatePasswordModel
        {
            public string Password { get; set; }
        }

        [HttpPut("updatePassword/{email}")]
        public IActionResult UpdatePassword(string email, [FromBody] UpdatePasswordModel model)
        {
            try
            {
                package.Update_password(email, model.Password);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Error Uploading data");
            }
        }


        [HttpPut]

        public IActionResult EditUser(User user, int id)
        {
            try
            {
                package.Edit_user(user, id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Error Uploading data");
            }

        }
        [HttpDelete]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                package.Delete_user(id);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Error Uploading data");
            }
        }
        [HttpGet("Users")]
        [Authorize(Roles = "admin")]
        public IActionResult GetUsers()
        {
            List<User> users = new List<User>();
            try
            {
                users = package.Get_users();
                return Ok(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public IActionResult GetUser(int id)
        {
            try
            {
                User user = package.Get_user(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }

        [HttpGet("userby/{email}")]
        public IActionResult GetUserByEmail(string email)
        {
            try
            {
                User user = package.Get_user_byEmail(email);
                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }
    } 
}
