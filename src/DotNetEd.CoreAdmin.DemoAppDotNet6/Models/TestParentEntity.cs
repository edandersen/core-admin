using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetEd.CoreAdmin.DemoAppDotNet6.Models
{
    public class TestParentEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ChildId { get; set; }

        [ForeignKey("ChildId")]
        public TestChildEntity Child { get; set; }
    }
}
