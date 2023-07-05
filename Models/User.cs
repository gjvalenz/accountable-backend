using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accountable.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string? Name { get; set; }
        [Column(TypeName = "nvarchar(MAX)")]
        public string? ProfilePicture { get; set; }
        [Required]
        [Column(TypeName = "DateTime2")]
        public DateTime Registered { get; set; }
        [Required]
        [Column(TypeName = "int")]
        public int Weight { get; set; } // in lbs. (american audience)
        [Required]
        [Column(TypeName = "int")]
        public int Height { get; set; } // in in.
        [Required]
        [Column(TypeName = "nvarchar(2)")]
        public char Gender { get; set; } // m/f/n, used for bmi calculator if want
        [Required]
        [Column(TypeName = "int")]
        public int NumFriends { get; set; }
        [Column(TypeName = "varbinary(250)")]
        [MaxLength(250)]
        public byte[]? About { get; set; } // small about section
        [Required]
        [Column(TypeName = "bit")]
        public bool? PrivateProgress { get; set; } // users can make their progress private (only available to them)
        public User() { }
        protected User(User other)
        {
            this.Id = other.Id;
            this.Registered = other.Registered;
            this.Weight = other.Weight;
            this.Height = other.Height;
            this.Gender = other.Gender;
            this.NumFriends = other.NumFriends;
            this.PrivateProgress = other.PrivateProgress;
            this.About = other.About;
            this.Name = other.Name;
            this.ProfilePicture = other.ProfilePicture;
        }
    }
}
