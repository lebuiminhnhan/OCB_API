using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCB_API.Models
{
    public class UserGift
    {
        [Key]
        public int ID { get; set; }

        // Foreign key to User table
        [ForeignKey("User")]
        public int UserId { get; set; }

        // Navigation property to User table
        public User? User { get; set; }

        // Foreign key to Gift table
        [ForeignKey("Gift")]
        public int GiftId { get; set; }

        // Navigation property to Gift table
        public Gift? Gift { get; set; }
    }
}
