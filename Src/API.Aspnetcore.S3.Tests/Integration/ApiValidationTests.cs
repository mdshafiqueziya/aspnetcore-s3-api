using Amazon.S3;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace API.Aspnetcore.S3.Tests.Integration;

public class ApiValidationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiValidationTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task CreateBucket_WithMissingName_ReturnsValidationProblem()
    {
        using var response = await _client.PostAsJsonAsync("/api/buckets", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GenerateUrl_WithInvalidExpiry_ReturnsValidationProblem()
    {
        using var response = await _client.GetAsync(
            "/api/objects/pre-signed-url?bucketName=demo-bucket&key=object.txt&expiryMinutes=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAmazonS3>();
            services.AddSingleton(new Mock<IAmazonS3>().Object);
        });
    }
}
