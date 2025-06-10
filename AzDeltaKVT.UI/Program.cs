using AzDeltaKVT.UI;
using AzDeltaKVT.UI.Modal;
using AzDeltaKVT.UI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:8080")
});

// Register ApiService
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<UploadResultState>();

await builder.Build().RunAsync();