// ECommerce.WebApp/Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Para [Authorize]
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity; // Para SignInManager, UserManager (se usar para autenticação direta)
using System.Security.Claims; // Para ClaimsPrincipal
using ECommerce.Models.DTOs.User; // Para DTOs (LoginRequest, RegisterRequest, LoginResult)
using Newtonsoft.Json;

namespace ECommerce.WebApp.Controllers
{
    // [Route("[controller]")] // Remover esta rota de nível de controller para evitar conflitos na action padrão
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger; // <-- ADICIONE ESTA LINHA

        public AccountController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<AccountController> logger) // <-- ADICIONE ESTE PARÂMETRO AO CONSTRUTOR
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger; // <-- ATRIBUA AQUI
        }

        // Action para exibir a página de login customizada (que é uma View MVC)
        [HttpGet] // Mapeia para /Account/Login (pela rota padrão) ou /Login (se mapear explicitamente)
        public IActionResult Login(string? returnUrl = null) // Nome da action Login, servirá Login.cshtml
        {
            ViewData["ReturnUrl"] = returnUrl; // Passa o returnUrl para a View
            return View(); // Retorna a View: Views/Account/Login.cshtml
        }

        // Action para exibir a página de registro customizada (que é uma View MVC ou modal)
        [HttpGet]
        public IActionResult Register(string? returnUrl = null) // Nome da action Register, servirá Register.cshtml
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(); // Retorna a View: Views/Account/Register.cshtml (se você tiver uma)
        }


        // Action para processar o REGISTRO (chamada via AJAX do modal)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            try
            {
                var apiResponse = await client.PostAsJsonAsync("api/Account/register", request);
                Console.WriteLine($"API Response Status: {apiResponse.StatusCode}"); // Log para depuração

                if (apiResponse.IsSuccessStatusCode)
                {
                    return Ok(new { success = true, message = "Conta criada com sucesso!" });
                }
                else
                {
                    var errorContent = await apiResponse.Content.ReadAsStringAsync();
                    try
                    {
                        var identityErrors = JsonConvert.DeserializeObject<IdentityError[]>(errorContent);
                        if (identityErrors != null && identityErrors.Any())
                        {
                            var errorMessages = identityErrors.Select(e => e.Description).ToList();
                            return BadRequest(new { success = false, message = "Falha no registro.", errors = errorMessages });
                        }
                    }
                    catch { /* Fallback to general error */ }
                    return StatusCode((int)apiResponse.StatusCode, new { success = false, message = errorContent ?? "Erro desconhecido da API de registro." });
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { success = false, message = $"Erro de rede ao registrar: {ex.Message}" });
            }
        }


        // Action para processar o LOGIN (chamada via formulário POST desta View: Views/Account/Login.cshtml)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null) // Remover [FromBody] se o formulário for padrão
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(request); // Retorna a View com os dados preenchidos para mostrar erros
            }

            var client = _httpClientFactory.CreateClient("ECommerceApi");

            try
            {
                // Serializar o request para JSON e logar para depuração
                var requestJson = JsonConvert.SerializeObject(request);
                Console.WriteLine($"API Request Payload: {requestJson}"); // <-- ADICIONE ESTA LINHA
                
                var apiResponse = await client.PostAsJsonAsync("api/Account/login", request);
                Console.WriteLine($"API Login Response Status: {apiResponse.StatusCode}"); // Log para depuração

                if (apiResponse.IsSuccessStatusCode)
                {
                    var loginResult = JsonConvert.DeserializeObject<LoginResult>(await apiResponse.Content.ReadAsStringAsync());

                    if (loginResult != null && loginResult.Success && !string.IsNullOrEmpty(loginResult.Token))
                    {
                        // Autenticar o usuário no MVC usando cookies
                        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jwtToken = tokenHandler.ReadJwtToken(loginResult.Token);

                        var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true, // Permite "Lembrar-me"
                            ExpiresUtc = jwtToken.ValidTo // Define o vencimento do cookie com base no JWT
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);
                        HttpContext.Session.SetString("JwtToken", loginResult.Token); // Armazena o JWT na sessão

                        _logger.LogInformation($"Login bem-sucedido para usuário: {request.email}");
                        return LocalRedirect(Url.Content("~/UserProfile"));
                    }
                    else
                    {
                        // Login falhou na API, mas o status foi 200 OK (ex: senha errada - API deve retornar 400/401)
                        ModelState.AddModelError(string.Empty, loginResult?.Message ?? "Credenciais inválidas.");
                        return View(request); // Retorna a View com o erro
                    }
                }
                else if (apiResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(string.Empty, "Credenciais inválidas.");
                    return View(request);
                }
                else
                {
                    var errorContent = await apiResponse.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, errorContent ?? "Erro desconhecido ao fazer login.");
                    return View(request);
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro de rede ao fazer login: {ex.Message}");
                return View(request);
            }
        }
        
        // Action para Logout
        [HttpPost]
        [Authorize] // Exige autenticação para fazer logout
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("JwtToken"); // Remove o token da sessão
            return RedirectToAction("Index", "Home");
        }
    }
}