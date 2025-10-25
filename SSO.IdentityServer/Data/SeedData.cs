using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
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

            await context.Database.MigrateAsync();

            var user = await userManager.FindByNameAsync("admin");
            if (user == null)
            {
                user = new IdentityUser("admin") { Email = "admin@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(user, "Admin123!");
            }

            if (await appManager.FindByClientIdAsync("client1") is null)
            {
                await appManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "client1",
                    ClientSecret = "secret",
                    DisplayName = "MVC Client 1",
                    RedirectUris = { new Uri("https://localhost:7272/signin-oidc") },
                    ClientType = ClientTypes.Confidential,
                    PostLogoutRedirectUris = { new Uri("https://localhost:7272/signout-callback-oidc") },
                    Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                     OpenIddictConstants.Permissions.Endpoints.EndSession,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.Endpoints.Introspection,
                    Permissions.Endpoints.Revocation,
                    Permissions.ResponseTypes.Code,
                    Permissions.Prefixes.Scope + Scopes.OpenId,
                    Permissions.Prefixes.Scope + Scopes.Profile,
                    Permissions.Prefixes.Scope + Scopes.Email,
                }

                });
            }
            if (await appManager.FindByClientIdAsync("client2") is null)
            {
                await appManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "client2",
                    DisplayName = "Blazor Client 2",
                    RedirectUris = { new Uri("https://localhost:7193/authentication/login-callback") },
                    ClientType = ClientTypes.Public,
                    ConsentType = ConsentTypes.Explicit,
                    PostLogoutRedirectUris = { new Uri("https://localhost:7193/authentication/logout-callback") },
                    Permissions =
                    {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    Permissions.Endpoints.Introspection,
                    Permissions.Endpoints.Revocation,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Prefixes.Scope + Scopes.OpenId,
                    Permissions.Prefixes.Scope + Scopes.Profile,
                    Permissions.Prefixes.Scope + Scopes.Email,
                    },
                    Requirements =
                    {
                        OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }
        }
    }
}
