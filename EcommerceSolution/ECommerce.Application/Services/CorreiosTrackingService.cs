using ECommerce.Application.Interfaces;
using Ecommerce.Models.DTOs.Tracking;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace ECommerce.Application.Services;

public class CorreiosTrackingService : ITrackingService
{
    private readonly HttpClient _httpClient; // Usar HttpClient diretamente para chamadas externas
    private readonly IConfiguration _configuration;

    public CorreiosTrackingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        // Definir a base URL para a API dos Correios (se for fixa)
        _httpClient.BaseAddress = new Uri(_configuration["CorreiosApi:BaseUrl"] ?? "https://proxyapp.correios.com.br/v1/"); // URL de exemplo
    }

    public async Task<TrackingResultDto> TrackOrderAsync(string trackingNumber)
    {
        var result = new TrackingResultDto { TrackingNumber = trackingNumber };

        // A API oficial dos Correios (SRO) pode ter requisitos de autenticação/tokens.
        // As URLs e métodos podem variar, consulte a documentação mais recente dos Correios.
        // Exemplo de URL: https://proxyapp.correios.com.br/v1/sro-objeto/{codigo_rastreamento}
        var requestUrl = $"sro-objeto/{trackingNumber}"; // Exemplo

        try
        {
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var correiosResponse = JsonConvert.DeserializeObject<CorreiosTrackingResponse>(content);

            if (correiosResponse?.Objetos?.Any() == true)
            {
                var objeto = correiosResponse.Objetos.FirstOrDefault();
                if (objeto != null)
                {
                    result.OrderStatus = objeto.Status ?? "Status Desconhecido";
                    result.IsDelivered = objeto.Status?.Contains("entregue", StringComparison.OrdinalIgnoreCase) ?? false;

                    // Mapear eventos
                    result.Events = objeto.Eventos
                        .OrderByDescending(e => DateTime.Parse($"{e.Data} {e.Hora}")) // Ordenar por data/hora
                        .Select(e => new TrackingEventDto
                        {
                            Status = e.Status ?? "N/A",
                            Description = e.Descricao ?? e.Detalhe ?? "Sem descrição",
                            DateTime = $"{e.Data} {e.Hora}",
                            Location = e.Local ?? "N/A"
                        })
                        .ToList();
                }
            }
            else
            {
                result.IsError = true;
                result.ErrorMessage = "Código de rastreamento não encontrado ou sem eventos.";
            }
        }
        catch (HttpRequestException ex)
        {
            result.IsError = true;
            result.ErrorMessage = $"Erro de comunicação com a API dos Correios: {ex.Message}";
            // Se 404, pode ser código inválido; se 500, problema na API deles.
        }
        catch (JsonException ex)
        {
            result.IsError = true;
            result.ErrorMessage = $"Erro ao processar resposta dos Correios: {ex.Message}";
        }
        catch (Exception ex)
        {
            result.IsError = true;
            result.ErrorMessage = $"Ocorreu um erro inesperado: {ex.Message}";
        }

        return result;
    }
}