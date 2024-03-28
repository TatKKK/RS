using Microsoft.IdentityModel.Tokens;
using RS.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RS.Auth
{
    public interface IJwtManager
    {
        Token GetToken(User user);
    }
    public class JwtManager : IJwtManager
    {
        private readonly IConfiguration config;

        public JwtManager(IConfiguration config)
        {
            this.config = config;
        }

        public Token GetToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(config["JWT:key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.Fname),
            new Claim(ClaimTypes.Surname, user.Lname),
            new Claim(ClaimTypes.Role, user.Discriminator),
            new Claim("ImageUrl", user.ImageUrl ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenData = tokenHandler.CreateToken(tokenDescriptor);
            var token = new Token { AccessToken = tokenHandler.WriteToken(tokenData) };
            return token;
        }
    }
    public class Token
    {
        public string? AccessToken { get; set; }

    }
}
