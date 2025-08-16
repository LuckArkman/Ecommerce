using System.Text.Json;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;

namespace ECommerce.Application.Services;

public class ShippingService : IShippingService
{
    private readonly ApplicationDbContext _context;

    public ShippingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsFreeShipping(int productId, string clientZipCode)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || !product.IsFreeShipping)
        {
            return false; // Produto não existe ou não tem frete grátis
        }

        if (string.IsNullOrEmpty(product.FreeShippingRegionsJson))
        {
            // Se IsFreeShipping é true mas não há regiões específicas, é grátis para todos
            return true;
        }

        try
        {
            // Exemplo: FreeShippingRegionsJson contém um array JSON de UFs (estados)
            // Ou poderia ser um array de faixas de CEP (ex: ["00000-000-00999-999", "10000-000-19999-999"])
            var regions = JsonSerializer.Deserialize<List<string>>(product.FreeShippingRegionsJson);
            if (regions == null || !regions.Any()) return false;

            // Obtenha a UF do CEP do cliente (você precisaria de um serviço de CEP para isso)
            // Para simplicidade, vamos SIMULAR que o cliente já forneceu a UF ou que o serviço de CEP existe.
            // Digamos que clientZipCode é "01000-000" e você precisa extrair a UF "SP".
            // Ou, para teste, que clientZipCode é a própria UF (ex: "SP").
            string clientUF = clientZipCode.Length >= 2 ? clientZipCode.Substring(0, 2) : clientZipCode; // Simplificado

            return regions.Any(r => r.Equals(clientUF, StringComparison.OrdinalIgnoreCase) || 
                                    clientZipCode.StartsWith(r)); // Se região for um prefixo de CEP
        }
        catch (JsonException)
        {
            // Erro ao deserializar o JSON das regiões, tratar como não grátis.
            return false;
        }
    }
}