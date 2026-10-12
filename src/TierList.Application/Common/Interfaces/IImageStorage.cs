namespace TierList.Application.Common.Interfaces;

public interface IImageStorage
{
    Task<string> SaveAsync(Stream stream, string contentType, CancellationToken cancellationToken = default);
}
