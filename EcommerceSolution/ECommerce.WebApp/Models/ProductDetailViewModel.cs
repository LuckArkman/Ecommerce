using ECommerce.Models.DTOs.Product;
using ECommerce.Models.DTOs.Review;

namespace ECommerce.WebApp.Models;

public class ProductDetailViewModel
{
    public ProductDto? Product { get; set; }
    public List<ProductDto>? RelatedProducts { get; set; }
    public List<ReviewDto>? Reviews { get; set; }
    public double AverageRating { get; set; }
    public Dictionary<int, int>? RatingCounts { get; set; }
    public int TotalReviews { get; set; }
    public CreateReviewRequest NewReview { get; set; } = new();
}