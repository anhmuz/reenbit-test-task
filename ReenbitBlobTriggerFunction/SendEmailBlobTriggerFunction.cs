using System;
using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;

namespace ReenbitBlobTriggerFunction
{
    public class SendEmailBlobTriggerFunction
    {
        [FunctionName("SendEmailBlobTriggerFunction")]
        public static async Task Run([BlobTrigger("reenbitblobcontainer/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob,
            string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var apiKey = Environment.GetEnvironmentVariable("SendGridApiKey");

            try
            {
                var url = generateFileUrlWithBlobSasToken(storageConnectionString, "reenbitblobcontainer", name);

                log.LogInformation($"SAS blob Uri: {url}");

                var client = new SendGridClient(apiKey);
                var message = new SendGridMessage()
                {
                    From = new EmailAddress("anhelina.muzychenko@gmail.com", "Anhelina Muzychenko"),
                    Subject = "File Uploaded Successfully",
                    PlainTextContent = $"<p>The file has been uploaded successfully. Here is the link: <a href='{url}'>{url}</a></p>"
                };

                var email = await getEmailFromBlobMetadata(storageConnectionString, "reenbitblobcontainer", name);
                message.AddTo(new EmailAddress(email));

                var response = await client.SendEmailAsync(message);

                log.LogInformation(response.IsSuccessStatusCode ? "Email sent successfully!" : "Something went wrong!");
            } 
            catch (Exception ex)
            {
                log.LogError($"Error occured while processing Blob {name}, Exception - {ex.InnerException}");
            }
        }

        private static async Task<string> getEmailFromBlobMetadata(string storageConnectionString, string container, string blobName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(blobName);
            await blob.FetchAttributesAsync();
            return blob.Metadata["email"];
        }

        private static string generateFileUrlWithBlobSasToken(string storageConnectionString, string container, string blobName)
        {
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(container);
            var blobClient = containerClient.GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = container,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            
            var accountKey = parseConnectionString(storageConnectionString)["AccountKey"];
            var sharedKeyCredential = new StorageSharedKeyCredential(blobServiceClient.AccountName, accountKey);
            var sasToken = sasBuilder.ToSasQueryParameters(sharedKeyCredential);

            var sasBlobUri = new UriBuilder(blobClient.Uri)
            {
                Query = sasToken.ToString()
            };

            return sasBlobUri.ToString();
        }

        private static Dictionary<string, string> parseConnectionString(string stringData)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            Array.ForEach(stringData.Split(';'), s =>
            {
                if (s != null && s.Length != 0)
                {
                    int pos = s.IndexOf('=');
                    keyValuePairs.Add(s[..pos], s[(pos + 1)..]);
                }
            });

            return keyValuePairs;
        }
    }
}
