using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practicum.Models;

namespace Practicum.Controllers
{
    [Authorize]
    public class MediaController : Controller
    {
        private readonly BlobContainerClient _container;
        public MediaController(BlobServiceClient svc) => _container = svc.GetBlobContainerClient("drone-images");

        public async Task<IActionResult> Index()
        {
            await _container.CreateIfNotExistsAsync();
            var items = new List<BlobItemViewModel>();
            await foreach (var b in _container.GetBlobsAsync())
            {
                var client = _container.GetBlobClient(b.Name);
                items.Add(new BlobItemViewModel
                {
                    Name = b.Name,
                    Url = client.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(30)).ToString()
                });
            }
            return View(items);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please choose a file";
                return RedirectToAction(nameof(Index));
            }
            await _container.CreateIfNotExistsAsync();
            var name = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
            await using var stream = file.OpenReadStream();
            await _container.UploadBlobAsync(name, stream);

            TempData["Success"] = "Image uploaded. The Blob function will log it.";
            return RedirectToAction(nameof(Index));
        }

    }
}
