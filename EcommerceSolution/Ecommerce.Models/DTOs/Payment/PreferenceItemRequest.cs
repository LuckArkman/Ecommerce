namespace Ecommerce.Models.DTOs.Payment;

public class PreferenceItemRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public string CurrencyId { get; set; } // Geralmente "BRL"
    public decimal UnitPrice { get; set; }
    // Outras propriedades como PictureUrl, CategoryId, etc.
}