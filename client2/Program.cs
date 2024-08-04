using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

namespace client2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure data protection
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"c:\SharedKeyRing"))
                .SetApplicationName("SharedCookieApp");

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(options =>
               {
                   options.Cookie.Name = ".AspNet.SharedCookie";
                  // options.LoginPath = new PathString("https://localhost:7025/Identity/Account/Login"); // Redirect to Identity Server's login page
                   options.Events = new CookieAuthenticationEvents
                   {
                       OnRedirectToLogin = redirectContext =>
                       {
                          // redirectContext.HttpContext.Response.Redirect("https://localhost:7025/Identity/Account/Login");

                           var returnUrl = redirectContext.HttpContext.Request.Path +
                                 redirectContext.HttpContext.Request.QueryString;

                           // Append the returnUrl as a query parameter to the Identity Server login URL
                           var redirectUri = new UriBuilder("https://localhost:7025/Identity/Account/Login");
                           redirectUri.Query = $"returnUrl={"https://localhost:7135" + returnUrl}";

                           redirectContext.HttpContext.Response.Redirect(redirectUri.ToString());
                           return Task.CompletedTask;
                       }
                   };
                   /*
                   options.AccessDeniedPath = "/Account/AccessDenied";
                   options.Cookie.Path = "/";*/
               });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
