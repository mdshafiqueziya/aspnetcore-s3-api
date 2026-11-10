using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.S3.Model;
using API.Aspnetcore.S3.Features.Buckets;
using API.Aspnetcore.S3.Features.Objects;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

AWSOptions aWSOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(aWSOptions);
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IBucketService, BucketService>();
builder.Services.AddScoped<IObjectService, ObjectService>();
builder.Services.AddScoped<BucketHandler>();
builder.Services.AddScoped<ObjectHandler>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var statusCode = exception switch
    {
        ArgumentException => StatusCodes.Status400BadRequest,
        AmazonS3Exception s3Exception when s3Exception.StatusCode == HttpStatusCode.NotFound
            => StatusCodes.Status404NotFound,
        AmazonS3Exception s3Exception when s3Exception.StatusCode == HttpStatusCode.Forbidden
            => StatusCodes.Status403Forbidden,
        AmazonS3Exception s3Exception when s3Exception.StatusCode == HttpStatusCode.Conflict
            => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    context.Response.StatusCode = statusCode;
    await Results.Problem(
        statusCode: statusCode,
        title: statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : "The request could not be completed.").ExecuteAsync(context);
}));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapBucketEndpoints();
app.MapObjectEndpoints();

app.Run();
