namespace ECommerce.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Ex: "Pending", "Processing", "Shipped", "Delivered"
        public string? TrackingNumber { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}