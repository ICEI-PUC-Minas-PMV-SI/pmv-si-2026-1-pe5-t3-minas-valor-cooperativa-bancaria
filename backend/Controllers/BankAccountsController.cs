using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    /// Get all bank accounts
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        return View(await _context.BankAccounts.ToListAsync());
    }

    /// <summary>
    /// Create bank account
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
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
            AccountBranch = bankAccountCreateDto.AccountBranch
        };
        if (ModelState.IsValid)
        {
            _context.Add(bankAccount);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        
        return View(bankAccount);
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
        
        var bankAccount = await _context.BankAccounts.FindAsync(id);
        if (bankAccount == null)
        {
            return NotFound();
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
    
}