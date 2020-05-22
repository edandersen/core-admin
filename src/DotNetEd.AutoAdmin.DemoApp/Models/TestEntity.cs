using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetEd.AutoAdmin.DemoApp.Models
{
    public class TestEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
