using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCB_API.Models
{
    public class UserLogin
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string? UserName { get; set; }
        public string? Password { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }

        public string? RoleUser { get; set; }

        public User? User { get; set; }
    }

    public class Login
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
