using System;
using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.Net.Mail;
using System.Net;


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
  
            try
            {
                var url = generateFileUrlWithBlobSasToken(storageConnectionString, "reenbitblobcontainer", name);
                log.LogInformation($"SAS blob Uri: {url}");

                string subject = "File Uploaded Successfully";
                string content = $"<p>The file has been uploaded successfully. Here is the <a href='{url}'>link</a></p>";
                string emailTo = await getEmailFromBlobMetadata(storageConnectionString, "reenbitblobcontainer", name);

                try
                {
                    sendEmail(emailTo, subject, content);
                    log.LogInformation("Email sent successfully");
                }
                catch (Exception ex)
                {
                    log.LogError($"Email cannot be sent: {ex.Message}");
                }
            } 
            catch (Exception ex)
            {
                log.LogError($"Error occured while processing Blob {name}, Exception - {ex.InnerException}");
            }
        }

        private static void sendEmail(string emailTo, string subject, string content)
        {
            string emailLogin = Environment.GetEnvironmentVariable("GmailLogin");
            string password = Environment.GetEnvironmentVariable("GmailPassword");

            MailMessage mailMessage = new MailMessage(emailLogin, emailTo, subject, content)
            {
                IsBodyHtml = true
            };
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailLogin, password)
            };
            smtpClient.Send(mailMessage);
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
