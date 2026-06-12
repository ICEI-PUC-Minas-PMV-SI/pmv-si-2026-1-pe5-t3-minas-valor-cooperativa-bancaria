using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace minas_valor_backend.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    [Display(Name = "Colaborador")]
    Admin,
    [Display(Name = "Cooperado")]
    User
}