using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetEd.CoreAdmin.ViewModels
{
    public class MenuViewModel
    {
        public List<string> DbContextNames { get; set; } = new List<string>();
        public List<string> DbSetNames { get; set; } = new List<string>();
    }
}
