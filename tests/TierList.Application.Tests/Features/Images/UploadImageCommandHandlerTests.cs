using FluentAssertions;
using Moq;
using TierList.Application.Common.Interfaces;
using TierList.Application.Features.Images.Commands.UploadImage;
using Xunit;

namespace TierList.Application.Tests.Features.Images;

public class UploadImageCommandHandlerTests
{
    private readonly Mock<IImageStorage> _imageStorageMock = new();

    [Fact]
    public async Task Handle_ValidRequest_ShouldCallImageStorageAndReturnUrl()
    {
        // Arrange
        var expectedUrl = "/uploads/abc123xyz.png";
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var contentType = "image/png";

        _imageStorageMock
            .Setup(s => s.SaveAsync(stream, contentType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrl);

        var handler = new UploadImageCommandHandler(_imageStorageMock.Object);
        var command = new UploadImageCommand(stream, contentType, 3);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedUrl);
        _imageStorageMock.Verify(s => s.SaveAsync(stream, contentType, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void Validator_AllowedContentTypes_ShouldPass(string contentType)
    {
        var validator = new UploadImageCommandValidator();
        var command = new UploadImageCommand(new MemoryStream(), contentType, 1024);

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/tiff")]
    [InlineData("text/plain")]
    public void Validator_UnsupportedContentType_ShouldFail(string contentType)
    {
        var validator = new UploadImageCommandValidator();
        var command = new UploadImageCommand(new MemoryStream(), contentType, 1024);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ContentType");
    }

    [Fact]
    public void Validator_FileTooLarge_ShouldFail()
    {
        var validator = new UploadImageCommandValidator();
        var oversized = (10 * 1024 * 1024) + 1; // 10MB + 1 byte
        var command = new UploadImageCommand(new MemoryStream(), "image/png", oversized);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Length");
    }

    [Fact]
    public void Validator_EmptyFile_ShouldFail()
    {
        var validator = new UploadImageCommandValidator();
        var command = new UploadImageCommand(new MemoryStream(), "image/png", 0);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Length");
    }
}
