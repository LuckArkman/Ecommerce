using ECommerce.Models.DTOs.Review;

namespace ECommerce.Models.DTOs.Dashboard;

public class CustomerSatisfactionMetricDto
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int PositiveReviews { get; set; }
    public int NegativeReviews { get; set; }
    public int NeutralReviews { get; set; }
    public List<ReviewDto> RecentReviews { get; set; } = new List<ReviewDto>();
}