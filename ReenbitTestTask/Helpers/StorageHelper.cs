using Azure.Identity;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using System.Text;

namespace ReenbitTestTask.Helpers;

public static class StorageHelper
{
    static public async Task UploadBlob(string accountName, string containerName, string blobName, byte[] blobContent, string email)
    {
        // Construct the blob container endpoint from the arguments.
        string containerEndpoint = string.Format("https://{0}.blob.core.windows.net/{1}",
            accountName, containerName);

        // Get a credential and create a client object for the blob container.
        BlobContainerClient containerClient = new BlobContainerClient(
            new Uri(containerEndpoint), new DefaultAzureCredential());

        //BlobContainerClient containerClient = new BlobContainerClient(new Uri(containerEndpoint),
        //                                                                           new InteractiveBrowserCredential());

        //BlobContainerClient containerClient = new BlobContainerClient(new Uri(containerEndpoint),
        //                                                                           new VisualStudioCredential());
        
        Dictionary<string, string> blobMetadata = new();

        try
        {
            // Create the container if it does not exist.
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);

            blobMetadata.Add("email", email);

            using (var stream = new MemoryStream(blobContent))
            {
                await blobClient.UploadAsync(stream, null, blobMetadata, null, null, null);
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    static public async Task DeleteBlob(string accountName, string containerName, string blobName)
    {
        // Construct the blob container endpoint from the arguments.
        string containerEndpoint = string.Format("https://{0}.blob.core.windows.net/{1}",
            accountName, containerName);

        // Get a credential and create a client object for the blob container.
        BlobContainerClient containerClient = new BlobContainerClient(
            new Uri(containerEndpoint), new DefaultAzureCredential());

        try
        {
            var blob = containerClient.GetBlobClient(blobName);
            await blob.DeleteIfExistsAsync();
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    static public async Task<List<DocumentBlobDTO>> GetBlobs(string accountName, string containerName)
    {
        List<DocumentBlobDTO> blobs = new List<DocumentBlobDTO>();

        // Construct the blob container endpoint from the arguments.
        string containerEndpoint = string.Format("https://{0}.blob.core.windows.net/{1}",
            accountName, containerName);

        // Get a credential and create a client object for the blob container.
        BlobContainerClient containerClient = new BlobContainerClient(
            new Uri(containerEndpoint), new DefaultAzureCredential());

        await containerClient.CreateIfNotExistsAsync();

        try
        {
            // List all the blobs                
            await foreach (BlobItem blob in containerClient.GetBlobsAsync())
            {
                // Download the blob's contents and save it to a file
                // Get a reference to a blob named "sample-file"
                BlobClient blobClient = containerClient.GetBlobClient(blob.Name);
                BlobDownloadInfo download = await blobClient.DownloadAsync();

                byte[] bytes;
                using (MemoryStream stream = new MemoryStream())
                {
                    await download.Content.CopyToAsync(stream);
                    bytes = stream.ToArray();
                }

                DocumentBlobDTO blobDTO;
                blobDTO.Name = blob.Name;
                blobDTO.Content = bytes;
                blobs.Add(blobDTO);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

        return blobs;
    }
}

public struct DocumentBlobDTO
{
    public string Name;
    public byte[] Content;
}
