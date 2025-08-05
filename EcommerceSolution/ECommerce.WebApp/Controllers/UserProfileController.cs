// ECommerce.WebApp/Controllers/UserProfileController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Para [Authorize]
using System.Security.Claims; // Para ClaimsPrincipal, ClaimsIdentity
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ECommerce.Models.DTOs.User; // DTOs do projeto Models
using ECommerce.WebApp.Models; // Seu ViewModel customizado
using System.Text;

namespace ECommerce.WebApp.Controllers
{
    [Authorize] // Apenas usuários autenticados podem acessar este controller
    public class UserProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public UserProfileController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AccountController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        private async Task<HttpClient> GetApiClientWithAuth()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            
            var jwtToken = HttpContext.Session.GetString("JwtToken"); // Exemplo se o token estivesse na sessão
            if (!string.IsNullOrEmpty(jwtToken))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            }

            return client;
        }
        
        // Exibir o perfil (CORREÇÃO DE LÓGICA AQUI)
        [HttpGet]
        public async Task<IActionResult>
            Index(bool isEditMode = false) // Adicione parâmetro para controlar o modo de edição
        {
            _logger.LogInformation("UserProfileController.Index (GET) chamado para usuário: {UserName}",
                User.Identity.Name);
            var viewModel = new UserProfileViewModel();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserProfileController (GET): userId nulo após [Authorize]. Redirecionando.");
                return Unauthorized(); // Isso redireciona para a página de login
            }

            try
            {
                // ***** RECUPERAR O CLIENTE DIRETAMENTE DO FACTORY *****
                var client = _httpClientFactory.CreateClient("ECommerceApi");
                var apiResponse = await client.GetAsync($"api/UserProfile"); // Endpoint na ECommerce.API
                apiResponse.EnsureSuccessStatusCode();

                var userProfileDto =
                    JsonConvert.DeserializeObject<UserProfileDto>(await apiResponse.Content.ReadAsStringAsync());
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
                }

                viewModel.IsEditMode = isEditMode; // Define o modo de edição com base no parâmetro
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

            return View(viewModel); // Retorna a View com o ViewModel preenchido
        }


        // Processar a edição do perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UserProfileViewModel viewModel)
        {
            // O ModelState.IsValid aqui valida o UpdateRequest dentro do ViewModel
            if (!ModelState.IsValid)
            {
                viewModel.IsEditMode = true; // Mantém no modo de edição se a validação falhar
                viewModel.Message = "Por favor, corrija os erros no formulário.";
                viewModel.IsSuccess = false;
                // Recarregar UserProfile para o ViewModel, pois ele pode estar vazio se a submissão falhar
                var _userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(_userId))
                {
                    try
                    {
                        var client = await GetApiClientWithAuth();
                        var apiResponse = await client.GetAsync($"api/userprofile");
                        apiResponse.EnsureSuccessStatusCode();
                        viewModel.UserProfile = JsonConvert.DeserializeObject<UserProfileDto>(await apiResponse.Content.ReadAsStringAsync()) ?? new UserProfileDto();
                    }
                    catch (HttpRequestException) { /* Lidar com erro de carregamento aqui */ }
                }
                return View(viewModel);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var client = await GetApiClientWithAuth();
                var apiResponse = await client.PutAsJsonAsync("api/userprofile", viewModel.UpdateRequest);
                apiResponse.EnsureSuccessStatusCode(); // Lança exceção para 4xx/5xx

                viewModel.Message = "Perfil atualizado com sucesso!";
                viewModel.IsSuccess = true;
                viewModel.IsEditMode = false; // Sai do modo de edição

                // Recarregar os dados mais recentes do perfil após a atualização
                var updatedUserProfileDto = JsonConvert.DeserializeObject<UserProfileDto>(await client.GetAsync($"api/userprofile").Result.Content.ReadAsStringAsync());
                if (updatedUserProfileDto != null)
                {
                    viewModel.UserProfile = updatedUserProfileDto;
                    viewModel.UpdateRequest = new UpdateUserProfileRequest // Atualiza o UpdateRequest também
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
                viewModel.Message = $"Erro ao atualizar perfil: {ex.Message}";
                viewModel.IsSuccess = false;
                viewModel.IsEditMode = true; // Permanece no modo de edição em caso de erro

                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("Login", "Account");
                }
                // Recarregar UserProfile para exibir dados atuais no formulário após erro de submissão
                // (pode ser o UserProfileDto original, ou chamar GetAsync novamente para ter certeza)
                var client = await GetApiClientWithAuth();
                var apiResponse = await client.GetAsync($"api/userprofile");
                if (apiResponse.IsSuccessStatusCode) {
                     viewModel.UserProfile = JsonConvert.DeserializeObject<UserProfileDto>(await apiResponse.Content.ReadAsStringAsync()) ?? new UserProfileDto();
                }

            }
            catch (JsonException ex)
            {
                viewModel.Message = $"Erro ao processar dados do perfil: {ex.Message}";
                viewModel.IsSuccess = false;
                viewModel.IsEditMode = true;
            }

            return View(viewModel);
        }
    }
}