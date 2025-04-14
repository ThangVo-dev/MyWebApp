using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using WebApp.Admin.Models;
using WebApp.Shared.Responses;

namespace WebApp.MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["IsLoginPage"] = true;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", login);
            }

            var client = _httpClientFactory.CreateClient("ProductApi");
            var content = new StringContent(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(data);

                if (!string.IsNullOrEmpty(tokenResponse?.Token))
                {
                    // Save token to session
                    HttpContext.Session.SetString("JwtToken", tokenResponse.Token);
                    return RedirectToAction("Index", "Product");
                }

                TempData["ErrorMessage"] = "Failed to retrieve token.";
                return View("Login", login);
            }

            TempData["ErrorMessage"] = "Invalid username or password.";
            return View("Login", login);
        }

        [HttpGet]
        public IActionResult LoginGoogle()
        {
            //Redirect user to Google for authentication
            var redirectUrl = Url.Action("GoogleCallback", "User");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            //Get user information from Google after authentication
            var authenticateResult = await HttpContext.AuthenticateAsync();

            if (!authenticateResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Google login failed.";
                return RedirectToAction("Login");
            }

            // Get user information from claims
            var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;

            var token = await HttpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(token) && await ValidateGoogleAccessToken(token))
            {
                HttpContext.Session.SetString("JwtToken", token);
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to retrieve Google access token.";
                return RedirectToAction("Login");
            }

            TempData["SuccessMessage"] = $"Welcome, {name} ({email})!";
            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Delete the authentication cookie
            await HttpContext.SignOutAsync();

            // Delete the JWT token from session (if any)
            HttpContext.Session.Remove("JwtToken");

            // Redirect to the Login page
            return RedirectToAction("Login", "User");
        }

        public async Task<bool> ValidateGoogleAccessToken(string accessToken)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={accessToken}");
            return response.IsSuccessStatusCode;
        }
    }
}