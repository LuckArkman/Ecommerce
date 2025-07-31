// ECommerce.Application/Interfaces/IReviewService.cs

using ECommerce.Models.DTOs.Review;
using Microsoft.EntityFrameworkCore;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity; // Para obter nome do usuário

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ReviewDto> AddReviewAsync(string userId, CreateReviewRequest request)
    {
        var review = new Review
        {
            ProductId = request.ProductId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var product = await _context.Products.FindAsync(request.ProductId);
        var user = await _userManager.FindByIdAsync(userId);

        return new ReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            ProductName = product?.Name,
            UserName = user?.UserName,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId)
    {
        return await _context.Reviews
            .Where(r => r.ProductId == productId)
            .Include(r => r.Product)
            .Include(r => r.User) // Inclui o usuário para pegar o nome
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                UserName = r.User.UserName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
    {
        return await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.User)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                UserName = r.User.UserName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}