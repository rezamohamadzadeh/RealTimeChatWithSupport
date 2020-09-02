using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using RealTimeChatWithSupport.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
namespace RealTimeChatWithSupport.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClient;
        public IConfiguration _config { get; }

        public LoginModel(IHttpClientFactory httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }

        public void OnGet(/*string returnUrl = null*/)
        {
            //ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPost([FromQuery] string returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid) return Page();

                var username = Request.Form["username"];
                var password = Request.Form["password"];


                var url = _config["BaseApiUrl"];
                url += "/api/User/Login";
                var client = _httpClient.CreateClient();

                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UserName", username));
                nvc.Add(new KeyValuePair<string, string>("Password", password));



                var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };

                HttpResponseMessage messages = await client.SendAsync(req);

                if (!messages.IsSuccessStatusCode)
                    return Page();

                var content = await messages.Content.ReadAsAsync<JsonResultContent<UserDto>>();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, content.Data.UserName),
                    new Claim(ClaimTypes.NameIdentifier, content.Data.UserId),
                };

                bool isAdmin = false;

                foreach (var role in content.Data.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                    if (role == "Admin")
                        isAdmin = true;
                }
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var properties = new AuthenticationProperties
                {
                    RedirectUri = isAdmin ? Url.Content("~/Agent/Index") : Url.Content("~/Home/Index")
                };

                return SignIn(
                        new ClaimsPrincipal(identity),
                        properties,
                        CookieAuthenticationDefaults.AuthenticationScheme);

            }
            catch (Exception ex)
            {
                return Page();
            }

        }
    }
}
