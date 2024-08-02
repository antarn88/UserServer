using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserServer.Data;
using UserServer.DTOs;
using UserServer.Models;

namespace UserServer.Services
{
    public class AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        public AuthResponse Login(string email, string password)
        {
            User? user = _context.Users.SingleOrDefault(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            string? token = GenerateJwtToken(user);

            return new AuthResponse
            {
                AccessToken = token,
                LoggedInUser = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Age = user.Age
                }
            };
        }

        private string GenerateJwtToken(User user)
        {
            string? jwtKey = _configuration["Jwt:Key"];
            string? jwtIssuer = _configuration["Jwt:Issuer"];
            string? jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("JWT configuration is missing.");
            }

            SymmetricSecurityKey? securityKey = new(Encoding.UTF8.GetBytes(jwtKey));
            SigningCredentials? credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            Claim[]? claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ];

            JwtSecurityToken? token = new(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(3), // Token érvényességi ideje: 3 nap
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
