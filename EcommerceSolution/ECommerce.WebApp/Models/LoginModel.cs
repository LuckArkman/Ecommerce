using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ECommerce.Models.DTOs.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace ECommerce.WebApp.Models
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LoginModel(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LoginModel> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Esta classe InputModel define os campos que o usuário preenche no formulário de login.
        /// Ela está aninhada dentro de LoginModel.
        /// </summary>
        [BindProperty] // Indica que esta propriedade deve ser vinculada aos dados do formulário
        public InputModel Input { get; set; } = new InputModel(); // Inicializa para evitar NullReferenceException

        /// <summary>
        /// Usado para redirecionar o usuário após o login para a URL que ele tentou acessar antes de ser redirecionado para o login.
        /// </summary>
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// Lista de provedores de login externos (Google, Facebook, etc.) configurados.
        /// </summary>
        public IList<AuthenticationScheme>? ExternalLogins { get; set; }

        /// <summary>
        /// A classe que define os campos do formulário de login.
        /// </summary>
        public class InputModel
        {
            [Required(ErrorMessage = "O email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Formato de email inválido.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "A senha é obrigatória.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Lembrar-me?")] public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Método chamado quando a página é acessada via GET (primeira vez).
        /// </summary>
        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMassage)) // Para mensagens de erro de outros lugares
            {
                ModelState.AddModelError(string.Empty, ErrorMassage);
            }

            returnUrl ??= Url.Content("~/"); // Define ReturnUrl para a homepage se não houver um específico

            // Limpa o cookie de autenticação externa existente para garantir um processo de login limpo
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        /// <summary>
        /// Método chamado quando o formulário é submetido via POST.
        /// </summary>
        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/UserProfile");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient("ECommerceApi");
                try
                {
                    var apiLoginRequest = new LoginRequest { email = Input.Email, password = Input.Password };
                    var apiResponse = await client.PostAsJsonAsync("api/Account/login", apiLoginRequest);

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        var loginResult =
                            JsonConvert.DeserializeObject<LoginResult>(await apiResponse.Content.ReadAsStringAsync());

                        if (loginResult != null && loginResult.Success && !string.IsNullOrEmpty(loginResult.Token))
                        {
                            // Autenticar o usuário no MVC usando cookies (usando claims do JWT da API)
                            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                            var jwtToken = tokenHandler.ReadJwtToken(loginResult.Token);

                            var claimsIdentity = new ClaimsIdentity(jwtToken.Claims,
                                CookieAuthenticationDefaults.AuthenticationScheme);
                            var authProperties = new AuthenticationProperties
                            {
                                IsPersistent = Input.RememberMe, // Usa o "Lembrar-me" do formulário
                                ExpiresUtc = jwtToken.ValidTo // Expiração do cookie igual ao JWT
                            };

                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(claimsIdentity),
                                authProperties);

                            // **** ARMAZENAR O JWT NA SESSÃO AQUI ****
                            HttpContext.Session.SetString("JwtToken", loginResult.Token);

                            _logger.LogInformation("Usuário logado via API.");
                            return RedirectToAction("Index", "UserProfile");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty,
                                loginResult?.Message ?? "Credenciais inválidas ou erro na API.");
                            return Page();
                        }
                    }
                    else // API retornou status de erro (401, 400, 500)
                    {
                        var errorContent = await apiResponse.Content.ReadAsStringAsync();
                        if (apiResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            ModelState.AddModelError(string.Empty, "Credenciais inválidas.");
                        }
                        else
                        {
                            // Tentar extrair erros mais detalhados se a API retornar ProblemDetails ou erros do Identity
                            ModelState.AddModelError(string.Empty,
                                $"Erro na API: {apiResponse.StatusCode} - {errorContent}");
                        }

                        return Page();
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Erro de rede ao chamar a API de login.");
                    ModelState.AddModelError(string.Empty, "Erro de conexão. Por favor, tente novamente mais tarde.");
                    return Page();
                }
            }

            return Page();
        }

        [TempData] public string? ErrorMassage { get; set; }
    }
}