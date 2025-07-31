using ECommerce.Models.DTOs.Review;

namespace ECommerce.Application.DTOs.Dashboard;

public class CustomerSatisfactionMetricDto
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int PositiveReviews { get; set; } // Ex: Rating >= 4
    public int NegativeReviews { get; set; } // Ex: Rating <= 2
    public int NeutralReviews { get; set; } // Ex: Rating == 3
    public List<ReviewDto> RecentReviews { get; set; } // Ultimas N reviews
}