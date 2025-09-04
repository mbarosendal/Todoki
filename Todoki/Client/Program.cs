using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Todoki.Client;
using Todoki.Client.Services;
using Todoki.Client.Shared;

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
builder.Services.AddScoped<JwtAuthorizationMessageHandler>(sp =>
    new JwtAuthorizationMessageHandler(
        sp.GetRequiredService<ILocalStorageService>(),
        sp.GetRequiredService<IHttpClientFactory>(),
        builder.HostEnvironment.BaseAddress
    ));

var jsonOptions = new JsonSerializerOptions
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

builder.Services.AddHttpClient("ServerAPI", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddScoped(sp =>
{
    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient("ServerAPI");
    return new HttpClientWrapper(client, jsonOptions);
});

await builder.Build().RunAsync();