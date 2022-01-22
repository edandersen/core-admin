using System.Collections.Generic;

namespace DotNetEd.CoreAdmin.ViewModels
{
    public class MenuViewModel
    {
        public List<string> DbContextNames { get; set; } = new List<string>();
        public List<string> DbSetNames { get; set; } = new List<string>();
    }
}
