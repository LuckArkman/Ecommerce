namespace ECommerce.Application.Interfaces;


public interface IShippingService
{
    Task<bool> IsFreeShipping(int productId, string clientZipCode);
}