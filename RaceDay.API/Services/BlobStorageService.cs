using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString = configuration["AzureBlobStorage:ConnectionString"]!;
            string containerName = configuration["AzureBlobStorage:ContainerName"]!;

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        // Creates the container if it does not already exist
        public async Task CreateContainerIfNotExistsAsync()
        {
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        }
    }
}