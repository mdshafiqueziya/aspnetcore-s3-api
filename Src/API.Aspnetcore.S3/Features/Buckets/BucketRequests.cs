using System.ComponentModel.DataAnnotations;

namespace API.Aspnetcore.S3.Features.Buckets;

public sealed record CreateBucketRequest
{
    [Required]
    [StringLength(63, MinimumLength = 3)]
    public string BucketName { get; init; } = string.Empty;
}

public sealed record CreateFolderRequest
{
    [Required]
    [StringLength(63, MinimumLength = 3)]
    public string BucketName { get; init; } = string.Empty;

    [Required]
    [StringLength(1_024, MinimumLength = 1)]
    public string FolderName { get; init; } = string.Empty;
}
