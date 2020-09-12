using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RealTimeChatWithSupport.AppContext;
using RealTimeChatWithSupport.Dtos;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealTimeChatWithSupport.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        public IConfiguration _config { get; }
        private readonly ApplicationContext _context;

        public AccountController(ApplicationContext context,
            IHttpClientFactory httpClient,
            IConfiguration config)
        {
            _context = context;
            _httpClient = httpClient;
            _config = config;

        }

        public IActionResult AccessDenied()
        {
            return View();
        }
        public async Task<IActionResult> Login(string ReturnUrl = null)
        {
            ViewData["ReturnUrl"] = ReturnUrl;

            if (User.Identity.IsAuthenticated)
                await HttpContext.SignOutAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string ReturnUrl)
        {

            ViewData["ReturnUrl"] = ReturnUrl;
            if (ModelState.IsValid)
            {

                var url = _config["BaseApiUrl"];
                url += "/api/User/Login";
                var client = _httpClient.CreateClient();

                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("UserName", model.UserName));
                nvc.Add(new KeyValuePair<string, string>("Password", model.Password));



                var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };
                HttpResponseMessage messages;

                try
                {
                    messages = await client.SendAsync(req);

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }

                if (!messages.IsSuccessStatusCode)
                {
                    var errorMesssage = await messages.Content.ReadAsAsync<JsonResultContent>();
                    ModelState.AddModelError(string.Empty, errorMesssage.Message);
                    return View(model);
                }

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
                var principal = new ClaimsPrincipal(identity);
                var properties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                };

                await HttpContext.SignInAsync(principal, properties);

                if (isAdmin)
                    return RedirectToAction("Index", "Agent");
                else
                    return RedirectToAction("Index", "Home");

            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }


    }
}
