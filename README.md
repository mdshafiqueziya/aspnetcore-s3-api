# ASP.NET Core S3 API

REST API built with ASP.NET Core for common Amazon S3 operations such as bucket management, folder placeholder creation, object upload, object listing, object download, and pre-signed download URL generation.

## Tech Stack

- ASP.NET Core Web API
- .NET 10
- AWS SDK for .NET
- Swagger / OpenAPI

## Architecture

This application uses a pragmatic vertical slice architecture. Code is grouped by business capability rather than by technical type: the Buckets slice owns bucket endpoints, requests, and S3 operations, while the Objects slice owns object endpoints, responses, validation, and S3 operations.

The request flow is:

```text
HTTP Request
    ↓
Buckets or Objects slice
    ↓
AWS SDK IAmazonS3 Client
    ↓
Amazon S3
```

### Layers

| Layer | Location | Responsibility |
| --- | --- | --- |
| Buckets slice | `Features/Buckets` | Owns bucket and folder endpoints, request models, and bucket S3 operations. |
| Objects slice | `Features/Objects` | Owns upload, listing, download, and pre-signed URL endpoints, response models, and object S3 operations. |
| Infrastructure / External Service Layer | AWS SDK `IAmazonS3` | Provides access to Amazon S3. The client is registered in `Program.cs` using `AddAWSService<IAmazonS3>()`. |
| Composition Root | `Program.cs` | Configures Minimal APIs, Swagger/OpenAPI, AWS options, dependency injection, middleware, and endpoint mapping. It does not contain feature behavior. |

### Dependency Injection

Services are registered in `Program.cs`:

```csharp
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IBucketService, BucketService>();
builder.Services.AddScoped<IObjectService, ObjectService>();
```

Each slice keeps its endpoints, validators, handlers, and S3 behavior close together:

- `Features/Buckets/BucketEndpoints.cs` maps the bucket routes.
- `Features/Buckets/BucketValidator.cs` validates bucket requests.
- `Features/Buckets/BucketHandler.cs` coordinates bucket operations.
- `Features/Buckets/BucketService.cs` depends on `IAmazonS3`.
- `Features/Objects/ObjectEndpoints.cs` maps the object routes.
- `Features/Objects/ObjectValidator.cs` validates object requests.
- `Features/Objects/ObjectHandler.cs` coordinates object operations.
- `Features/Objects/ObjectService.cs` depends on `IAmazonS3`.

### Architectural Style

The project is a monolithic REST API organized with pragmatic vertical slices. It is not using Clean Architecture, CQRS, Repository Pattern, MediatR, domain-driven design, or a microservice split. Each slice is intentionally lightweight and can evolve independently inside the single ASP.NET Core project.

## Project Structure

```text
.
├── README.md
├── LICENSE
└── Src
    ├── API.Aspnetcore.S3.slnx
    └── API.Aspnetcore.S3
        ├── Features
        │   ├── Buckets
        │   │   ├── BucketEndpoints.cs
        │   │   ├── BucketHandler.cs
        │   │   ├── BucketService.cs
        │   │   ├── BucketValidator.cs
        │   │   └── Models/BucketRequests.cs
        │   └── Objects
        │       ├── ObjectEndpoints.cs
        │       ├── ObjectHandler.cs
        │       ├── ObjectService.cs
        │       ├── ObjectValidator.cs
        │       └── Models/ObjectResponse.cs
        ├── Program.cs
        ├── appsettings.json
        └── API.Aspnetcore.S3.csproj
```

## Features

- Create an S3 bucket.
- List all accessible S3 buckets.
- Delete an S3 bucket.
- Get a bucket location.
- Create an S3 folder placeholder by creating an empty object with a trailing slash key.
- Upload one object.
- Upload multiple objects.
- List object keys in a bucket.
- Download an object.
- Generate a pre-signed download URL.

`ObjectService` also contains methods for deleting objects and generating pre-signed upload URLs, but those methods are not currently exposed through the Minimal API endpoints.

## Prerequisites

- .NET 10 SDK
- AWS account with S3 access
- AWS credentials configured for the AWS SDK default credential chain

The application registers `IAmazonS3` through `AddAWSService<IAmazonS3>()`, so credentials and region are resolved by the standard AWS SDK configuration providers, including environment variables, the shared AWS credentials/config files, and IAM roles when running on AWS infrastructure.

## AWS Configuration

For local development, the simplest setup is to configure the AWS CLI:

```bash
aws configure
```

Provide:

- AWS Access Key ID
- AWS Secret Access Key
- Default AWS Region
- Default output format, such as `json`

You can also use a named profile:

```bash
aws configure --profile myprofile
export AWS_PROFILE=myprofile
```

The AWS identity used by the API needs permissions for the S3 operations you call. Common permissions include:

- `s3:ListAllMyBuckets`
- `s3:CreateBucket`
- `s3:DeleteBucket`
- `s3:GetBucketLocation`
- `s3:ListBucket`
- `s3:PutObject`
- `s3:GetObject`
- `s3:DeleteObject`

For production, prefer least-privilege IAM policies and avoid storing long-lived credentials in application configuration.

## Configuration

The checked-in `appsettings.json` only configures logging and allowed hosts:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

If you want to configure AWS options explicitly through configuration, the AWS SDK for .NET can read an `AWS` section, for example:

```json
{
  "AWS": {
    "Profile": "myprofile",
    "Region": "us-east-1"
  }
}
```

Do not commit access keys or secret keys to this file.

## Run Locally

Restore and build:

```bash
dotnet restore Src/API.Aspnetcore.S3.slnx
dotnet build Src/API.Aspnetcore.S3.slnx
```

Run the API:

```bash
dotnet run --project Src/API.Aspnetcore.S3/API.Aspnetcore.S3.csproj
```

The launch profile configures these local URLs:

- HTTP: `http://localhost:5260`
- HTTPS: `https://localhost:7093`

Swagger UI is enabled in the Development environment:

```text
https://localhost:7093/swagger
```

## API Endpoints

### Buckets

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/buckets` | Create a bucket. |
| `POST` | `/api/buckets/folder` | Create a folder placeholder in a bucket. |
| `GET` | `/api/buckets` | List accessible buckets. |
| `GET` | `/api/buckets/{bucketName}/location` | Get a bucket location. |
| `DELETE` | `/api/buckets/{bucketName}` | Delete a bucket. |

### Objects

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/objects/upload/{bucketName}` | Upload one file as multipart form data. |
| `POST` | `/api/objects/uploads/{bucketName}` | Upload multiple files as multipart form data. |
| `GET` | `/api/objects?bucketName={bucketName}` | List object keys in a bucket. |
| `GET` | `/api/objects/download?bucket={bucketName}&key={objectKey}` | Download an object. |
| `GET` | `/api/objects/pre-signed-url?bucketName={bucketName}&key={objectKey}&expiryMinutes={minutes}` | Generate a pre-signed download URL. |

## Request Examples

Create a bucket:

```bash
curl -X POST http://localhost:5260/api/buckets \
  -H "Content-Type: application/json" \
  -d '{"bucketName":"my-s3-bucket"}'
```

Create a folder placeholder:

```bash
curl -X POST http://localhost:5260/api/buckets/folder \
  -H "Content-Type: application/json" \
  -d '{"bucketName":"my-s3-bucket","folderName":"documents"}'
```

List buckets:

```bash
curl http://localhost:5260/api/buckets
```

Upload a file:

```bash
curl -X POST http://localhost:5260/api/objects/upload/my-s3-bucket \
  -F "file=@./example.txt"
```

Upload multiple files:

```bash
curl -X POST http://localhost:5260/api/objects/uploads/my-s3-bucket \
  -F "files=@./example-1.txt" \
  -F "files=@./example-2.txt"
```

List objects:

```bash
curl "http://localhost:5260/api/objects?bucketName=my-s3-bucket"
```

Download an object using the key returned by the upload endpoint:

```bash
curl -L "http://localhost:5260/api/objects/download?bucket=my-s3-bucket&key=uploads%2F<upload-id>%2Fexample.txt" \
  -o example.txt
```

Generate a pre-signed download URL using the returned object key:

```bash
curl "http://localhost:5260/api/objects/pre-signed-url?bucketName=my-s3-bucket&key=uploads%2F<upload-id>%2Fexample.txt&expiryMinutes=15"
```

Delete a bucket:

```bash
curl -X DELETE http://localhost:5260/api/buckets/my-s3-bucket
```

## Development Notes

- Swagger and OpenAPI are only mapped when `ASPNETCORE_ENVIRONMENT` is `Development`.
- Bucket deletion requires the bucket to be empty.
- Uploaded object keys use a generated unique prefix and a sanitized version of the incoming file name, preventing accidental overwrites.
- Uploads are limited to 50 MB per file and 20 files per multi-upload request.
- Pre-signed URL expiry is limited to 1–60 minutes.
- There is no authentication or authorization policy configured in this API yet, even though `UseAuthorization()` is present in the pipeline.
- Validation and AWS errors are returned as standard problem-details responses with appropriate HTTP status codes.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
