using UserServer.DTOs;
using UserServer.Models;

namespace UserServer.Interfaces
{
    public interface IUserService
    {
        Task<Models.PagedResult<UserDto>> GetPagedUsers(int page, int perPage, string sort);
        Task<UserDto?> GetUserById(Guid id);
        Task<UserDto?> GetUserByEmail(string email);
        Task<User> CreateUser(CreateUserRequest request);
        Task<User?> UpdateUser(Guid id, UpdateUserRequest request);
        Task<bool> DeleteUser(Guid id);
        Task<byte[]> ExportUsersToExcel(int page, int perPage, string sort);
    }
}
