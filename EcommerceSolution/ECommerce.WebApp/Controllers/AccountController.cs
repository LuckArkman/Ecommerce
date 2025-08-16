// ECommerce.WebApp/Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ECommerce.Models.DTOs.User;
using ECommerce.WebApp.Models;
using Newtonsoft.Json;

namespace ECommerce.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<AccountController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // Action para exibir a página de login customizada
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Action GET para exibir a página de registro
        [HttpGet]
        public async Task<IActionResult> Register(string? returnUrl = null)
        {
            var model = new RegisterViewModel
            {
                ReturnUrl = returnUrl ?? Url.Content("~/"),
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            
            return View(model);
        }

        // Action POST para processar o registro (formulário padrão)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            model.ReturnUrl = returnUrl;
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient("ECommerceApi");
                try
                {
                    var apiRegisterRequest = new RegisterRequest
                    {
                        Email = model.Email,
                        Password = model.Password,
                        ConfirmPassword = model.ConfirmPassword
                    };

                    var apiResponse = await client.PostAsJsonAsync("api/Account/register", apiRegisterRequest);
                    _logger.LogInformation($"API Response Status: {apiResponse.StatusCode}");

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Usuário criou uma nova conta com sucesso via API.");

                        // Login automático após registro
                        var loginResponse = await client.PostAsJsonAsync("api/Account/login",
                            new LoginRequest { email = model.Email, password = model.Password });

                        if (loginResponse.IsSuccessStatusCode)
                        {
                            var loginResult = JsonConvert.DeserializeObject<LoginResult>(
                                await loginResponse.Content.ReadAsStringAsync());

                            if (loginResult != null && loginResult.Success && !string.IsNullOrEmpty(loginResult.Token))
                            {
                                // Autenticar usando cookies
                                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                                var jwtToken = tokenHandler.ReadJwtToken(loginResult.Token);

                                var claimsIdentity = new ClaimsIdentity(jwtToken.Claims,
                                    CookieAuthenticationDefaults.AuthenticationScheme);
                                claimsIdentity.AddClaim(new Claim("access_token", loginResult.Token));

                                var authProperties = new AuthenticationProperties
                                {
                                    IsPersistent = true,
                                    ExpiresUtc = jwtToken.ValidTo
                                };

                                await HttpContext.SignInAsync(
                                    CookieAuthenticationDefaults.AuthenticationScheme,
                                    new ClaimsPrincipal(claimsIdentity),
                                    authProperties);

                                // Salvar informações na sessão
                                HttpContext.Session.SetString("UserId", loginResult.Id);
                                HttpContext.Session.SetString("JwtToken", loginResult.Token);
                                
                                _logger.LogInformation($"Login automático bem-sucedido após registro para usuário: {model.Email}");
                                
                                return RedirectToAction("Index", "UserProfile");
                            }
                        }

                        // Se login automático falhar, redirecionar para login
                        _logger.LogWarning($"Registro bem-sucedido, mas falha no login automático para usuário: {model.Email}");
                        TempData["SuccessMessage"] = "Conta criada com sucesso! Faça login para continuar.";
                        return RedirectToAction("Login", new { ReturnUrl = returnUrl });
                    }
                    else
                    {
                        var errorContent = await apiResponse.Content.ReadAsStringAsync();
                        try
                        {
                            var identityErrors = JsonConvert.DeserializeObject<IdentityError[]>(errorContent);
                            if (identityErrors != null && identityErrors.Any())
                            {
                                foreach (var error in identityErrors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Erro desconhecido no registro.");
                            }
                        }
                        catch
                        {
                            ModelState.AddModelError(string.Empty, 
                                $"Erro ao registrar: {apiResponse.StatusCode} - {errorContent}");
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Erro de rede ao registrar.");
                    ModelState.AddModelError(string.Empty, "Erro de conexão ao registrar. Por favor, tente novamente.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado durante o registro.");
                    ModelState.AddModelError(string.Empty, "Erro inesperado durante o registro. Por favor, tente novamente.");
                }
            }

            return View(model);
        }

        // Action para processar o REGISTRO via AJAX (mantido para compatibilidade com modal)
        [HttpPost("Account/RegisterAjax")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAjax([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            try
            {
                var apiResponse = await client.PostAsJsonAsync("api/Account/register", request);
                _logger.LogInformation($"API Response Status: {apiResponse.StatusCode}");

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
                    return StatusCode((int)apiResponse.StatusCode, 
                        new { success = false, message = errorContent ?? "Erro desconhecido da API de registro." });
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { success = false, message = $"Erro de rede ao registrar: {ex.Message}" });
            }
        }

        // Action para processar o LOGIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var client = _httpClientFactory.CreateClient("ECommerceApi");

            try
            {
                var requestJson = JsonConvert.SerializeObject(request);
                _logger.LogInformation($"API Request Payload: {requestJson}");
                
                var apiResponse = await client.PostAsJsonAsync("api/Account/login", request);
                _logger.LogInformation($"API Login Response Status: {apiResponse.StatusCode}");

                if (apiResponse.IsSuccessStatusCode)
                {
                    var loginResult = JsonConvert.DeserializeObject<LoginResult>(
                        await apiResponse.Content.ReadAsStringAsync());

                    if (loginResult != null && loginResult.Success && !string.IsNullOrEmpty(loginResult.Token))
                    {
                        // Autenticar o usuário no MVC usando cookies
                        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jwtToken = tokenHandler.ReadJwtToken(loginResult.Token);

                        var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, 
                            CookieAuthenticationDefaults.AuthenticationScheme);
                        claimsIdentity.AddClaim(new Claim("access_token", loginResult.Token));
                        
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = jwtToken.ValidTo
                        };
                        
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        HttpContext.Session.SetString("UserId", loginResult.Id);
                        HttpContext.Session.SetString("JwtToken", loginResult.Token);
                        
                        _logger.LogInformation($"Login bem-sucedido. JWT salvo na sessão. Usuário: {request.email}");
                        
                        return RedirectToAction("Index", "UserProfile");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, loginResult?.Message ?? "Credenciais inválidas.");
                        return View(request);
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
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("JwtToken");
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Index", "Home");
        }
    }
}