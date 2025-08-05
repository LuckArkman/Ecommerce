// ECommerce.WebApp/Controllers/UserProfileController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ECommerce.Models.DTOs.User; // DTOs do projeto Models
using ECommerce.WebApp.Models; // Seu ViewModel customizado
using System.Text;
using Microsoft.Extensions.Configuration; // Adicione este using
using Microsoft.Extensions.Logging; // Adicione este using
// Remova outros usings desnecessários como Microsoft.AspNetCore.Identity, Authentication, Cookies se não estiverem sendo usados diretamente aqui

namespace ECommerce.WebApp.Controllers
{
    [Authorize]
    [Route("Account/[controller]")] // Rota para o perfil
    public class UserProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserProfileController> _logger; // ***** CORREÇÃO AQUI: Use ILogger<UserProfileController> *****

        public UserProfileController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<UserProfileController> logger) // ***** CORREÇÃO AQUI: Use ILogger<UserProfileController> *****
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        // Exibir o perfil
        [HttpGet]
        public async Task<IActionResult> Index(bool isEditMode = false)
        {
            _logger.LogInformation("UserProfileController.Index (GET) chamado para usuário: {UserName}",
                User.Identity.Name);
            var viewModel = new UserProfileViewModel();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserProfileController (GET): userId nulo após [Authorize]. Redirecionando.");
                return Unauthorized();
            }

            try
            {
                // ***** RECUPERAR O CLIENTE DIRETAMENTE DO FACTORY (CORRETO) *****
                var client = _httpClientFactory.CreateClient("ECommerceApi"); 
                var apiResponse = await client.GetAsync($"api/UserProfile");
                apiResponse.EnsureSuccessStatusCode();

                var userProfileDto =
                    JsonConvert.DeserializeObject<UserProfileDto>(await apiResponse.Content.ReadAsStringAsync());
                if (userProfileDto != null)
                {
                    viewModel.UserProfile = userProfileDto;
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
                }
                
                viewModel.IsEditMode = isEditMode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro HTTP ao carregar perfil do usuário (GET).");
                viewModel.Message = $"Erro ao carregar perfil: {ex.Message}";
                viewModel.IsSuccess = false;
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
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

            return View("~/Views/Account/UserProfile/Index.cshtml", viewModel);
        }

        // Processar a edição do perfil (Post)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UserProfileViewModel viewModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (!ModelState.IsValid)
            {
                viewModel.IsEditMode = true;
                viewModel.Message = "Por favor, corrija os erros no formulário.";
                viewModel.IsSuccess = false;
                try
                {
                    // ***** RECUPERAR O CLIENTE DIRETAMENTE DO FACTORY (CORRETO) *****
                    var client = _httpClientFactory.CreateClient("ECommerceApi");
                    var apiResponse = await client.GetAsync($"api/userprofile");
                    apiResponse.EnsureSuccessStatusCode();
                    viewModel.UserProfile = JsonConvert.DeserializeObject<UserProfileDto>(await apiResponse.Content.ReadAsStringAsync()) ?? new UserProfileDto();
                }
                catch (HttpRequestException ex)
                { _logger.LogError(ex, "Erro ao recarregar perfil após erro de validação (POST)."); }
                catch (JsonException ex)
                { _logger.LogError(ex, "Erro de JSON ao recarregar perfil após erro de validação (POST)."); }
                return View(viewModel);
            }

            try
            {
                // ***** RECUPERAR O CLIENTE DIRETAMENTE DO FACTORY (CORRETO) *****
                var client = _httpClientFactory.CreateClient("ECommerceApi");
                var apiResponse = await client.PutAsJsonAsync("api/userprofile", viewModel.UpdateRequest);
                apiResponse.EnsureSuccessStatusCode();

                viewModel.Message = "Perfil atualizado com sucesso!";
                viewModel.IsSuccess = true;
                viewModel.IsEditMode = false;

                var updatedUserProfileDto = JsonConvert.DeserializeObject<UserProfileDto>(await client.GetAsync($"api/userprofile").Result.Content.ReadAsStringAsync());
                if (updatedUserProfileDto != null)
                {
                    viewModel.UserProfile = updatedUserProfileDto;
                    viewModel.UpdateRequest = new UpdateUserProfileRequest
                    {
                        Email = updatedUserProfileDto.Email,
                        FirstName = updatedUserProfileDto.FirstName,
                        LastName = updatedUserProfileDto.LastName,
                        Address = updatedUserProfileDto.Address,
                        City = updatedUserProfileDto.City,
                        State = updatedUserProfileDto.State,
                        ZipCode = updatedUserProfileDto.ZipCode,
                        PhoneNumber = updatedUserProfileDto.PhoneNumber
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro HTTP ao atualizar perfil do usuário (POST).");
                viewModel.Message = $"Erro ao atualizar perfil: {ex.Message}";
                viewModel.IsSuccess = false;
                viewModel.IsEditMode = true;

                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Perfil (POST): API retornou 401/403 ao atualizar. Redirecionando para login.");
                    return RedirectToAction("Login", "Account");
                }
                try {
                    // ***** RECUPERAR O CLIENTE DIRETAMENTE DO FACTORY (CORRETO) *****
                    var client = _httpClientFactory.CreateClient("ECommerceApi");
                    var apiResponse = await client.GetAsync($"api/userprofile");
                    if (apiResponse.IsSuccessStatusCode) {
                         viewModel.UserProfile = JsonConvert.DeserializeObject<UserProfileDto>(await apiResponse.Content.ReadAsStringAsync()) ?? new UserProfileDto();
                    }
                }
                catch (HttpRequestException loadEx) { _logger.LogError(loadEx, "Erro ao recarregar perfil após falha de atualização (POST)."); }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro de JSON ao atualizar perfil do usuário (POST).");
                viewModel.Message = $"Erro ao processar dados do perfil: {ex.Message}";
                viewModel.IsSuccess = false;
                viewModel.IsEditMode = true;
            }

            return View(viewModel);
        }
    }
}