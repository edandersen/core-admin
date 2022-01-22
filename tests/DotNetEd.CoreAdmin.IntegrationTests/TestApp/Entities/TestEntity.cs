using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotNetEd.CoreAdmin.IntegrationTests.TestApp.Entities
{
    public class TestEntity
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }
    }
}
