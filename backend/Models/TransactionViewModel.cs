namespace minas_valor_backend.Models;

public class TransactionViewModel
{
    public string FromAccountIdentifier { get; set; }
    public int FromAccountId { get; set; }
    public string? ToAccountIdentifier { get; set; }
    public TransactionOperation Operation { get; set; }
    public decimal Value { get; set; }
    public List<BankAccount>? AvailableAccounts { get; set; }
}