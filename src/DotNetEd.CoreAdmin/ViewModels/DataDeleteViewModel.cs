namespace DotNetEd.CoreAdmin.ViewModels
{
    public class DataDeleteViewModel
    {
        public string DbSetName {get;set;}
        public string Id {get;set;}
        public object Object { get; internal set; }
    }
}