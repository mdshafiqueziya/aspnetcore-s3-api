using API.Aspnetcore.S3.Features.Buckets;

namespace API.Aspnetcore.S3.Tests.Features.Buckets;

public class BucketValidatorTests
{
    [Fact]
    public void CreateBucket_WithValidName_ReturnsNoErrors()
    {
        var errors = BucketValidator.Validate(new CreateBucketRequest { BucketName = "demo-bucket" });

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void CreateBucket_WithInvalidName_ReturnsBucketError(string bucketName)
    {
        var errors = BucketValidator.Validate(new CreateBucketRequest { BucketName = bucketName });

        Assert.Contains(nameof(CreateBucketRequest.BucketName), errors.Keys);
    }

    [Fact]
    public void CreateFolder_WithoutFolderName_ReturnsFolderError()
    {
        var errors = BucketValidator.Validate(new CreateFolderRequest { BucketName = "demo-bucket" });

        Assert.Contains(nameof(CreateFolderRequest.FolderName), errors.Keys);
    }
}
