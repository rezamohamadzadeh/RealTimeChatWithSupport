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
            services.AddHttpClient();
            services.AddSingleton<IChatRoom, ChatRoom>();

            #region AddSignalR
            services.AddSignalR();
            #endregion

            #region AddCors
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins("http://localhost:50483")
                        .AllowCredentials();
                });


            });
            #endregion

            #region AddMvc
            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });
            #endregion

            #region AddDbContext
            services.AddDbContext<ApplicationContext>();
            #endregion

            #region Authentication
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
            #endregion




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
            app.UseCors("CorsPolicy");

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
                    pattern: "{controller=Agent}/{action=Index}/{id?}");
                endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapHub<AgentHub>("/agentHub");
            });
        }
    }
}
