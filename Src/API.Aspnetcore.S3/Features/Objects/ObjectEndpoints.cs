using Microsoft.AspNetCore.Http;

namespace API.Aspnetcore.S3.Features.Objects;

public static class ObjectEndpoints
{
    public static IEndpointRouteBuilder MapObjectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/objects").WithTags("Objects");

        group.MapPost("/upload/{bucketName}", async (string bucketName, IFormFile? file, ObjectHandler handler, HttpContext context) =>
        {
            if (file is null)
                return Results.BadRequest("A file is required.");

            var errors = ObjectValidator.Validate(bucketName, file.FileName);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var key = await handler.UploadAsync(bucketName, file, context.RequestAborted);
            return Results.Created($"/api/objects/download?bucket={bucketName}&key={Uri.EscapeDataString(key)}", key);
        }).DisableAntiforgery();

        group.MapPost("/uploads/{bucketName}", async (string bucketName, List<IFormFile>? files, ObjectHandler handler, HttpContext context) =>
        {
            if (files is null || files.Count == 0)
                return Results.BadRequest("At least one file is required.");
            if (files.Count > 20)
                return Results.BadRequest("A maximum of 20 files can be uploaded at once.");

            var errors = ObjectValidator.Validate(bucketName, "batch");
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var keys = await handler.UploadManyAsync(bucketName, files, context.RequestAborted);
            return Results.Created($"/api/objects?bucketName={Uri.EscapeDataString(bucketName)}", keys);
        }).DisableAntiforgery();

        group.MapGet("/", async (string bucketName, ObjectHandler handler, HttpContext context) =>
        {
            var errors = ObjectValidator.Validate(bucketName, "list");
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);
            return Results.Ok(await handler.ListAsync(bucketName, context.RequestAborted));
        });

        group.MapGet("/download", async (string bucket, string key, ObjectHandler handler, HttpContext context) =>
        {
            var errors = ObjectValidator.Validate(bucket, key);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var response = await handler.DownloadAsync(bucket, key, context.RequestAborted);
            return Results.File(response.ResponseStream, response.Headers.ContentType, key);
        });

        group.MapGet("/pre-signed-url", async (string bucketName, string key, int expiryMinutes, ObjectHandler handler) =>
        {
            var errors = ObjectValidator.Validate(bucketName, key);
            foreach (var expiryError in ObjectValidator.ValidateExpiry(expiryMinutes))
                errors[expiryError.Key] = expiryError.Value;
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            return Results.Ok(await handler.CreateDownloadUrlAsync(bucketName, key, expiryMinutes));
        });

        return endpoints;
    }
}
