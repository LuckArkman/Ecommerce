// ECommerce.WebApp/Controllers/UserProfileController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Para [Authorize]
using System.Security.Claims;
using ECommerce.Application.Interfaces;
using ECommerce.Models.DTOs.User;
using ECommerce.WebApp.Models;
using Newtonsoft.Json; // Para ClaimsPrincipal, ClaimsIdentity
// ... outros usings ...

namespace ECommerce.WebApp.Controllers
{
    // [Authorize] // <-- COMENTE OU REMOVA ESTA LINHA PARA PERMITIR ACESSO ANÔNIMO
    [Route("Account/[controller]")] // Rota para o perfil
    public class UserProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserProfileController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor; // Para acessar a sessão

        public UserProfileController(
            IHttpClientFactory httpClientFactory,
            ILogger<UserProfileController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index(bool isEditMode = false)
        {
            _logger.LogInformation("UserProfileController.Index (GET) chamado para usuário: {UserName}", User.Identity.Name);
            var viewModel = new UserProfileViewModel();
            var userId = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
            Console.WriteLine($"UserId >> {userId} <<");
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserProfileController (GET): userId nulo após [Authorize]. Redirecionando.");
            }
            try
            {
                // ***** CHAMADA PARA A API DO PERFIL DO USUÁRIO *****
                var apiResponse = await GetProfile($"api/Account/userProfile?userId={userId}") as string;

                var userProfileDto = JsonConvert.DeserializeObject<UserProfileDto>(apiResponse);
                
                if (userProfileDto != null)
                {
                    viewModel.UserProfile = userProfileDto;
                    // Preenche o UpdateRequest para o formulário de edição
                    viewModel.UpdateRequest = new UpdateUserProfileRequest
                    {
                        Email = userProfileDto.Email,
                        FirstName = userProfileDto.FirstName,
                        LastName = userProfileDto.LastName,
                        Address = userProfileDto.Address,
                        City = userProfileDto.City,
                        State = userProfileDto.State,
                        ZipCode = userProfileDto.ZipCode,
                        PhoneNumber = userProfileDto.PhoneNumber
                    };
                    _logger.LogInformation($"Dados do perfil carregados para {userProfileDto.Email}.");
                }
                else
                {
                    _logger.LogWarning("UserProfileController (GET): userProfileDto retornado nulo da API.");
                    viewModel.Message = $"Erro: Dados do perfil não encontrados.";
                    viewModel.IsSuccess = false;
                }
                viewModel.IsEditMode = isEditMode; // Define o modo de edição com base no parâmetro
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro HTTP ao carregar perfil do usuário (GET). Status: {StatusCode}", ex.StatusCode);
                viewModel.Message = $"Erro ao carregar perfil: {ex.Message}. Por favor, tente novamente.";
                viewModel.IsSuccess = false;
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Perfil (GET): API retornou 401/403. Redirecionando para login.");
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro de JSON ao carregar perfil do usuário (GET).");
                viewModel.Message = $"Erro ao processar dados do perfil: {ex.Message}";
                viewModel.IsSuccess = false;
            }
            catch (Exception ex) // Catch genérico para qualquer outra exceção
            {
                _logger.LogError(ex, "Erro inesperado ao carregar perfil do usuário (GET).");
                viewModel.Message = $"Erro inesperado: {ex.Message}";
                viewModel.IsSuccess = false;
            }

            return View("~/Views/Account/UserProfile/Index.cshtml", viewModel); // Retorna a View com o ViewModel preenchido
        }

        private async Task<object> GetProfile(string empty)
        {
            var cl = _httpClientFactory.CreateClient("ECommerceApi");
            string url = $"{cl.BaseAddress}{empty}";
            Console.WriteLine($"Url >> {url} <<");
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (HttpClient client = new HttpClient(handler))
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        return responseBody;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error No Data: {ex.Message}");
                    }
                }
            }
            return "";
        }
    }
}