using Amazon.S3;
using Amazon.S3.Model;

namespace API.Aspnetcore.S3.Features.Buckets;

public interface IBucketService
{
    Task CreateBucketAsync(string bucketName, CancellationToken cancellationToken = default);

    Task<List<string>> GetBucketsAsync();

    Task DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default);

    Task<string> GetBucketLocationAsync(string bucketName, CancellationToken cancellationToken = default);

    Task CreateFolderAsync(string bucketName, string folderName, CancellationToken cancellationToken = default);

}

public class BucketService(IAmazonS3 _amazonS3) : IBucketService
{

    public async Task CreateBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        ValidateRequired(bucketName, nameof(bucketName));
        await _amazonS3.EnsureBucketExistsAsync(bucketName);
    }

    public async Task CreateFolderAsync(string bucketName, string folderName, CancellationToken cancellationToken = default)
    {
        ValidateRequired(bucketName, nameof(bucketName));
        ValidateRequired(folderName, nameof(folderName));

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = folderName.Trim('/').Replace('\\', '/') + "/",
            ContentBody = ""
        };

        await _amazonS3.PutObjectAsync(request, cancellationToken);
    }

    public async Task DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        ValidateRequired(bucketName, nameof(bucketName));
        await _amazonS3.DeleteBucketAsync(bucketName, cancellationToken);
    }

    public async Task<string> GetBucketLocationAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        ValidateRequired(bucketName, nameof(bucketName));
        var response = await _amazonS3.GetBucketLocationAsync(bucketName, cancellationToken);

        return response.Location.Value;
    }

    public async Task<List<string>> GetBucketsAsync()
    {
        var response = await _amazonS3.ListBucketsAsync();

        return [.. response.Buckets.Select(x => x.BucketName)];
    }

    private static void ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A value is required.", parameterName);
    }
}
