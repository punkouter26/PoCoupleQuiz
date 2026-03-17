using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using PoCoupleQuiz.Client;
using PoCoupleQuiz.Client.Extensions;
using PoCoupleQuiz.Client.Services;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddRadzenComponents();

// Auth: dev cookie provider in Development, real MSAL in Production
if (builder.HostEnvironment.IsDevelopment())
{
    builder.Services.AddAuthorizationCore();
    builder.Services.AddScoped<AuthenticationStateProvider, TestAuthStateProvider>();
}
else
{
    builder.Services.AddMsalAuthentication(options =>
    {
        builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
        options.ProviderOptions.DefaultAccessTokenScopes
            .Add("api://0cb02b96-d2dc-41ec-90b3-5ff9937fab29/game.play");
    });
}

builder.Services.AddClientServices();

var app = builder.Build();

await app.RunAsync();
