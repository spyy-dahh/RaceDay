using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

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

        // Uploads an event image to Blob Storage
        public async Task<string> UploadImageAsync(IFormFile image)
        {
            // Check whether a file was selected
            if (image == null || image.Length == 0)
            {
                throw new ArgumentException("Please select an image.");
            }

            // Limit image size to 5 MB
            if (image.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("Image size cannot exceed 5 MB.");
            }

            // Only allow certain image formats
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

            string fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException("Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            // Generate a unique name for the image
            string blobName = $"{Guid.NewGuid()}{fileExtension}";

            // Get a reference to where the image will be stored
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            // Open and upload the image
            using (Stream imageStream = image.OpenReadStream())
            {
                await blobClient.UploadAsync(
                    imageStream,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = image.ContentType
                        }
                    });
            }

            // Return the image name so it can be saved in the database
            return blobName;
        }

        // Retrieves an event image from Blob Storage
        public async Task<BlobDownloadStreamingResult?> GetImageAsync(string imageName)
        {
            // Get a reference to the requested image
            BlobClient blobClient = _containerClient.GetBlobClient(imageName);

            // Check whether the image exists
            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            // Download the image
            BlobDownloadStreamingResult downloadResult = await blobClient.DownloadStreamingAsync();

            return downloadResult;
        }

        // Deletes an event image from Blob Storage
        public async Task DeleteImageAsync(string imageName)
        {
            // Get a reference to the image
            BlobClient blobClient = _containerClient.GetBlobClient(imageName);

            // Delete the image if it exists
            await blobClient.DeleteIfExistsAsync();
        }
    }
}