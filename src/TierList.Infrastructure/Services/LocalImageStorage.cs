using Microsoft.Extensions.Configuration;
using TierList.Application.Common.Interfaces;

namespace TierList.Infrastructure.Services;

public class LocalImageStorage : IImageStorage
{
    private readonly string _uploadDirectory;
    private readonly string _baseUrl;

    public LocalImageStorage(IConfiguration configuration)
    {
        var configuredPath = configuration["ImageStorage:UploadDirectory"];
        _uploadDirectory = !string.IsNullOrWhiteSpace(configuredPath)
            ? configuredPath
            : Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        _baseUrl = configuration["ImageStorage:BaseUrl"] ?? "/uploads";

        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }
    }

    public async Task<string> SaveAsync(Stream stream, string contentType, CancellationToken cancellationToken = default)
    {
        var extension = GetExtension(contentType);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_uploadDirectory, fileName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await stream.CopyToAsync(fileStream, cancellationToken);

        var url = $"{_baseUrl.TrimEnd('/')}/{fileName}";
        return url;
    }

    private static string GetExtension(string contentType) =>
        contentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" or "image/pjpeg" => ".jpg",
            "image/png" or "image/x-png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/avif" => ".avif",
            _ => ".png"
        };
}
