
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using SSO.IdentityServer.Data;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace SSO.IdentityServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load appsettings + user secrets + environment variables
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddUserSecrets<Program>(optional: true) 
                .AddEnvironmentVariables();

            // Register core MVC & Razor services
            builder.Services.AddControllersWithViews();         
            builder.Services.AddRazorPages();
            builder.Services.AddCors();
            builder.Services.AddOpenApi();

            // Configure EF Core + Identity + OpenIddict
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING"));
                options.UseOpenIddict(); 
            });

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Configure OpenIddict (server + validation)
            builder.Services.AddOpenIddict()
            .AddCore(opt => opt.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>())
            
            .AddServer(opt =>
            {
                // Server endpoints
                opt.SetAuthorizationEndpointUris("/connect/authorize")
                   .SetTokenEndpointUris("/connect/token")
                   .SetEndSessionEndpointUris("/connect/logout")
                   .SetUserInfoEndpointUris("/connect/userinfo")
                   .SetIntrospectionEndpointUris("/connect/introspect");

                // Supported scopes and flows
                opt.RegisterScopes("openid", "email", "profile");

                opt.AllowAuthorizationCodeFlow()
                   .AllowRefreshTokenFlow()
                   .AllowImplicitFlow()
                   .AllowHybridFlow();

                // Development certificates only (replace in production)
                opt.AddDevelopmentEncryptionCertificate()
                  .AddDevelopmentSigningCertificate();

                 opt.EnableAuthorizationRequestCaching();

                // Integrate with ASP.NET Core pipeline
                opt.UseAspNetCore()
                   .EnableAuthorizationEndpointPassthrough()
                   .EnableTokenEndpointPassthrough()
                   .EnableEndSessionEndpointPassthrough()
                   .EnableStatusCodePagesIntegration();

                opt.DisableAccessTokenEncryption();
            })
            .AddValidation(opt =>
            {
                opt.UseLocalServer();
                opt.UseAspNetCore();
            });

            // Authentication & Authorization
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // OpenAPI (dev only)
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            // CORS setup (allow local client)
            app.UseCors(policy =>
            {
                policy.WithOrigins("https://localhost:7193")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });

            // Middleware pipeline
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
         
            app.MapControllers();
            app.MapDefaultControllerRoute();
            app.MapRazorPages();

            // Seed initial data
            using (var scope = app.Services.CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                await SeedData.InitializeAsync(app.Services, config);
            }

            app.Run();      

        }
    }
}
