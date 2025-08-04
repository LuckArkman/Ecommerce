// ECommerce.WebApp/Program.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ECommerce.WebApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using ECommerce.WebApp.Handlers; // Adicione este using para TimeSpan
using Microsoft.Extensions.DependencyInjection; // Certifique-se deste using
// ... outros usings ...

var builder = WebApplication.CreateBuilder(args);

// Configuração para o Identity embutido do MVC (se você usou "Individual Accounts")
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ***** ESTE É O BLOCO CORRETO DA AUTENTICAÇÃO *****
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redireciona para a action MVC Login
        options.AccessDeniedPath = "/Account/AccessDenied"; // Se tiver uma página de acesso negado
        options.LogoutPath = "/Account/Logout"; // Redireciona para a action MVC Logout
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
// Fim da configuração do Identity embutido do MVC

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor(); // Needed for HttpContext.Session inside JwtAuthHandler

// Configure the HttpClient for ECommerce.API
builder.Services.AddTransient<JwtAuthHandler>();
// Configura o HttpClient para chamar a ECommerce.API
builder.Services.AddHttpClient("ECommerceApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// MIDDLEWARE DE SESSÃO DEVE VIR ANTES DE AUTENTICAÇÃO/AUTORIZAÇÃO
app.UseSession();
app.UseAuthentication(); // Para o Identity embutido do MVC
app.UseAuthorization();

// REMOVER app.MapRazorPages() - Não usaremos as Razor Pages do Identity diretamente.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();