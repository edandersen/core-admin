using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetEd.CoreAdmin.DemoApp.Models
{
    public class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> contextOptions) : base(contextOptions)
        {
            this.Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var seedData = new List<TestEntity>();
            foreach(var i in Enumerable.Range(0, 2000))
            {
                seedData.Add(new TestEntity() { Id = Guid.NewGuid(), Name = "Test entity " + i, Title = "Test title " + i, Price = new Random().NextDouble() });
            }

            modelBuilder.Entity<TestEntity>().HasData(seedData);
        }
    }
}
