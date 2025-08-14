using Ecommerce.Models.DTOs.Tracking;

namespace ECommerce.Application.Interfaces;

public interface ITrackingService
{
    Task<TrackingResultDto> TrackOrderAsync(string trackingNumber);
}