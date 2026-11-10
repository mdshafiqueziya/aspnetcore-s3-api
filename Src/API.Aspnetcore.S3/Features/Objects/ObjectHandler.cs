using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;

namespace API.Aspnetcore.S3.Features.Objects;

public sealed class ObjectHandler(IObjectService service)
{
    public Task<string> UploadAsync(string bucketName, IFormFile file, CancellationToken cancellationToken) =>
        service.UploadFileAsync(bucketName, file, cancellationToken);

    public Task<List<string>> UploadManyAsync(string bucketName, List<IFormFile> files, CancellationToken cancellationToken) =>
        service.UploadFilesAsync(bucketName, files, cancellationToken);

    public Task<GetObjectResponse> DownloadAsync(string bucketName, string key, CancellationToken cancellationToken) =>
        service.DownloadFileAsync(bucketName, key, cancellationToken);

    public Task<ObjectResponse> ListAsync(string bucketName, CancellationToken cancellationToken) =>
        service.GetObjectsAsync(bucketName, cancellationToken);

    public Task<string> CreateDownloadUrlAsync(string bucketName, string key, int expiryMinutes) =>
        service.GenerateDownloadUrlAsync(bucketName, key, expiryMinutes);
}
