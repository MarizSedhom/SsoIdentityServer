using Client1.Data;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Client1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
          

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

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
                           redirectUri.Query = $"returnUrl={"https://localhost:7093" + returnUrl}";

                           redirectContext.HttpContext.Response.Redirect(redirectUri.ToString());
                           return Task.CompletedTask;
                       },
                       OnRedirectToLogout = redirectContext =>
                       {
                           var returnUrl = redirectContext.HttpContext.Request.Path +
                                           redirectContext.HttpContext.Request.QueryString;

                           // Append the returnUrl as a query parameter to the Identity Server logout URL
                           var redirectUri = new UriBuilder("https://localhost:7025/Identity/Account/Logout");
                           redirectUri.Query = $"returnUrl={"https://localhost:7135"}";

                           redirectContext.HttpContext.Response.Redirect(redirectUri.ToString());
                           return Task.CompletedTask;
                       }
                   };
                   /*
                   options.AccessDeniedPath = "/Account/AccessDenied";
                   options.Cookie.Path = "/";*/
               });

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
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
            //To test user Claims
            /*app.Use(async (context, next) =>
            {
                if (context.User.Identity.IsAuthenticated)
                {
                    Console.WriteLine("User is authenticated");
                    foreach (var claim in context.User.Claims)
                    {
                        Console.WriteLine($"{claim.Type}: {claim.Value}");
                    }
                }
                else
                {
                    Console.WriteLine("User is not authenticated");
                }
                await next.Invoke();
            });*/

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            //app.MapRazorPages();

            app.Run();
        }
    }
}
