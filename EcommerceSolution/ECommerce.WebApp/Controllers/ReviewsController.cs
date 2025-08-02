using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using ECommerce.Models.DTOs.Review;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce.WebApp.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ReviewsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var token = HttpContext.Session.GetString("JwtToken"); // Ou de um cookie seguro
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            try
            {
                var response = await client.PostAsJsonAsync("api/reviews", request);
                response.EnsureSuccessStatusCode();
                var reviewDto = JsonConvert.DeserializeObject<ReviewDto>(await response.Content.ReadAsStringAsync());
                return Ok(reviewDto);
            }
            catch (HttpRequestException ex)
            {
                var errorContent = ex.HResult.ToString();
                return StatusCode((int)(ex.StatusCode), new { message = errorContent ?? "Erro ao adicionar avaliação na API." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReviewsByProduct(int productId, string filter = "all", string order = "newest")
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            List<ReviewDto> reviews = new();

            try
            {
                var response = await client.GetAsync($"api/reviews/product/{productId}");
                response.EnsureSuccessStatusCode();
                reviews = JsonConvert.DeserializeObject<List<ReviewDto>>(await response.Content.ReadAsStringAsync());

                // Lógica de filtro e ordenação no backend, mas pode ser aplicada aqui para demonstrar o frontend
                // Filter: "all", "withComments"
                if (filter == "withComments")
                {
                    reviews = reviews.Where(r => !string.IsNullOrEmpty(r.Comment)).ToList();
                }

                // Order: "newest", "oldest", "highest", "lowest"
                switch (order)
                {
                    case "oldest": reviews = reviews.OrderBy(r => r.CreatedAt).ToList(); break;
                    case "highest": reviews = reviews.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt).ToList(); break;
                    case "lowest": reviews = reviews.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt).ToList(); break;
                    case "newest":
                    default: reviews = reviews.OrderByDescending(r => r.CreatedAt).ToList(); break;
                }
            }
            catch (HttpRequestException ex)
            {
                // Logar o erro
            }
            return Json(reviews);
        }
    }
}