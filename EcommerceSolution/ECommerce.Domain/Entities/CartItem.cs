namespace ECommerce.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // FK para ApplicationUser
        public ApplicationUser? User { get; set; } // Propriedade de navegação
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}