using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies", options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.Name = ".AspNet.SharedCookie";
})
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://localhost:7013"; 
    options.ClientId = "client1";
    options.ClientSecret = "secret";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.UsePkce = false;
    options.CallbackPath = "/signin-oidc";
    options.RequireHttpsMetadata = true; 
    
    options.Scope.Add("email");
    options.Scope.Add("profile");

    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "role";

    options.Events = new OpenIdConnectEvents
    {
        OnRemoteFailure = context =>
        {
            context.Response.Redirect("/error?message=" + Uri.EscapeDataString(context.Failure?.Message ?? "unknown"));
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddCascadingAuthenticationState();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();


var app = builder.Build();
app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
   
    app.UseHsts();
}



app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapGet("/signin-oidc", () => "Hit signin-oidc manually"); // test


app.Run();
