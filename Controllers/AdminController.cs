using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Practicum.Data;

namespace Practicum.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index() => View(await _db.Users.ToListAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var u = await _db.Users.FindAsync(id);
            if (u != null && new[] { "Farmer", "Agronomist", "Admin" }.Contains(role))
            {
                u.Role = role;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
