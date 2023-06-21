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

    public class PostInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Likes { get; set; }
        public byte[]? Content { get; set; }
        public string? PostPhoto1 { get; set; }
        public string? PostPhoto2 { get; set; }
        public string? PostPhoto3 { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Liked { get; set; }
    }

    public class CreatePost
    {
        public byte[]? Content { get; set; }
        public string[]? PhotoUrls { get; set; }
    }
}