using ECommerce.Models.DTOs.Order;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderRequest request)
    {
        if (!request.CartItems.Any())
        {
            throw new ArgumentException("Carrinho não pode estar vazio.");
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            ShippingAddress = request.ShippingAddress,
            Status = "Pending", // Status inicial
            OrderItems = new List<OrderItem>()
        };

        foreach (var cartItemDto in request.CartItems)
        {
            var product = await _context.Products.FindAsync(cartItemDto.ProductId);
            if (product == null || product.Stock < cartItemDto.Quantity)
            {
                throw new InvalidOperationException($"Produto {cartItemDto.ProductName} sem estoque suficiente.");
            }

            order.OrderItems.Add(new OrderItem
            {
                ProductId = cartItemDto.ProductId,
                Quantity = cartItemDto.Quantity,
                Price = cartItemDto.Price // Preço no momento da compra
            });
            product.Stock -= cartItemDto.Quantity; // Atualiza o estoque
        }

        order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Limpar o carrinho após a criação do pedido (opcional, mas comum)
        var cartItemsToRemove = await _context.CartItems.Where(ci => ci.UserId == userId).ToListAsync();
        _context.CartItems.RemoveRange(cartItemsToRemove);
        await _context.SaveChangesAsync();


        // Mapear para DTO de retorno
        var orderDto = new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            Status = order.Status,
            TrackingNumber = order.TrackingNumber,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = _context.Products.Find(oi.ProductId)?.Name, // Obter nome do produto
                ProductImageUrl = _context.Products.Find(oi.ProductId)?.ImageUrl,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        };

        return orderDto;
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)!
                .ThenInclude(oi => oi.Product) // Incluir dados do produto para OrderItems
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                ShippingAddress = o.ShippingAddress,
                Status = o.Status,
                TrackingNumber = o.TrackingNumber,
                OrderItems = o.OrderItems!.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product!.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<OrderDto> GetOrderByIdAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)!
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return null;

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            Status = order.Status,
            TrackingNumber = order.TrackingNumber,
            OrderItems = order.OrderItems!.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product!.Name,
                ProductImageUrl = oi.Product.ImageUrl,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        };
    }

    public async Task UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return;

        order.Status = request.Status;
        order.TrackingNumber = request.TrackingNumber;
        _context.Entry(order).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)!
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                ShippingAddress = o.ShippingAddress,
                Status = o.Status,
                TrackingNumber = o.TrackingNumber,
                OrderItems = o.OrderItems!.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product!.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            })
            .ToListAsync();
    }
}