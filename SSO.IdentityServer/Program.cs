
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
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddControllersWithViews();
            
            builder.Services.AddRazorPages();
            
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
        
        opt.SetAuthorizationEndpointUris("/connect/authorize");
        opt.SetTokenEndpointUris("/connect/token");

        opt.AllowAuthorizationCodeFlow().
            AllowRefreshTokenFlow();
        opt.RegisterScopes("openid", "email", "profile");
        opt.AcceptAnonymousClients();
        opt.AddDevelopmentEncryptionCertificate();
        opt.AddDevelopmentSigningCertificate();

        opt.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableStatusCodePagesIntegration();

        opt.DisableAccessTokenEncryption(); 
        opt.AddEventHandler<OpenIddict.Server.OpenIddictServerEvents.ProcessErrorContext>(builder =>
        {
            builder.UseInlineHandler(async context =>
            {
                if (context.Transaction.Properties.TryGetValue("httpContext", out var ctxObj) && ctxObj is HttpContext httpContext)
                {
                    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(context.Error);
                }
                else
                {
                   
                    Console.WriteLine($"OpenIddict error: {context.Error}");
                }
                await Task.CompletedTask;
            });
            });

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

            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

          
            app.MapControllers();
            
            app.MapDefaultControllerRoute();

            app.MapRazorPages();

            await SeedData.InitializeAsync(app.Services);

            app.Run();

           

        }
    }
}
