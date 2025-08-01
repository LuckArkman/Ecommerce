using ECommerce.Models.DTOs.Review; // Importa os DTOs de Review do projeto Models

namespace ECommerce.Client.Services
{
    public class ReviewApiClient
    {
        private readonly HttpClient _httpClient;

        public ReviewApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Adiciona uma nova avaliação/comentário para um produto.
        /// </summary>
        /// <param name="request">Os dados da nova avaliação.</param>
        /// <returns>A avaliação criada.</returns>
        public async Task<ReviewDto> AddReview(CreateReviewRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/reviews", request);
            response.EnsureSuccessStatusCode(); // Lança HttpRequestException para status de erro (4xx, 5xx)
            return await response.Content.ReadFromJsonAsync<ReviewDto>() ?? throw new InvalidOperationException("Resposta da API de review foi nula.");
        }

        /// <summary>
        /// Obtém todas as avaliações de um produto específico.
        /// </summary>
        /// <param name="productId">ID do produto.</param>
        /// <returns>Uma lista de ReviewDto para o produto.</returns>
        public async Task<List<ReviewDto>> GetReviewsByProductId(int productId)
        {
            return await _httpClient.GetFromJsonAsync<List<ReviewDto>>($"api/reviews/product/{productId}") ?? new List<ReviewDto>();
        }

        /// <summary>
        /// (Admin) Obtém todas as avaliações de todos os produtos.
        /// Requer autenticação com papel de Administrador na API.
        /// </summary>
        /// <returns>Uma lista de todos os ReviewDto.</returns>
        public async Task<List<ReviewDto>> GetAllReviews()
        {
            return await _httpClient.GetFromJsonAsync<List<ReviewDto>>("api/reviews/all") ?? new List<ReviewDto>();
        }
    }
}