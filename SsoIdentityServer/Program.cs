using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SsoIdentityServer.Data;

namespace SsoIdentityServer
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

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"c:\SharedKeyRing"))
                .SetApplicationName("SharedCookieApp");

            //added part
           builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                 .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                 {
                     options.Cookie.Name = ".AspNet.SharedCookie";
                     options.LoginPath = "/Account/Login";
                 });

            /**/ builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".AspNet.SharedCookie";
                options.LoginPath = "/Account/Login";
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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
