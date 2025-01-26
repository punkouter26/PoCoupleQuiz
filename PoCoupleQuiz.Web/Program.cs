using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using PoCoupleQuiz.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

// Add application services
builder.Services.AddSingleton<IQuestionService, AzureOpenAIQuestionService>();
builder.Services.AddSingleton<ITeamService, AzureTableTeamService>();
builder.Services.AddSingleton<IAzureTableTeamService>(sp => sp.GetRequiredService<ITeamService>() as AzureTableTeamService);
builder.Services.AddSingleton<IGameHistoryService, GameHistoryService>();

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

app.Run();
