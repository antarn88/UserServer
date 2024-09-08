using AutoMapper;
using UserServer.DTOs;
using UserServer.Models;

namespace UserServer.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserRequest, User>();
        }
    }
}
