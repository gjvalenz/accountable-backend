using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

// still need notifications (maybe hard)
// - notifications controller can handle all types of notifications including 
// - like notification, friend notification (accepted fr), friend request notification
// - comment notification, (message notification)?

namespace Accountable.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public string? Kind { get; set; }
        [Required]
        [Column(TypeName = "DateTime2")]
        public DateTime TimeSent { get; set; }
        [Required]
        public int KeyTo { get; set; }
        [Required]
        [ForeignKey("User")]
        public int To { get; set; }
        [Required]
        [ForeignKey("User")]
        public int From { get; set; }
        [Required]
        [Column(TypeName = "bit")]
        public bool Read { get; set; }

        public static string[] Kinds = { "Message", "Like", "FriendAccepted", "FriendRequest", "CommentPost", "CommentSub" };

    }
}
