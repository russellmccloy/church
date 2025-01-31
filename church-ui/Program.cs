using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace attempt1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

            // Add services to the container.
            //builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            //    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

            builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
            .EnableTokenAcquisitionToCallDownstreamApi(
                //new string[] {
                //    builder.Configuration.GetSection("DownstreamApi:Scopes:Read").Get<string>()!,
                //    builder.Configuration.GetSection("DownstreamApi:Scopes:Write").Get<string>()!
                //}
            )
            .AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
            .AddInMemoryTokenCaches();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminsOnly", policy =>
                    policy.RequireClaim("groups", "AdminsUI"));
            });
            
            //builder.Services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("AdminsOnly", policy =>
            //        policy.RequireAssertion(context =>
            //        {
            //            var groupClaimType = "groups";
            //            var adminGroupId = "<Your-Admins-Group-Object-ID>";
            //            return context.User.HasClaim(c =>
            //                c.Type == groupClaimType && c.Value == adminGroupId);
            //        }));
            //});

            builder.Services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            builder.Services.AddRazorPages()
                .AddMicrosoftIdentityUI();

            // Add configuration for Azure Blob Storage
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            
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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
