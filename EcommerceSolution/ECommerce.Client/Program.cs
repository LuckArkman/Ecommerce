using ECommerce.Client.Components;
using ECommerce.Client.Services;
using ECommerce.Client.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage; // Para Blazored.LocalStorage
using System.Net.Http.Headers; // Para HttpClient.DefaultRequestHeaders.Authorization
using System.Web; // Para HttpUtility

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Adiciona Blazored Local Storage
builder.Services.AddBlazoredLocalStorage();

// Configuração do HttpClient e APIs Clientes
builder.Services.AddScoped(sp => new HttpClient {
    BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!) // Certifique-se de que BaseUrl não é nulo
});

// Registra o provedor de autenticação customizado
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

// Registra os Typed HttpClients para cada API
builder.Services.AddHttpClient<ProductApiClient>(client => client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!));
builder.Services.AddHttpClient<CartApiClient>(client => client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!));
builder.Services.AddHttpClient<OrderApiClient>(client => client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!));
builder.Services.AddHttpClient<UserProfileApiClient>(client => client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!));
builder.Services.AddHttpClient<ReviewApiClient>(client => client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!));
builder.Services.AddHttpClient<DashboardApiClient>(client => client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!));
builder.Services.AddHttpClient<AuthApiClient>(client => client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!)); // Novo!

// Registra os serviços de lógica de negócio do cliente
builder.Services.AddScoped<CartService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ECommerce.Client.WebAssembly.App).Assembly);

app.Run();