using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace DotNetEd.CoreAdmin.ViewModels
{
    public class DataListViewModel
    {
        public Type EntityType { get; internal set; }
        public IEnumerable<object> Data { get; internal set; }
        public DbContext DbContext { get; internal set; }
        public PropertyInfo DbSetProperty { get; internal set; }
    }
}
