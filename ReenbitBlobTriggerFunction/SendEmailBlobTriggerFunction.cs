using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ReenbitBlobTriggerFunction
{
    public class SendEmailBlobTriggerFunction
    {
        [FunctionName("SendEmailBlobTriggerFunction")]
        public void Run([BlobTrigger("reenbitblobcontainer/{name}", Connection = "ReenbitStorage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
