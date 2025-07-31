using ECommerce.Models.DTOs.Review;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ECommerce.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> AddReviewAsync(string userId, CreateReviewRequest request);
        Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId);
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync(); // Para o Admin
    }
}