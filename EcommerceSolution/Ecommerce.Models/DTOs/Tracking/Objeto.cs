namespace Ecommerce.Models.DTOs.Tracking;

public class Objeto
{
    public string? Codigo { get; set; } // Código de rastreamento
    public string? Tipo { get; set; }
    public string? Status { get; set; }
    public string? Data { get; set; } // Data do último evento
    public string? Hora { get; set; } // Hora do último evento
    public string? Local { get; set; } // Local do último evento
    public string? Original { get; set; } // Mensagem original do status
    public List<Evento> Eventos { get; set; } = new List<Evento>();
}