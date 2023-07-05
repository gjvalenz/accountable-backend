using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Accountable.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        [Required]
        [Column(TypeName = "varbinary(500)")]
        [MaxLength(500)]
        public byte[]? Content { get; set; }
        [Required]
        [Column(TypeName = "DateTime2")]
        public DateTime CreatedAt { get; set; }
        /*
         * reply type can be 1 (post) or 2 (comment)
         * ONLY top-level comments can be replied to
         */
        [Required]
        public int ReplyType { get; set; }
        [Required]
        public int ReplyToKey { get; set; } // indicates whether to find post or comment
    }
}
