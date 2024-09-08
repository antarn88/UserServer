namespace UserServer.DTOs
{
    public class CreateUserRequest
    {
        public string Name { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public int Age { get; set; }
        public string Password { get; set; } = String.Empty;
    }
}
