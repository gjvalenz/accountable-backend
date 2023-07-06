using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Accountable.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        [Required]
        [ForeignKey("User")]
        public int ToUserId { get; set; }
        [Required]
        [Column(TypeName = "varbinary(500)")]
        [MaxLength(500)]
        public byte[]? Content { get; set; }
        [Required]
        [Column(TypeName = "DateTime2")]
        public DateTime SentAt { get; set; }
        [Required]
        [Column(TypeName = "bit")]
        public bool Read { get; set; }
    }
}
