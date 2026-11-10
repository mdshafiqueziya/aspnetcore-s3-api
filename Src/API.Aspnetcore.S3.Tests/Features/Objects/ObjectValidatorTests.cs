using API.Aspnetcore.S3.Features.Objects;

namespace API.Aspnetcore.S3.Tests.Features.Objects;

public class ObjectValidatorTests
{
    [Fact]
    public void ValidObjectRequest_ReturnsNoErrors()
    {
        var errors = ObjectValidator.Validate("demo-bucket", "object.txt");

        Assert.Empty(errors);
    }

    [Fact]
    public void MissingBucketOrKey_ReturnsBothErrors()
    {
        var errors = ObjectValidator.Validate("", "");

        Assert.Contains("bucketName", errors.Keys);
        Assert.Contains("key", errors.Keys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(61)]
    public void InvalidExpiry_ReturnsExpiryError(int expiryMinutes)
    {
        var errors = ObjectValidator.ValidateExpiry(expiryMinutes);

        Assert.Contains("expiryMinutes", errors.Keys);
    }
}
