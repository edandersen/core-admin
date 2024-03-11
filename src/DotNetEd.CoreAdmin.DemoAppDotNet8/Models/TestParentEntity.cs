using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetEd.CoreAdmin.DemoAppDotNet6.Models
{
    public class TestParentEntity
    {
        [Display(AutoGenerateField = false)]
        [Key]
        public Guid ParentId { get; set; }

        [Display(AutoGenerateField = false)]
        public Guid? ChildId { get; set; }

        [ForeignKey("ChildId")]
        public TestChildEntity? Child { get; set; }
    }
}
