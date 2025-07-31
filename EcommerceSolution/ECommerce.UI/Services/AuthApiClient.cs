using ECommerce.Models.DTOs.User;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ECommerce.Client.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResult> Login(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/login", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResult>() ?? new LoginResult { Success = false, Message = "Resposta vazia." };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Tentar deserializar o erro da API, se for um formato conhecido (ex: ValidationProblemDetails)
                return new LoginResult { Success = false, Message = $"Login falhou: {response.StatusCode}. Detalhes: {errorContent}" };
            }
        }

        public async Task<bool> Register(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/register", request);
            response.EnsureSuccessStatusCode(); // Lança exceção para status code de erro
            return response.IsSuccessStatusCode;
        }
    }
}