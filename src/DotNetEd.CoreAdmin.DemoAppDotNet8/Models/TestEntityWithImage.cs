using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetEd.CoreAdmin.DemoAppDotNet6.Models
{
    public class TestEntityWithImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        // [System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
        public byte[]? Image { get; set; }
    }
}
