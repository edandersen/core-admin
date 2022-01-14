using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetEd.CoreAdmin.IntegrationTests.TestApp.Entities
{
    public class TestEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
