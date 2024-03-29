﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accountable.Models
{
    public class Friend
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("User")]
        public int UserId1 { get; set; }
        [Required]
        [ForeignKey("User")]
        public int UserId2 { get; set; }
        [Required]
        [Column(TypeName = "DateTime2")]
        public DateTime FriendsSince { get; set; }
    }
}
