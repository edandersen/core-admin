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

        [MaxLength(100)]
        public string? Name { get; set; }

        [Required]

        [MaxLength(100)]
        public string? Title { get; set; }

        [DataType(DataType.MultilineText)]

        [MaxLength(300)]
        public string? Description { get; set; }

        [DataType("Markdown")]
        [MaxLength(3000)]
        public string? Body { get; set; }

        [Display(Name = "Enum name")]
        public TestEnum EnumName { get; set; }

        public double Price { get; set; }

        public DateTime DateTime { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public DateOnly DateOnly { get; set; }
        public TimeOnly TimeOnly { get; set; }
        public TimeSpan TimeSpan { get; set; }

        public DateTimeOffset DateTimeOffset { get; set; }

        public bool IsUseful { get; set; }

        public decimal DecimalValue { get; set; }

        public decimal? NullableDecimalValue {get;set;}
        public DateTime? NullableDateTimeValue { get; set; }
        public int? NullableIntValue { get; set; }
        public long? NullableLongValue { get; set; }
        public long LongValue { get; set; }
        public double DoubleValue {get;set;}
        public double? NullableDoubleValue {get;set;}
        public DateTimeOffset? NullableDateTimeOffsetValue { get; set; }
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
