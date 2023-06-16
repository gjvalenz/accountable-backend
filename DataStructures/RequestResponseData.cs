namespace Accountable.DataStructures.ResponseRequestData
{
    public class UserContext
    {
        public string? Name { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime Registered { get; set; }
        public string? Email { get; set; }
    }

    public class UserIDs
    {
        public int UserID { get; set; }
        public int UserAccountID { get; set; }
    }

    public class TokenInfo
    {
        public string? AuthenticationToken { get; set; }
    }

    public class RegisterInfo
    {
        public string? Email { get; set; }
        public string? Password { get; set; }

        public string? Name { get; set; }

    }

    public class LoginInfo
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}