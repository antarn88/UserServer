namespace UserServer.DTOs
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = String.Empty;
        public UserDto LoggedInUser { get; set; } = new UserDto();
    }
}
