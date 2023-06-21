using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accountable.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        [Required]
        [Column(TypeName = "int")]
        public int Likes { get; set; }
        [Required]
        [Column(TypeName = "blob")]
        [MaxLength(500)]
        public byte[]? Content { get; set; }
        [Column(TypeName = "nvarchar(MAX)")]
        public string? PostPhoto1 { get; set; }
        [Column(TypeName = "nvarchar(MAX)")]
        public string? PostPhoto2 { get; set; }
        [Column(TypeName = "nvarchar(MAX)")]
        public string? PostPhoto3 { get; set; }
        [Required]
        [Column(TypeName = "DateTime2")]
        public DateTime CreatedAt { get; set; }
    }
}
