using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ECommerce.WebApp.Data;
// Se você está usando o Identity embutido do MVC
// using ECommerce.WebApp.Models; // Para o ApplicationUser do MVC template
//using Npgsql.EntityFrameworkCore.PostgreSQL; // se for usar Postgres para Identity local do MVC

var builder = WebApplication.CreateBuilder(args);

// Configuração para o Identity embutido do MVC (se você usou "Individual Accounts")
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)); // Ou .UseSqlServer

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
// Fim da configuração do Identity embutido do MVC

builder.Services.AddControllersWithViews();

// Configura o HttpClient para chamar a ECommerce.API
builder.Services.AddHttpClient("ECommerceApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
    // Você pode adicionar cabeçalhos padrão aqui, como Content-Type
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
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

app.UseAuthentication(); // Para o Identity embutido do MVC
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Para as páginas de Identity geradas

app.Run();