using Amazon.S3;
using Amazon.S3.Model;

namespace API.Aspnetcore.S3.Features.Objects;

public interface IObjectService
{
    Task<string> UploadFileAsync(string bucketName, IFormFile file, CancellationToken cancellationToken = default);

    Task<List<string>> UploadFilesAsync(string bucketName, List<IFormFile> files, CancellationToken cancellationToken = default);
    Task<GetObjectResponse> DownloadFileAsync(string bucketName, string key, CancellationToken cancellationToken = default);

    Task<ObjectResponse> GetObjectsAsync(string bucketName, CancellationToken cancellationToken = default);

    Task DeleteObjectAsync(string bucketName, string key, CancellationToken cancellationToken = default);

    Task DeleteObjectsAsync(string bucketName, List<string> keys, CancellationToken cancellationToken = default);

    Task<string> GenerateDownloadUrlAsync(string bucketName,
                                         string key,
                                         int expiryMinutes);

    Task<string> GenerateUploadUrlAsync(string bucketName,
                                        string key,
                                        int expiryMinutes);
}

public class ObjectService(IAmazonS3 _amazonS3) : IObjectService
{
    private const long MaxFileSize = 50_000_000;
    private const int MaxUrlExpiryMinutes = 60;

    public async Task DeleteObjectAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        ValidateRequired(bucketName, nameof(bucketName));
        ValidateRequired(key, nameof(key));
        await _amazonS3.DeleteObjectAsync(bucketName, key, cancellationToken);
    }

    public async Task DeleteObjectsAsync(string bucketName, List<string> keys, CancellationToken cancellationToken = default)
    {
        ValidateRequired(bucketName, nameof(bucketName));
        if (keys is null || keys.Count == 0)
            throw new ArgumentException("At least one object key is required.", nameof(keys));

        var request = new DeleteObjectsRequest
        {
            BucketName = bucketName,
            Objects = [.. keys
            .Select(x => new KeyVersion
            {
                Key = x
            })]
        };

        await _amazonS3.DeleteObjectsAsync(request, cancellationToken);
    }

    public async Task<GetObjectResponse> DownloadFileAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        ValidateRequired(bucketName, nameof(bucketName));
        ValidateRequired(key, nameof(key));
        return await _amazonS3.GetObjectAsync(bucketName, key, cancellationToken);
    }

    public async Task<string> GenerateDownloadUrlAsync(string bucketName, string key, int expiryMinutes)
    {
        ValidateUrlArguments(bucketName, key, expiryMinutes);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };

        return await _amazonS3.GetPreSignedURLAsync(request);
    }

    public Task<string> GenerateUploadUrlAsync(string bucketName, string key, int expiryMinutes)
    {
        ValidateUrlArguments(bucketName, key, expiryMinutes);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };

        return Task.FromResult(_amazonS3.GetPreSignedURL(request));
    }

    public async Task<ObjectResponse> GetObjectsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        ValidateRequired(bucketName, nameof(bucketName));

        var keys = new List<string>();
        string? continuationToken = null;

        do
        {
            var response = await _amazonS3.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = bucketName,
                ContinuationToken = continuationToken
            }, cancellationToken);

            keys.AddRange(response.S3Objects.Select(x => x.Key));
            continuationToken = response.IsTruncated == true ? response.NextContinuationToken : null;
        }
        while (continuationToken is not null);

        return new ObjectResponse(bucketName, keys);
    }

    public async Task<string> UploadFileAsync(string bucketName, IFormFile file, CancellationToken cancellationToken = default)
    {
        ValidateRequired(bucketName, nameof(bucketName));
        ArgumentNullException.ThrowIfNull(file);

        if (file.Length > MaxFileSize)
            throw new ArgumentException($"Files cannot exceed {MaxFileSize / 1_000_000} MB.", nameof(file));

        var key = CreateObjectKey(file.FileName);
        using var stream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = stream,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType
        };

        await _amazonS3.PutObjectAsync(request, cancellationToken);

        return key;
    }

    public async Task<List<string>> UploadFilesAsync(string bucketName, List<IFormFile> files, CancellationToken cancellationToken = default)
    {
        ValidateRequired(bucketName, nameof(bucketName));
        if (files is null || files.Count == 0)
            throw new ArgumentException("At least one file is required.", nameof(files));

        var uploadedFiles = new List<string>();

        foreach (var file in files)
        {
            uploadedFiles.Add(await UploadFileAsync(bucketName, file, cancellationToken));
        }

        return uploadedFiles;
    }

    private static string CreateObjectKey(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
            throw new ArgumentException("The uploaded file must have a name.", nameof(fileName));

        return $"uploads/{Guid.NewGuid():N}/{safeFileName}";
    }

    private static void ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A value is required.", parameterName);
    }

    private static void ValidateUrlArguments(string bucketName, string key, int expiryMinutes)
    {
        ValidateRequired(bucketName, nameof(bucketName));
        ValidateRequired(key, nameof(key));

        if (expiryMinutes is < 1 or > MaxUrlExpiryMinutes)
            throw new ArgumentOutOfRangeException(nameof(expiryMinutes), $"Expiry must be between 1 and {MaxUrlExpiryMinutes} minutes.");
    }
}
