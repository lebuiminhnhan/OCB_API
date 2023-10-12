using System.ComponentModel.DataAnnotations;

namespace OCB_API.Models
{
    public class InfoRegister
    {
        [Key]
        public int Id { get; set; }

        public string? Email { get; set; }
    }
}
