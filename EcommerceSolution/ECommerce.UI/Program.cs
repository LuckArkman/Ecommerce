using Blazored.LocalStorage;
using ECommerce.Client.Auth;
using ECommerce.Client.Services;
using ECommerce.UI.Components;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredLocalStorage();

string? baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
/*
if (string.IsNullOrEmpty(baseUrl))
{
    throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
}
*/

// Register typed HttpClients
builder.Services.AddHttpClient<ProductApiClient>(client => client.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<CartApiClient>(client => client.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<OrderApiClient>(client => client.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<UserProfileApiClient>(client => client.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<ReviewApiClient>(client => client.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<DashboardApiClient>(client => client.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<AuthApiClient>(client => client.BaseAddress = new Uri(baseUrl));

// Authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

// Services
builder.Services.AddScoped<CartService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();