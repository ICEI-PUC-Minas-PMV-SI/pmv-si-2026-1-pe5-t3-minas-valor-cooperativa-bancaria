using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using minas_valor_backend.Models;

namespace minas_valor_backend.Controllers;

[Authorize]
public class BankAccountsController : Controller
{
    private readonly MinasValorContext _context;

    public BankAccountsController(MinasValorContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get bank accounts (all for staff; only own for Cooperados)
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        var accounts = _context.BankAccounts
            .Include(b => b.Owner)
            .AsQueryable();

        // Cooperados only see their own accounts; staff (Admin) see all.
        if (!User.IsInRole("Admin"))
        {
            var userId = CurrentUserId();
            accounts = accounts.Where(b => b.OwnerId == userId);
        }

        return View(await accounts.ToListAsync());
    }

    /// <summary>
    /// Create bank account
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await PopulateOwnersAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(BankAccountCreateViewModel bankAccountCreateDto)
    {
        var bankAccount = new BankAccount
        {
            Balance = 0,
            CreatedAt = DateTime.UtcNow,
            AccountIdentifier = bankAccountCreateDto.AccountIdentifier,
            AccountBranch = bankAccountCreateDto.AccountBranch,
            OwnerId = bankAccountCreateDto.OwnerId
        };
        if (ModelState.IsValid)
        {
            _context.Add(bankAccount);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        await PopulateOwnersAsync();
        return View(bankAccountCreateDto);
    }

    /// <summary>
    /// Edit bank account
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var bankAccount = await _context.BankAccounts.FindAsync(id);
        if (bankAccount == null)
        {
            return NotFound();
        }

        return  View(bankAccount);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BankAccount bankAccount)
    {
        if (id != bankAccount.Id)
        {
            return NotFound();
        }

        var existing = await _context.BankAccounts.FindAsync(id);

        if (existing == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            existing.AccountIdentifier = bankAccount.AccountIdentifier;
            existing.AccountBranch = bankAccount.AccountBranch;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        return View(bankAccount);
    }

    /// <summary>
    /// Get one bank account
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return new NotFoundResult();
        }

        var bankAccount = await _context.BankAccounts
            .Include(b => b.Owner)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (bankAccount == null)
        {
            return NotFound();
        }

        // Cooperados may only view their own accounts.
        if (!User.IsInRole("Admin") && bankAccount.OwnerId != CurrentUserId())
        {
            return Forbid();
        }

        var transactions = await _context.Transactions
            .Where(t => t.FromBankAccountId == id || t.ToBankAccountId == id)
            .Include(t => t.FromBankAccount)
            .Include(t => t.ToBankAccount)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        ViewBag.Transactions = transactions;

        return  View(bankAccount);
    }

    /// <summary>
    /// Close a bank account
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return new NotFoundResult();
        }

        var bankAccount = await _context.BankAccounts.FindAsync(id);
        if (bankAccount == null)
        {
            return NotFound();
        }

        return  View(bankAccount);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var bankAccount = await _context.BankAccounts.FindAsync(id);
        if (bankAccount == null)
        {
            return NotFound();
        }

        bankAccount.ClosedAt = DateTime.UtcNow;
        bankAccount.Balance = 0;
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    private int? CurrentUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private async Task PopulateOwnersAsync()
    {
        var cooperados = await _context.Users
            .Where(u => u.Role == UserRole.User)
            .OrderBy(u => u.Name)
            .ToListAsync();

        ViewBag.Owners = new SelectList(cooperados, "Id", "Name");
    }
}
