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

namespace ECommerce.WebApp.Models;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    private readonly ILogger<RegisterModel> _logger;

    // private readonly IEmailSender _emailSender; // Se precisar de envio de e-mail
    private readonly IHttpClientFactory _httpClientFactory; // Para chamar a API
    private readonly IConfiguration _configuration; // Para ler ApiBaseUrl

    public RegisterModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<RegisterModel> logger,
        // IEmailSender emailSender,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        // _emailSender = emailSender;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();
    
    public string ReturnUrl { get; set; } = string.Empty;

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.",
            MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar senha")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação de senha não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    
    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        
        // Initialize ExternalLogins
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        
        // Ensure Input is initialized (though it's already done in the property declaration)
        if (Input == null)
        {
            Input = new InputModel();
        }
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        // Initialize ReturnUrl properly
        ReturnUrl = returnUrl ?? Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            try
            {
                var apiRegisterRequest = new RegisterRequest
                    { Email = Input.Email, Password = Input.Password, ConfirmPassword = Input.ConfirmPassword };
                var apiResponse = await client.PostAsJsonAsync("api/Account/register", apiRegisterRequest);

                if (apiResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Usuário criou uma nova conta com sucesso via API.");

                    // Opcional: Logar o usuário automaticamente após o registro bem-sucedido
                    var loginResponse = await client.PostAsJsonAsync("api/Account/login",
                        new LoginRequest { email = Input.Email, password = Input.Password });
                    loginResponse.EnsureSuccessStatusCode(); // Lança exceção se o login automático falhar
                    var loginResult =
                        JsonConvert.DeserializeObject<LoginResult>(await loginResponse.Content.ReadAsStringAsync());

                    if (loginResult != null && loginResult.Success && !string.IsNullOrEmpty(loginResult.Token))
                    {
                        // Autenticar no MVC usando cookies e o token JWT da API
                        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jwtToken = tokenHandler.ReadJwtToken(loginResult.Token);

                        var claimsIdentity = new ClaimsIdentity(jwtToken.Claims,
                            CookieAuthenticationDefaults.AuthenticationScheme);
                        claimsIdentity.AddClaim(new Claim("access_token",
                            loginResult.Token)); // Adicionar JWT como claim
                        var authProperties = new AuthenticationProperties
                            { IsPersistent = true, ExpiresUtc = jwtToken.ValidTo };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return LocalRedirect(ReturnUrl);
                    }
                    else
                    {
                        // Registro OK, mas login automático falhou. Redirecionar para a página de login.
                        _logger.LogWarning(
                            $"Registro bem-sucedido, mas falha no login automático para usuário: {Input.Email}");
                        ModelState.AddModelError(string.Empty,
                            "Registro bem-sucedido, mas o login automático falhou. Por favor, faça login manualmente.");
                        return RedirectToPage("Login", new { ReturnUrl = ReturnUrl });
                    }
                }
                else
                {
                    var errorContent = await apiResponse.Content.ReadAsStringAsync();
                    var identityErrors =
                        JsonConvert.DeserializeObject<IdentityError[]>(errorContent); // Tenta ler erros do Identity
                    if (identityErrors != null && identityErrors.Any())
                    {
                        foreach (var error in identityErrors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty,
                            $"Erro ao registrar: {apiResponse.StatusCode} - {errorContent}");
                    }

                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de rede ao registrar.");
                ModelState.AddModelError(string.Empty, "Erro de conexão ao registrar. Por favor, tente novamente.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado durante o registro.");
                ModelState.AddModelError(string.Empty,
                    "Erro inesperado durante o registro. Por favor, tente novamente.");
                return Page();
            }
        }

        // Se ModelState.IsValid for false
        return Page();
    }
}