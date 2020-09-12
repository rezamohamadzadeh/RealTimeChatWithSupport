using RealTimeChatWithSupport.AppContext;
using RealTimeChatWithSupport.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System;

namespace RealTimeChatWithSupport
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddHttpContextAccessor();
            services.AddSignalR();
            services.AddHttpClient();
            services.AddSingleton<IChatRoomService, MemoryChatRoomService>();

            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

            services.AddDbContext<ApplicationContext>();

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            }).AddCookie(op =>
            {
                op.SlidingExpiration = false;
                op.LoginPath = "/Account/Login";
                op.LogoutPath = "/Account/LogOut";
                op.AccessDeniedPath = new PathString("/Account/AccessDenied");
                op.Cookie.Name = "Support.Cookie";
                op.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                op.Cookie.SameSite = SameSiteMode.Strict;
                op.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                op.SlidingExpiration = true;                
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapHub<AgentHub>("/agentHub");
            });
        }
    }
}
