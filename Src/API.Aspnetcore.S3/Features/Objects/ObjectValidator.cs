namespace API.Aspnetcore.S3.Features.Objects;

public static class ObjectValidator
{
    public static Dictionary<string, string[]> Validate(string bucketName, string key)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(bucketName))
            errors[nameof(bucketName)] = ["Bucket name is required."];
        if (string.IsNullOrWhiteSpace(key))
            errors[nameof(key)] = ["Object key is required."];

        return errors;
    }

    public static Dictionary<string, string[]> ValidateExpiry(int expiryMinutes)
    {
        var errors = new Dictionary<string, string[]>();
        if (expiryMinutes is < 1 or > 60)
            errors[nameof(expiryMinutes)] = ["Expiry must be between 1 and 60 minutes."];
        return errors;
    }
}
