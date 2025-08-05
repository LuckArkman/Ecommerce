// ECommerce.WebApp/Program.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ECommerce.WebApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using ECommerce.WebApp.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Configuração para o Identity embutido do MVC (se você usou "Individual Accounts")
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ***** BLOCO DE CONFIGURAÇÃO DE AUTENTICAÇÃO *****
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LoginPath = "/Account/RedirectToProfile";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
// Fim da configuração do Identity embutido do MVC

// ***** CORREÇÃO AQUI: Apenas UMA VEZ *****
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<JwtAuthHandler>(); // Registra o handler no DI
builder.Services.AddHttpClient("ECommerceApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<JwtAuthHandler>(); // <-- **CORREÇÃO CRÍTICA AQUI: ANEXAR O HANDLER**

// Se você está usando sessões (para armazenar o JWT lá, por exemplo)
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

// app.MapRazorPages(); // Esta linha DEVE SER REMOVIDA se você usa Views MVC para Login/Register/etc.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();