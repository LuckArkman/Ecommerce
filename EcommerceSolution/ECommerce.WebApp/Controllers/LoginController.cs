using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebApp.Controllers;

public class LoginController: Controller
{
    // Esta ação será acessível via /Login ou /Login/Index
    public IActionResult Index()
    {
        // Redireciona para a página Razor de login do ASP.NET Core Identity
        return RedirectToPage("/Account/Login", new { area = "Identity" });
    }
}