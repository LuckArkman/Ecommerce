namespace ECommerce.Models.DTOs.Product
{
    public class ProductQueryParams
    {
        public int? CategoryId { get; set; }
        public string? OrderBy { get; set; }
        public string? SearchTerm { get; set; }
    }
}