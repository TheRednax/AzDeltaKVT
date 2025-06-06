using AzDeltaKVT.UI;
using AzDeltaKVT.UI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient voor Docker backend
builder.Services.AddScoped(sp =>
{
    var handler = new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("https://localhost:32772/")
    };
});

// Register ApiService
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
