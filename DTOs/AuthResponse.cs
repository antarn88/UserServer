namespace UserServer.DTOs
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = "";
        public UserDto LoggedInUser { get; set; } = new UserDto();
    }
}
