namespace Ecommerce.Models.DTOs.Tracking;

public class Evento
{
    public string? Status { get; set; }
    public string? Data { get; set; }
    public string? Hora { get; set; }
    public string? Local { get; set; }
    public string? Descricao { get; set; }
    public string? Detalhe { get; set; } // Detalhes adicionais do evento
    // Outras propriedades como Unidade, Destino, etc.
}