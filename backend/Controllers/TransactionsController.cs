using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minas_valor_backend.Models;

namespace minas_valor_backend.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private readonly MinasValorContext _context;

    public TransactionsController(MinasValorContext context)
    {
        _context = context;
    }

    // =========================
    // WITHDRAW
    // =========================
    public async Task<IActionResult> Withdraw(int? accountId)
    {
        if (accountId == null)
            return NotFound();

        var account = await _context.BankAccounts.FindAsync(accountId.Value);
        if (account == null || account.ClosedAt != null)
        {
            return NotFound();
        }

        var vm = new TransactionViewModel
        {
            FromAccountId = account.Id,
            FromAccountIdentifier = account.AccountIdentifier,
            Operation = TransactionOperation.Withdraw
        };

        return View("Create", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(TransactionViewModel vm)
    {
        var account = await _context.BankAccounts.FindAsync(vm.FromAccountId);
        if (account == null || account.ClosedAt != null)
        {
            return NotFound();
        }

        if (vm.Value <= 0)
        {
            ModelState.AddModelError("", "Valor inválido.");
            return View("Create", vm);
        }

        if (vm.Value > account.Balance)
        {
            ModelState.AddModelError("", "Saldo insuficiente.");
            return View("Create", vm);
        }

        account.Balance -= vm.Value;
        account.UpdatedAt = DateTime.UtcNow;

        var transaction = new Transaction
        {
            Operation = TransactionOperation.Withdraw,
            Value = vm.Value,
            FromBankAccountId = account.Id,
            ToBankAccountId = account.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "BankAccounts", new { id = account.Id });
    }

    // =========================
    // DEPOSIT
    // =========================
    public async Task<IActionResult> Deposit(int? accountId)
    {
        if (accountId == null)
        {
            return NotFound();
        }

        var account = await _context.BankAccounts.FindAsync(accountId.Value);
        if (account == null || account.ClosedAt != null)
        {
            return NotFound();
        }

        var vm = new TransactionViewModel
        {
            FromAccountId = account.Id,
            FromAccountIdentifier = account.AccountIdentifier,
            Operation = TransactionOperation.Deposit
        };

        return View("Create", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit(TransactionViewModel vm)
    {
        var account = await _context.BankAccounts.FindAsync(vm.FromAccountId);
        if (account == null || account.ClosedAt != null)
        {
            return NotFound();
        }

        if (vm.Value <= 0)
        {
            ModelState.AddModelError("", "Valor inválido.");
            return View("Create", vm);
        }

        account.Balance += vm.Value;
        account.UpdatedAt = DateTime.UtcNow;

        var transaction = new Transaction
        {
            Operation = TransactionOperation.Deposit,
            Value = vm.Value,
            FromBankAccountId = account.Id,
            ToBankAccountId = account.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "BankAccounts", new { id = account.Id });
    }

    // =========================
    // TRANSFER
    // =========================
    public async Task<IActionResult> Transfer(int? fromAccountId)
    {
        if (fromAccountId == null)
        {
            return NotFound();
        }

        var fromAccount = await _context.BankAccounts.FindAsync(fromAccountId.Value);
        if (fromAccount == null || fromAccount.ClosedAt != null)
        {
            return NotFound();
        }

        var accounts = await _context.BankAccounts
            .Where(a => a.Id != fromAccountId.Value && a.ClosedAt == null)
            .ToListAsync();

        var vm = new TransactionViewModel
        {
            FromAccountId = fromAccount.Id,
            FromAccountIdentifier = fromAccount.AccountIdentifier,
            Operation = TransactionOperation.WireTransfer,
            AvailableAccounts = accounts
        };

        return View("Create", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(TransactionViewModel vm)
    {
        var from = await _context.BankAccounts.FindAsync(vm.FromAccountId);
        var to = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.AccountIdentifier == vm.ToAccountIdentifier);

        if (from == null || to == null || from.ClosedAt != null || to.ClosedAt != null)
        {
            return NotFound();
        }

        if (vm.Value <= 0)
        {
            ModelState.AddModelError("", "Valor inválido.");
        }

        if (vm.ToAccountIdentifier == null)
        {
            ModelState.AddModelError("", "Selecione uma conta de destino.");
        }

        if (vm.Value > from.Balance)
        {
            ModelState.AddModelError("", "Saldo insuficiente.");
        }

        if (!ModelState.IsValid)
        {
            vm.AvailableAccounts = await _context.BankAccounts
                .Where(a => a.Id != vm.FromAccountId && a.ClosedAt == null)
                .ToListAsync();

            return View("Create", vm);
        }

        await using var dbTransaction = await _context.Database.BeginTransactionAsync();

        try
        {
            from.Balance -= vm.Value;
            from.UpdatedAt = DateTime.UtcNow;

            to.Balance += vm.Value;
            to.UpdatedAt = DateTime.UtcNow;

            var transaction = new Transaction
            {
                Operation = TransactionOperation.WireTransfer,
                Value = vm.Value,
                FromBankAccountId = from.Id,
                ToBankAccountId = to.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }

        return RedirectToAction("Details", "BankAccounts", new { id = from.Id });
    }
}