using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Practicum.Data;
using Practicum.Models;
using System.Security.Claims;

namespace Practicum.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        public AccountController(AppDbContext db, IPasswordHasher<User> hasher) { _db = db; _hasher = hasher; }

        [HttpGet] public IActionResult Register() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ViewModel m)
        {
            if (!ModelState.IsValid) return View(m);
            if (m.Role == "Admin") m.Role = "Farmer";        // admins can't self-register
            if (await _db.Users.AnyAsync(u => u.Username == m.Username))
            {
                ModelState.AddModelError("", "Username already exists");
                return View(m);
            }
            var user = new User { Username = m.Username, Role = m.Role };
            user.PasswordHash = _hasher.HashPassword(user, m.Password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        [HttpGet] public IActionResult Login() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel m)
        {
            if (!ModelState.IsValid) return View(m);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == m.Username);
            if (user == null ||
                _hasher.VerifyHashedPassword(user, user.PasswordHash, m.Password) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(m);
            }

            var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(principal);
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult Denied() => View();
    }
}
