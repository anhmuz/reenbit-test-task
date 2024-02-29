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
            string email = "anhelina.muzychenko@gmail.com";
            await Context.CreateDocument(new Models.Document
            {
                Name = "fooname3",
                Content = "fooContent3"
            },
            email);
        }
    }
}
 