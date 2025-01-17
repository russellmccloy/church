using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace church_ui
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var azureAdConfig = builder.Configuration.GetSection("DownstreamApi").GetChildren();
            foreach (var item in azureAdConfig)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }

            // 1. Add Microsoft Identity Web authentication for the Web App
            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

            // 2. Add Microsoft Identity Web API authentication to acquire tokens for the downstream API
            builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration.GetSection("DownstreamApi"))
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();  // Allows in-memory token caching



            builder.Services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            builder.Services.AddRazorPages()
                .AddMicrosoftIdentityUI();

            builder.Services.AddHttpClient();

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

            app.UseAuthentication(); // Add authentication middleware
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
