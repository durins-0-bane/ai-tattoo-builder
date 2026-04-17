using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace TattooShop.Api.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(byte[] imageBytes, string contentType = "image/png");
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"]
            ?? throw new InvalidOperationException("AzureBlobStorage:ConnectionString is not configured.");
        var containerName = configuration["AzureBlobStorage:ContainerName"] ?? "tattoo-images";

        _containerClient = new BlobContainerClient(connectionString, containerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadImageAsync(byte[] imageBytes, string contentType = "image/png")
    {
        var blobName = $"{Guid.NewGuid()}.png";
        var blobClient = _containerClient.GetBlobClient(blobName);

        using var stream = new MemoryStream(imageBytes);
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });

        return blobClient.Uri.AbsoluteUri;
    }
}
