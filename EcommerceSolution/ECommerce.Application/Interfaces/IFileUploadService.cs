using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Interfaces;

public interface IFileUploadService
{
    Task<string> UploadFileAsync(IFormFile file, string folderName);
}