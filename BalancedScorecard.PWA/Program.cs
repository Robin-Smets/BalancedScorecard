using BalancedScorecard.PWA;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient for Blazor WebAssembly
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5555/") });

await builder.Build().RunAsync();
