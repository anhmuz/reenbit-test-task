using ReenbitTestTask.Data;
using Microsoft.AspNetCore.Components;

namespace ReenbitTestTask.Pages
{
    partial class Index
    {
        [Inject]
        public DocumentsContext Context { get; set; } = default!;

        public async Task Upload()
        {
            await Context.CreateDocument(new Models.Document
            {
                Name = "fooname1",
                Content = "fooContent"
            });
        }
    }
}
 