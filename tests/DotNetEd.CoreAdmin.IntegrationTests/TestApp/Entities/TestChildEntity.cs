using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetEd.CoreAdmin.IntegrationTests.TestApp.Entities
{
    public class TestChildEntity
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        public override string ToString()
        {
            return Name ?? String.Empty;
        }

    }
}
