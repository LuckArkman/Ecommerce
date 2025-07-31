using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using ECommerce.Models.DTOs.Review;

[Route("api/[controller]")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpPost]
    [Authorize] // Usuários logados podem adicionar reviews
    public async Task<ActionResult<ReviewDto>> AddReview([FromBody] CreateReviewRequest request)
    {
        var review = await _reviewService.AddReviewAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetReviewsByProductId), new { productId = review.ProductId }, review);
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByProductId(int productId)
    {
        var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
        return Ok(reviews);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")] // Apenas admin pode ver todos os reviews
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetAllReviews()
    {
        var reviews = await _reviewService.GetAllReviewsAsync();
        return Ok(reviews);
    }
}