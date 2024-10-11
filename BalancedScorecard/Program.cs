// Program.cs

using Radzen;
using BalancedScorecard.Components;
using BalancedScorecard.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.UseUrls("http://0.0.0.0:80");

// Add components.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddRadzenComponents();

// Add datastore
builder.Services.AddSingleton<IDataStoreService, DataStoreService>();
builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<IDataStoreService>());

// Add services
builder.Services.AddScoped<IEventMediator, EventMediator>();
builder.Services.AddScoped<IPlotDrawer, PlotDrawer>();
builder.Services.AddScoped<IMLService, MLService>();
builder.Services.AddScoped<ITerminalService, TerminalService>();
builder.Services.AddScoped<ITransformer, Transformer>();
builder.Services.AddScoped<IAppState, AppState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
