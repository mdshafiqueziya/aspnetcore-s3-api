namespace API.Aspnetcore.S3.Features.Buckets;

public sealed class BucketHandler(IBucketService service)
{
    public Task CreateAsync(string bucketName, CancellationToken cancellationToken) =>
        service.CreateBucketAsync(bucketName, cancellationToken);

    public Task CreateFolderAsync(string bucketName, string folderName, CancellationToken cancellationToken) =>
        service.CreateFolderAsync(bucketName, folderName, cancellationToken);

    public Task<List<string>> ListAsync() => service.GetBucketsAsync();

    public Task DeleteAsync(string bucketName, CancellationToken cancellationToken) =>
        service.DeleteBucketAsync(bucketName, cancellationToken);

    public Task<string> GetLocationAsync(string bucketName, CancellationToken cancellationToken) =>
        service.GetBucketLocationAsync(bucketName, cancellationToken);
}
