using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minas_valor_backend.Models;

namespace minas_valor_backend.Controllers;

[Authorize]
public class UsersController : Controller
{
    private MinasValorContext _context;

    public UsersController(MinasValorContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Login
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(User userLogin)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userLogin.Email);
        if (user == null)
        {
            ViewBag.Message = "E-mail e/ou senha estão incorretos";
        }

        bool isValidPassword = BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password);

        if (isValidPassword)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var userIdentity = new ClaimsIdentity(claims, "login");
            ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

            var props = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.ToLocalTime().AddHours(8),
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(principal, props);

            return Redirect("/");
        }
        else
        {
            ViewBag.Message = "E-mail e/ou senha estão incorretos";
        }

        return View();
    }

    /// <summary>
    /// Logout
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        return RedirectToAction("Login", "Users");
    }

    /// <summary>
    /// Access Denied
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        return View(await _context.Users.ToListAsync());
    }


    /// <summary>
    /// Get one user
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    /// <summary>
    /// Create user
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(User user)
    {
        if (ModelState.IsValid)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.CreatedAt = DateTime.UtcNow;
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(user);
    }

    /// <summary>
    /// Edit an user
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.Id)
        {
            return NotFound();
        }

        var existing = await _context.Users.FindAsync(id);

        if (existing == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.Role = user.Role;
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existing.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        return View(user);
    }

    /// <summary>
    /// Delete an user
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}