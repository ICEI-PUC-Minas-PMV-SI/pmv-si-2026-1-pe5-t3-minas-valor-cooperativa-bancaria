using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace minas_valor_backend.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionOperation
{
    [Display(Name = "Depósito")]
    Deposit,
    [Display(Name = "Saque")]
    Withdraw,
    [Display(Name = "Transferência")]
    WireTransfer
}