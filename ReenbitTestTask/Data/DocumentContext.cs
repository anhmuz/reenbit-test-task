using Microsoft.Extensions.Options;
using ReenbitTestTask.Models;
using ReenbitTestTask.Helpers;

namespace ReenbitTestTask.Data;

public class DocumentsContext
{
    private readonly AzureStorageConfig _config;

    public DocumentsContext(IOptions<AzureStorageConfig> config)
    {
        _config = config.Value;
    }

    public List<Document>? Documents { get; set; }

    public async Task<List<Document>> GetDocuments()
    {
        List<DocumentBlobDTO> blobs = await StorageHelper.GetBlobs(_config.AccountName, _config.ContainerName);

        List<Document> Documents = new List<Document>();
        foreach (DocumentBlobDTO blob in blobs)
        {
            Document Document = new Document();
            Document.Name = blob.Name;
            Document.Content = blob.Content;

            Documents.Add(Document);
        }

        return Documents;
    }

    public async Task CreateDocument(Document Document)
    {
        await StorageHelper.UploadBlob(_config.AccountName, _config.ContainerName, Document.Name, Document.Content);
    }

    public async Task DeleteDocument(Document Document)
    {
        await StorageHelper.DeleteBlob(_config.AccountName, _config.ContainerName, Document.Name);
    }
}