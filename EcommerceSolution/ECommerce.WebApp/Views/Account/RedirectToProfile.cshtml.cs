
using Microsoft.AspNetCore.Mvc; // Para RedirectToActionResult
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http; // Para HttpContextAccessor
using System.Net.Http.Headers; // Para autenticação
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ECommerce.WebApp.Account
{
    public class RedirectToProfileModel : PageModel
    {
        private readonly ILogger<RedirectToProfileModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor; // Para acessar a sessão

        public RedirectToProfileModel(ILogger<RedirectToProfileModel> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult OnGet() // Mudar para IActionResult
        {
            var jwtToken = TempData["JwtTokenForRedirect"] as string;

            if (!string.IsNullOrEmpty(jwtToken))
            {
                // Salvar o token na sessão para o JwtAuthHandler
                _httpContextAccessor.HttpContext?.Session.SetString("JwtToken", jwtToken);
                _logger.LogInformation($"RedirectToProfile: JWT obtido de TempData e salvo na sessão. Tamanho: {jwtToken.Length}");
                
                // Redirecionar para o perfil
                return RedirectToAction("Index", "UserProfile");
            }
            else
            {
                _logger.LogWarning("RedirectToProfile: JWT não encontrado em TempData. Redirecionando para login.");
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
        }
    }
}