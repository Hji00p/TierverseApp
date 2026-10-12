using MediatR;

namespace TierList.Application.Features.Images.Commands.UploadImage;

public record UploadImageCommand(Stream Content, string ContentType, long Length) : IRequest<string>;
