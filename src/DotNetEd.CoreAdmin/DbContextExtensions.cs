using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetEd.CoreAdmin
{
    public static class DbContextExtensions
    {
        public static IQueryable<object> Set(this DbContext _context, Type t)
        {
           var methods =  _context.GetType().GetMethods();
            foreach (var method in methods.Where(p=>p.Name == "Set"))
            {
                if(!method.GetParameters().Any())
                return    (IQueryable<object>)method.MakeGenericMethod(t).Invoke(_context, null);
            }
            return (IQueryable<object>)_context.GetType().GetMethod("Set").MakeGenericMethod(t).Invoke(_context, null);
        }
    }
}
