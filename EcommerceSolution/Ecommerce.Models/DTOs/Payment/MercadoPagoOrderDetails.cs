using Newtonsoft.Json;

namespace Ecommerce.Models.DTOs.Payment;

public class MercadoPagoOrderDetails
{
    [JsonProperty("id")]
    public long Id { get; set; } // ID da ordem do comerciante
    // Adicione outras propriedades se Order tiver
}