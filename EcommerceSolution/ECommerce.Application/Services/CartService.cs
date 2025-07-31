using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ECommerce.Models.DTOs.Cart;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CartItemDto>> GetUserCartAsync(string userId)
    {
        return await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .Include(ci => ci.Product)
            .Select(ci => new CartItemDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                ProductImageUrl = ci.Product.ImageUrl,
                Price = ci.Product.Price,
                Quantity = ci.Quantity
            })
            .ToListAsync();
    }

    public async Task<CartItemDto> AddOrUpdateCartItemAsync(string userId, AddToCartRequest request)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == request.ProductId);

        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null)
        {
            throw new Exception("Produto não encontrado."); // Ou retorne null/BadRequest
        }

        if (cartItem == null)
        {
            cartItem = new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };
            _context.CartItems.Add(cartItem);
        }
        else
        {
            cartItem.Quantity += request.Quantity; // Adiciona à quantidade existente
            // Se você quiser que a requisição POST apenas *defina* a quantidade, mude para:
            // cartItem.Quantity = request.Quantity;
        }

        if (cartItem.Quantity <= 0) // Remove se a quantidade for zero ou negativa
        {
            _context.CartItems.Remove(cartItem);
        }

        await _context.SaveChangesAsync();

        // Retorna o DTO atualizado
        return new CartItemDto
        {
            Id = cartItem.Id,
            ProductId = cartItem.ProductId,
            ProductName = product.Name,
            ProductImageUrl = product.ImageUrl,
            Price = product.Price,
            Quantity = cartItem.Quantity
        };
    }

    public async Task RemoveCartItemAsync(string userId, int productId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(string userId)
    {
        var cartItems = await _context.CartItems.Where(ci => ci.UserId == userId).ToListAsync();
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();
    }
}