namespace API.Aspnetcore.S3.Features.Buckets;

public static class BucketValidator
{
    public static Dictionary<string, string[]> Validate(CreateBucketRequest request) =>
        Validate(request.BucketName);

    public static Dictionary<string, string[]> Validate(CreateFolderRequest request)
    {
        var errors = Validate(request.BucketName);

        if (string.IsNullOrWhiteSpace(request.FolderName))
            errors[nameof(request.FolderName)] = ["Folder name is required."];
        else if (request.FolderName.Length > 1_024)
            errors[nameof(request.FolderName)] = ["Folder name cannot exceed 1024 characters."];

        return errors;
    }

    private static Dictionary<string, string[]> Validate(string bucketName)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(bucketName))
            errors[nameof(CreateBucketRequest.BucketName)] = ["Bucket name is required."];
        else if (bucketName.Length is < 3 or > 63)
            errors[nameof(CreateBucketRequest.BucketName)] = ["Bucket name must contain between 3 and 63 characters."];

        return errors;
    }
}
