using Accountable.Models;

namespace Accountable.DataStructures.ResponseRequestData
{
    public class UserContext
    {
        public string? Name { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime Registered { get; set; }
        public string? Email { get; set; }
    }

    public class UserView
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ProfilePicture { get; set; }
        public int NumFriends { get; set; }
        public DateTime FriendRequestSent { get; set; }

        public static UserView FromUser(User u)
        {
            return new UserView
            {
                Id = u.Id,
                Name = u.Name,
                ProfilePicture = u.ProfilePicture,
                NumFriends = u.NumFriends
            };
        }
    }

    public class UserVerboseView : User
    {
        public bool Friend { get; set; }
        public bool SentFr { get; set; }
        public bool ReceivedFr { get; set; }
        public UserVerboseView(User other) : base(other) { }
        public static UserVerboseView FromUser(User u, bool isFriend, bool sentFrTo, bool receivedFrFrom)
        {
            UserVerboseView uv = new UserVerboseView(u);
            uv.Friend = isFriend;
            uv.SentFr = sentFrTo;
            uv.ReceivedFr = receivedFrFrom;
            return uv;
        }
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
        public int Weight { get; set; }
        public int Height { get; set; }
        public string? About { get; set; }
        public char Gender { get; set; }
        public IFormFile? Photo { get; set; }
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
        public string? Username { get; set; }
        public string? UserPhoto { get; set; }
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
        public string? Content { get; set; }
        public IFormFile? Photo1 { get; set; }
        public IFormFile? Photo2 { get; set; }
        public IFormFile? Photo3 { get; set; }
    }

    public class CreateComment
    {
        public string? Content { get; set; }
        public int PostOrCommentId { get; set; }
        public int ReplyKind { get; set; }
    }

    public class PostLiked
    {
        public bool Liked { get; set; }
    }

    public class FriendRequestResult
    {
        public bool Friended { get; set; }
    }

    public class NotificationInformation
    {
        public int Id { get; set; }
        public string? Kind { get; set; }
        public byte[]? Content { get; set; }
        public int FromUserId { get; set; }
        public string? FromUsername { get; set; }
        public string? FromProfilePicture { get; set; }
        public int KeyTo { get; set; }
        public bool Read { get; set; }
        public DateTime TimeSent { get; set; }
    }

    public class PhotoInformation
    {
        public string? Url { get; set; }
    }

    public class ImageUpload
    {
        public IFormFile? Image { get; set; }
    }

    public class CreateMessage
    {
        public string? Content { get; set; }
        public int ToUserId { get; set; }
    }
}