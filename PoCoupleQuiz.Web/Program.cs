using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Web.Hubs;
// TODO: Move MockQuestionService to Shared/Core for proper reference
// using PoCoupleQuiz.Tests; // Do not re-add this

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddSignalR();

// Add application services
// Determine if we should use the mock service based on configuration
var openAiEndpoint = builder.Configuration["AzureOpenAI:Endpoint"];
var openAiKey = builder.Configuration["AzureOpenAI:Key"];
var useMockService = string.IsNullOrEmpty(openAiEndpoint) || 
                      string.IsNullOrEmpty(openAiKey) ||
                      openAiEndpoint.Contains("your-resource-name");

// Register IQuestionService with proper implementation based on availability of credentials
if (useMockService)
{
    Console.WriteLine("Using MockQuestionService for questions (Azure OpenAI credentials not provided)");
    builder.Services.AddSingleton<IQuestionService, MockQuestionService>();
}
else
{
    Console.WriteLine("Using AzureOpenAIQuestionService for questions");
    builder.Services.AddSingleton<IQuestionService, AzureOpenAIQuestionService>();
}

builder.Services.AddSingleton<ITeamService, AzureTableTeamService>();
builder.Services.AddSingleton<IAzureTableTeamService>(sp =>
{
    var service = sp.GetRequiredService<ITeamService>();
    var tableService = service as AzureTableTeamService;
    if (tableService == null)
        throw new InvalidOperationException($"ITeamService must be of type {nameof(AzureTableTeamService)}");
    return tableService;
});
builder.Services.AddSingleton<IGameHistoryService, GameHistoryService>();
builder.Services.AddScoped<IGameStateService, GameStateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapHub<GameHub>("/gamehub");

app.Run();
