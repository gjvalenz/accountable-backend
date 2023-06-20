using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accountable.Models
{
    public class FriendRequest
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("User")]
        public int FromUserId { get; set; }
        [Required]
        [ForeignKey("User")]
        public int ToUserId { get; set; }
        [Required]
        [Column(TypeName = "DateTime2")]
        public DateTime DateSent { get; set; }
    }
}
