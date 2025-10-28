
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
           // Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            var builder = WebApplication.CreateBuilder(args);

            // Load appsettings + user secrets + environment variables
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddUserSecrets<Program>(optional: true) 
                .AddEnvironmentVariables();

            builder.Services.AddControllers();
            builder.Services.AddControllersWithViews();
            
            builder.Services.AddRazorPages();

            builder.Services.AddCors();

            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
                options.UseOpenIddict(); 
            });

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddOpenIddict()
            .AddCore(opt => opt.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>())
            
            .AddServer(opt =>
            {
                opt.SetAuthorizationEndpointUris("/connect/authorize")
                   .SetTokenEndpointUris("/connect/token")
                   .SetEndSessionEndpointUris("/connect/logout")
                   .SetUserInfoEndpointUris("/connect/userinfo")
                   .SetIntrospectionEndpointUris("/connect/introspect")

                    .RegisterScopes("openid", "email", "profile")

                   .AllowAuthorizationCodeFlow()
                   .AllowRefreshTokenFlow()
                   .AllowImplicitFlow()
                   .AllowHybridFlow()


                   .AddDevelopmentEncryptionCertificate()
                   .AddDevelopmentSigningCertificate()
                   .EnableAuthorizationRequestCaching();
      

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

            builder.Services.AddControllers();
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            app.UseCors(policy =>
            {
                policy.WithOrigins("https://localhost:7193")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });

            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

          
            app.MapControllers();
            
            app.MapDefaultControllerRoute();

            app.MapRazorPages();

            using (var scope = app.Services.CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                await SeedData.InitializeAsync(app.Services, config);
            }

            app.Run();

           

        }
    }
}
