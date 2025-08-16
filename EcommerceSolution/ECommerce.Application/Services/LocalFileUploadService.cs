using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Services;

public class LocalFileUploadService : IFileUploadService
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<LocalFileUploadService> _logger;
    private readonly IConfiguration _configuration;
    
    // Lista de extensões permitidas
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };
    
    // Tamanho máximo em bytes (5MB)
    private const long MaxFileSize = 5 * 1024 * 1024;

    public LocalFileUploadService(
        IHostEnvironment hostEnvironment, 
        ILogger<LocalFileUploadService> logger,
        IConfiguration configuration)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName)
    {
        // Validações básicas
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Arquivo não fornecido.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException($"Arquivo muito grande. Tamanho máximo permitido: {MaxFileSize / (1024 * 1024)}MB");
        }

        // Validar extensão do arquivo
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException($"Tipo de arquivo não permitido. Extensões permitidas: {string.Join(", ", _allowedExtensions)}");
        }

        // Sanitizar o nome da pasta
        folderName = SanitizeFolderName(folderName);

        try
        {
            // Usar ContentRootPath como base e criar uma pasta de uploads
            var webRootPath = GetWebRootPath();
            var uploadsFolder = Path.Combine(webRootPath, folderName);
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
                _logger.LogInformation($"Diretório criado: {uploadsFolder}");
            }

            // Gerar nome único para o arquivo
            var uniqueFileName = GenerateUniqueFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Verificar se o caminho final está dentro do diretório esperado (segurança)
            var fullUploadsPath = Path.GetFullPath(uploadsFolder);
            var fullFilePath = Path.GetFullPath(filePath);
            if (!fullFilePath.StartsWith(fullUploadsPath))
            {
                throw new SecurityException("Tentativa de escrita fora do diretório permitido.");
            }

            // Salvar o arquivo
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            _logger.LogInformation($"Arquivo salvo com sucesso: {uniqueFileName}");

            // Retorna o caminho relativo para uso em URLs
            return $"/{folderName}/{uniqueFileName}";
        }
        catch (DirectoryNotFoundException ex)
        {
            _logger.LogError(ex, "Diretório não encontrado durante upload do arquivo");
            throw new InvalidOperationException("Erro ao acessar diretório de upload", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Acesso negado durante upload do arquivo");
            throw new InvalidOperationException("Permissão negada para salvar arquivo", ex);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Erro de I/O durante upload do arquivo");
            throw new InvalidOperationException("Erro ao salvar arquivo", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante upload do arquivo");
            throw;
        }
    }

    private string GetWebRootPath()
    {
        // Tenta obter o caminho do wwwroot da configuração
        var webRootPath = _configuration["WebRootPath"];
        
        if (!string.IsNullOrEmpty(webRootPath))
        {
            return webRootPath;
        }
        
        // Fallback: usa ContentRootPath + wwwroot
        var contentRoot = _hostEnvironment.ContentRootPath;
        return Path.Combine(contentRoot, "wwwroot");
    }

    private string SanitizeFolderName(string folderName)
    {
        // Remove caracteres perigosos do nome da pasta
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(folderName.Where(c => !invalidChars.Contains(c)).ToArray());
        
        // Remove pontos no início ou final (segurança adicional)
        sanitized = sanitized.Trim('.');
        
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            throw new ArgumentException("Nome da pasta inválido após sanitização.");
        }
        
        return sanitized;
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        var fileExtension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        
        // Sanitizar o nome original
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitizedName = new string(fileNameWithoutExtension.Where(c => !invalidChars.Contains(c)).ToArray());
        
        // Limitar o tamanho do nome
        if (sanitizedName.Length > 50)
        {
            sanitizedName = sanitizedName.Substring(0, 50);
        }
        
        // Gerar nome único
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Primeiros 8 caracteres
        
        return $"{sanitizedName}_{timestamp}_{uniqueId}{fileExtension}";
    }
}

// Exceção personalizada para problemas de segurança
public class SecurityException : Exception
{
    public SecurityException(string message) : base(message) { }
    public SecurityException(string message, Exception innerException) : base(message, innerException) { }
}