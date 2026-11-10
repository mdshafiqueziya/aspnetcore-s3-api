namespace API.Aspnetcore.S3.Features.Objects;

public record ObjectResponse(string BucketName, List<string> Keys);
