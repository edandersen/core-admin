namespace DotNetEd.CoreAdmin.ViewModels
{
	public class VerificationModel
	{
		public string Token { get; set; }
		public string Code { get; set; }
		public string MFACode { get; set; }
		public bool RememberMe { get; set; }
	}
}
