using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace OCB_API.Models
{
    public class Gift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime DateFrom { get; set; }
        public int Value { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        public bool IsHot { get; set; }
    }
}
