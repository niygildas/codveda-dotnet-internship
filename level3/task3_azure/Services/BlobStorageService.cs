using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureIntegration.Services;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(string containerName, string blobName, Stream content, string contentType);
    Task<Stream> DownloadFileAsync(string containerName, string blobName);
    Task DeleteFileAsync(string containerName, string blobName);
    Task<IEnumerable<string>> ListFilesAsync(string containerName);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(IConfiguration config, ILogger<BlobStorageService> logger)
    {
        var connectionString = config["Azure:BlobStorage:ConnectionString"]
            ?? throw new InvalidOperationException("Azure Blob Storage connection string not configured.");
        _blobServiceClient = new BlobServiceClient(connectionString);
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(string containerName, string blobName, Stream content, string contentType)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None);
        var blobClient = container.GetBlobClient(blobName);
        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });
        _logger.LogInformation("Uploaded {BlobName} to {Container}", blobName, containerName);
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadFileAsync(string containerName, string blobName)
    {
        var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        var download = await blobClient.DownloadAsync();
        return download.Value.Content;
    }

    public async Task DeleteFileAsync(string containerName, string blobName)
    {
        var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
        _logger.LogInformation("Deleted {BlobName} from {Container}", blobName, containerName);
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string containerName)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var names = new List<string>();
        await foreach (var blob in container.GetBlobsAsync())
            names.Add(blob.Name);
        return names;
    }
}
