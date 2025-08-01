using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.IdentityServer.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            // Kör migrering (om du vill slippa göra det manuellt)
            await context.Database.MigrateAsync();

            // Skapa en användare
            var user = await userManager.FindByNameAsync("admin");
            if (user == null)
            {
                user = new IdentityUser("admin") { Email = "admin@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(user, "Admin123!");
            }

            // Skapa OpenIddict-klient
            if (await appManager.FindByClientIdAsync("client1") is null)
            {
                await appManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "client1",
                    ClientSecret = "secret",
                    DisplayName = "Blazor Client 1",
                    RedirectUris = { new Uri("https://localhost:5002/signin-oidc") },
                    Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile
                }
                });
            }
        }
    }
}
