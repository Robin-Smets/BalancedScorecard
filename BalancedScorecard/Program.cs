using BalancedScorecard.Components;
using BalancedScorecard.Services;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRadzenComponents();

builder.Services.AddScoped<IDataStoreService, DataStoreService>();
builder.Services.AddScoped<IPlotDrawer, PlotDrawer>();
builder.Services.AddScoped<IEventMediator, EventMediator>();
builder.Services.AddScoped<IComponentService, ComponentService>();
builder.Services.AddScoped<IMLService, MLService>();
builder.Services.AddScoped<ITerminalService, TerminalService>();
builder.Services.AddScoped<ITransformer, Transformer>();

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
