using UserServer.DTOs;
using UserServer.Models;

namespace UserServer.Interfaces
{
    public interface IUserRepository
    {
        Task<PagedResult<UserDto>> GetPagedUsers(int page, int perPage, string sort);
        Task<UserDto?> GetUserDtoById(Guid id);
        Task<UserDto?> GetUserDtoByEmail(string email);
        Task<User?> GetUserByEmail(string email);
        Task<User> CreateUser(CreateUserRequest request);
        Task<User?> UpdateUser(Guid id, UpdateUserRequest request);
        Task<bool> DeleteUser(Guid id);
    }
}
