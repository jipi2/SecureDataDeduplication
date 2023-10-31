using FileStorageApp.Server.Entity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FileStorageApp.Server.SecurityFolder
{
    public class SecurityManager 
    {
        private readonly IConfiguration _configuration;
        public SecurityManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GetNewJwt(User user)
        {

            var secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("Secret").Value));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email), // NOTE: this will be the "User.Identity.Name" value to retrieve the user name on the server side
				new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, user.Email),
                new Claim(ClaimTypes.Role, user.Roles[0].RoleName)
            };

            var token = new JwtSecurityToken(issuer: "fileStorage.com", audience: "fileStorage.com", claims: claims, expires: DateTime.Now.AddMinutes(60), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
