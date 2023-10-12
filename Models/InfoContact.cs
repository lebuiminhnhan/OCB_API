using System.ComponentModel.DataAnnotations;

namespace OCB_API.Models
{
    public class InfoContact
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Email { get; set; }

        public string? Subject { get; set; }

        public string? Message { get; set; }
    }
}
