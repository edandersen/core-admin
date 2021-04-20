using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetEd.CoreAdmin.DemoApp.Models
{
    public class TestEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Display(Name = "Enum name")]
        public TestEnum EnumName { get; set; }
    }

    public enum TestEnum
    {
        Alice = 0,
        Bob = 1,
        Gary = 2,
        Nigel = 3,
        Ian = 4
    }
}
