using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using TickTask.Client;
using TickTask.Client.Services;
using TickTask.Client.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Services
builder.Services.AddScoped<TimerService>();
builder.Services.AddScoped<TaskApiService>();
builder.Services.AddScoped<UserSettingsApiService>();
builder.Services.AddScoped<UserProjectApiService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddSingleton<TimerStateService>();

builder.Services.AddScoped<JwtAuthorizationMessageHandler>();

// Configure HttpClient with global JsonSerializerOptions
var jsonOptions = new JsonSerializerOptions
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true
};

builder.Services.AddHttpClient("ServerAPI", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

// Register a typed HttpClient that uses the above JsonSerializerOptions
builder.Services.AddScoped(sp =>
{
    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient("ServerAPI");

    // Wrap client to include JsonOptions globally
    return new HttpClientWrapper(client, jsonOptions);
});

await builder.Build().RunAsync();
