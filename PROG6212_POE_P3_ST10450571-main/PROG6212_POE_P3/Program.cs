using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PROG6212_POE_P3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Add MVC services to the container ---
            builder.Services.AddControllersWithViews();

            // --- Add Session support ---
            builder.Services.AddDistributedMemoryCache(); // Required for session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 minutes session timeout
                options.Cookie.HttpOnly = true; // Secure access
                options.Cookie.IsEssential = true; // Required for GDPR compliance
            });

            var app = builder.Build();

            // --- Configure the HTTP request pipeline ---
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts(); // Enforces HTTPS for production
            }

            // --- Middleware configuration ---
            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Enables serving CSS, JS, and uploads from wwwroot
            app.UseRouting();
            app.UseSession(); // Enable session before authorization
            app.UseAuthorization();

            // --- Routing setup ---
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            // --- Run the app ---
            app.Run();
        }
    }
}
