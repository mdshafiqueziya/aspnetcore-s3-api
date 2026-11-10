namespace API.Aspnetcore.S3.Features.Buckets;

public static class BucketEndpoints
{
    public static IEndpointRouteBuilder MapBucketEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/buckets").WithTags("Buckets");

        group.MapPost("/", async (CreateBucketRequest request, BucketHandler handler, HttpContext context) =>
        {
            var errors = BucketValidator.Validate(request);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            await handler.CreateAsync(request.BucketName, context.RequestAborted);
            return Results.Created("/api/buckets", new { message = "Bucket created successfully." });
        });

        group.MapPost("/folder", async (CreateFolderRequest request, BucketHandler handler, HttpContext context) =>
        {
            var errors = BucketValidator.Validate(request);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            await handler.CreateFolderAsync(request.BucketName, request.FolderName, context.RequestAborted);
            return Results.Created("/api/buckets/folder", new { message = "Folder created successfully." });
        });

        group.MapGet("/", (BucketHandler handler) => handler.ListAsync());
        group.MapGet("/{bucketName}/location", async (string bucketName, BucketHandler handler, HttpContext context) =>
            Results.Ok(await handler.GetLocationAsync(bucketName, context.RequestAborted)));
        group.MapDelete("/{bucketName}", async (string bucketName, BucketHandler handler, HttpContext context) =>
        {
            await handler.DeleteAsync(bucketName, context.RequestAborted);
            return Results.NoContent();
        });

        return endpoints;
    }
}
