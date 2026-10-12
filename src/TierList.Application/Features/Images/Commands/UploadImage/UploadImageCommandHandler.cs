using MediatR;
using TierList.Application.Common.Interfaces;

namespace TierList.Application.Features.Images.Commands.UploadImage;

public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, string>
{
    private readonly IImageStorage _imageStorage;

    public UploadImageCommandHandler(IImageStorage imageStorage)
    {
        _imageStorage = imageStorage;
    }

    public async Task<string> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        return await _imageStorage.SaveAsync(request.Content, request.ContentType, cancellationToken);
    }
}
