using Microsoft.EntityFrameworkCore;

namespace minas_valor_backend.Models;

public class MinasValorContext : DbContext
{
    public MinasValorContext(DbContextOptions<MinasValorContext> options) : base(options)
    {
    }

    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasIndex(b => b.AccountIdentifier).IsUnique();

            entity.Property(b => b.AccountBranch)
                .HasConversion<string>();

            entity.Property(b => b.Balance)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(t => t.Value)
                .HasPrecision(18, 2);

            entity.Property(t => t.Operation)
                .HasConversion<string>();
            
            // 🔗 FROM
            entity.HasOne(t => t.FromBankAccount)
                .WithMany(b => b.TransactionsFrom)
                .HasForeignKey(t => t.FromBankAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔗 TO
            entity.HasOne(t => t.ToBankAccount)
                .WithMany(b => b.TransactionsTo)
                .HasForeignKey(t => t.ToBankAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasCheckConstraint(
                "CK_Transaction_Operation_Consistency",
                @"(
        (""Operation"" IN ('Withdraw', 'Deposit') AND ""FromBankAccountId"" = ""ToBankAccountId"")
        OR
        (""Operation"" = 'WireTransfer' AND ""FromBankAccountId"" <> ""ToBankAccountId"")
    )"
            );
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role)
                .HasConversion<string>();
        });
    }
}