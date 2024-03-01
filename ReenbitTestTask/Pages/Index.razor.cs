using ReenbitTestTask.Data;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace ReenbitTestTask.Pages
{
    partial class Index
    {
        [Inject]
        public DocumentsContext Context { get; set; } = default!;

        [EmailAddress]
        public string Email { get; set; } = default!;

        private bool _disableButton => !IsValidEmail(Email);

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task UploadFiles(IBrowserFile file)
        {
            if (file!.ContentType != "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                return;
            }

            using var ms = new MemoryStream();
            await file.OpenReadStream().CopyToAsync(ms);
            byte[] byteArray = ms.ToArray();

            await Context.CreateDocument(new Models.Document
            {
                Name = file.Name,
                Content = byteArray
            },
            Email);

            Email = default!;
        }
    }
}
 