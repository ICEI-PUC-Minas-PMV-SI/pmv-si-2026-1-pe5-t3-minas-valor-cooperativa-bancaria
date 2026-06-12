using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace minas_valor_backend.Models;

[Table(("BankAccounts"))]
public class BankAccount
{
    [Key]
    public int Id { get; set; }
    
    [Display(Name ="Número da conta")]
    public required string AccountIdentifier { get; set; }
    
    [Display(Name ="Banco de origem")]
    public required AccountBranch AccountBranch {  get; set; }
    
    [Display(Name ="Saldo")]
    public required decimal Balance { get; set; }
    
    [Display(Name ="Criada em")]
    public required DateTime CreatedAt { get; set; }
    
    [Display(Name ="Última atualização")]
    public DateTime? UpdatedAt { get; set; }
    
    [Display(Name ="Status")]
    public DateTime? ClosedAt { get; set; }
    
    public ICollection<Transaction> TransactionsFrom { get; set; }
    
    public ICollection<Transaction> TransactionsTo { get; set; }
}