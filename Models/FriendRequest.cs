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
        public int FromId { get; set; }
        [Required]
        [ForeignKey("User")]
        public int ToId { get; set; }
        [Required]
        [Column(TypeName = "DateTime2")]
        public DateTime DateSent { get; set; }
    }
}
