using DotNetEd.CoreAdmin.Provider;
using DotNetEd.CoreAdmin.ViewModels;
using Firebase.Auth;
using Google.Authenticator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetEd.CoreAdmin.Controllers
{
	public class AccountController : Controller
	{
		private readonly CoreAdminOptions options;
		private readonly ILogger<AccountController> logger;

		public AccountController(CoreAdminOptions options, ILogger<AccountController> logger)
		{
			this.logger = logger;
			this.options = options;
		}

		public IActionResult Verification()
		{
			return View();
		}

		[HttpPost]
		public async Task<ActionResult> Verification(VerificationModel model)
		{
			TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
			try
			{
				bool isCodeValid = tfa.ValidateTwoFactorPIN(model.MFACode, model.Code);
				if (!isCodeValid)
				{
					return View("Verification", model);
				}
				await SignIn(model.Token, model.RememberMe);
			}
			catch
			{
				return LocalRedirect("/account/login");
			}

			return LocalRedirect("/coreadmin");
		}

		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<ActionResult> Login(LoginModel model)
		{
			try
			{
				var auth = new FirebaseAuthProvider(new FirebaseConfig(CoreAdminProvider.Instance.FirebaseApiKey));
				var loginResult = await auth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);

				string token = loginResult.FirebaseToken;

				if (string.IsNullOrEmpty(token))
				{
					logger.Log(LogLevel.Error, "Firebase hasn't returned access token.");
				}

				var muResponse = await GetMasterUser(token);

				if (muResponse.Item1)
				{
					return View("Verification", new VerificationModel { MFACode = muResponse.Item2, Token = token, RememberMe = model.RememberMe });
				}
			}
			catch
			{
				return View(model);
			}

			return View(model);
		}

		public async Task<ActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return Redirect("/Account/Login");
		}

		private async Task SignIn(string token, bool isPersistent)
		{
			// Decode the token
			var tokenHandler = new JwtSecurityTokenHandler();
			var jsonToken = tokenHandler.ReadToken(token);
			var jswToken = jsonToken as JwtSecurityToken;

			var claims = new List<Claim>();

			claims.Add(new Claim("db_id", jswToken.Claims.First(claim => claim.Type == "db_id").Value));
			claims.Add(new Claim("user_id", jswToken.Claims.First(claim => claim.Type == "user_id").Value));
			var claimIdenties = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(claimIdenties);

			await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			principal,
			new AuthenticationProperties { IsPersistent = isPersistent });
		}

		private async Task<Tuple<bool, string>> GetMasterUser(string token)
		{
			HttpClient client = new HttpClient()
			{
				BaseAddress = new Uri(options.ApiUrl)
			};
			var authToken = "Bearer " + token;
			client.DefaultRequestHeaders.Add("Authorization", authToken);

			//Get current master user
			var masterUserResponse = await client.GetAsync("/api/v1/login");
			if (!masterUserResponse.IsSuccessStatusCode)
			{
				logger.Log(LogLevel.Warning, "/api/v1/login hasn't returned successfull status code.");
				return Tuple.Create(false, "");
			}

			var masterUserJson = await masterUserResponse.Content.ReadAsStringAsync();
			var masterUser = JsonConvert.DeserializeObject<JObject>(masterUserJson);
			string mfaCode = (string)masterUser["mfaCode"];

			var masterUserId = (long)masterUser["id"];
			if (masterUserId == options.SuperAdminId)
			{
				return Tuple.Create(true, mfaCode);
			}

			var tenants = masterUser["tenants"];
			string tenantId = "";
			if (tenants.Any())
			{
				foreach (var tenant in tenants)
				{
					if ((bool)tenant["isDefault"])
					{
						tenantId = (string)tenant["id"];
					}
				}
			}

			if (tenantId == "")
			{
				logger.Log(LogLevel.Information, "User doesn't have default tenant.");
				return Tuple.Create(false, "");
			}

			//Get user role
			client.DefaultRequestHeaders.Add("TenantId", tenantId);
			var permissionResponse = await client.GetAsync("/api/v1/login/permission_details");
			if (!permissionResponse.IsSuccessStatusCode)
			{
				logger.Log(LogLevel.Warning, "/api/v1/login/permission_details hasn't returned successfull status code.");
				return Tuple.Create(false, "");
			}

			var permissionResponseJson = await permissionResponse.Content.ReadAsStringAsync();
			var permissionsAndRoles = JsonConvert.DeserializeObject<JObject>(permissionResponseJson);
			var roles = permissionsAndRoles["roles"];
			if (roles.Any())
			{
				var roleName = roles[0]["name"].ToString();
				if (roleName.Equals(options.AllowedRole, StringComparison.InvariantCultureIgnoreCase))
				{
					return Tuple.Create(true, mfaCode);
				}
			}

			logger.Log(LogLevel.Information, "User doesn't have products.allaccess permission to access admin panel.");

			return Tuple.Create(false, "");
		}
	}
}
