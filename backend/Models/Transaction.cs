using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace minas_valor_backend.Models;

[Table("Transactions")]
public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public TransactionOperation Operation { get; set; }

    [Required]
    public decimal Value { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 🔗 Foreign Keys
    [Required]
    public int FromBankAccountId { get; set; }
    public BankAccount FromBankAccount { get; set; }
    
    [Required]
    public int ToBankAccountId { get; set; }
    public BankAccount ToBankAccount { get; set; }
    
}