using System.ComponentModel.DataAnnotations;

namespace DotNetEd.CoreAdmin.DemoAppDotNet6.Models
{
    public class TestEntityWithImage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public byte[] Image { get; set; }
    }
}
