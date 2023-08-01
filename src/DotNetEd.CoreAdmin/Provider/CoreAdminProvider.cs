namespace DotNetEd.CoreAdmin.Provider
{
	public class CoreAdminProvider
	{
		private static CoreAdminProvider instance = null;
		public static CoreAdminProvider Instance => instance ??= new CoreAdminProvider();
		public string FirebaseApiKey { get; set; }
	}
}
