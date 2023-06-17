using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Accountable.Models
{
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string? Email { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string? HashPass { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string? ConfirmationCode { get; set; }
        [Required]
        [Column(TypeName = "bit")]
        public bool Confirmed { get; set; }
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string? AuthorizationToken { get; set; }

        [NotMapped]
        public virtual User? User { get; set; }
    }
}
