using UserServer.DTOs;
using UserServer.Models;

namespace UserServer.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(string email, string password);
        string GenerateJwtToken(User user);
    }
}
