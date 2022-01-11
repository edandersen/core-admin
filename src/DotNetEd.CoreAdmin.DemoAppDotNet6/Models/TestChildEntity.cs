using System.ComponentModel.DataAnnotations;

namespace DotNetEd.CoreAdmin.DemoAppDotNet6.Models
{
    public class TestChildEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}
