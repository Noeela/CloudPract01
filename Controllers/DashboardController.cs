using Azure.Data.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practicum.Data;
using Practicum.Models;

namespace Practicum.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _http;
        private readonly TableServiceClient _tables;

        public DashboardController(AppDbContext db, IHttpClientFactory http, TableServiceClient tables)
        { _db = db; _http = http; _tables = tables; }

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel { Devices = await _db.Devices.ToListAsync() };
            try
            {
                // Calls HTTP GET Azure Function
                var client = _http.CreateClient("functions");
                vm.Telemetry = await client.GetFromJsonAsync<List<TelemetryDto>>("api/GetTelemetry") ?? new();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Could not load telemetry: " + ex.Message;
            }
            return View(vm);
        }

        // Calls HTTP POST Azure Function -> puts message on Storage Queue
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TriggerIrrigation(string deviceId)
        {
            var client = _http.CreateClient("functions");
            var resp = await client.PostAsJsonAsync("api/TriggerIrrigation",
                new { deviceId, reason = "Manual", requestedBy = User.Identity!.Name });

            TempData[resp.IsSuccessStatusCode ? "Success" : "Error"] =
                resp.IsSuccessStatusCode ? $"Irrigation command queued for {deviceId}" : "Failed to queue command";
            return RedirectToAction(nameof(Index));
        }

        // Adds sample sensor readings to Azure Table Storage
        [Authorize(Roles = "Admin,Agronomist")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedTelemetry()
        {
            var table = _tables.GetTableClient("Telemetry");
            await table.CreateIfNotExistsAsync();
            var rnd = new Random();

            foreach (var d in await _db.Devices.ToListAsync())
                for (int i = 0; i < 10; i++)
                {
                    var time = DateTime.UtcNow.AddMinutes(-i * 5);
                    var rowKey = (DateTime.MaxValue.Ticks - time.Ticks).ToString("D19");  // newest first
                    await table.UpsertEntityAsync(new TableEntity(d.DeviceId, rowKey)
                {
                    { "SoilMoisture", Math.Round(rnd.NextDouble() * 60 + 20, 1) },
                    { "Temperature", Math.Round(rnd.NextDouble() * 15 + 15, 1) },
                    { "ReadingTime", time }
                });
                }

            TempData["Success"] = "Sample telemetry added";
            return RedirectToAction(nameof(Index));
        }

    }
}
