using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accountable.Models
{
    // note: when weight entry is added, it should auto update our profile weight
    public class WeightEntry
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        [Required]
        [Column(TypeName = "int")]
        public int Weight { get; set; } // lbs for american audience
        [Required]
        [Column(TypeName = "DateTime2")]
        public DateTime DateEntered { get; set; }
    }
}
