using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace minas_valor_backend.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountBranch
{
    [Display(Name = "Poços de Caldas")]
    PocosDeCaldas,

    [Display(Name = "Andradas")]
    Andradas,

    [Display(Name = "Águas de Prata")]
    AguasDePrata,

    [Display(Name = "São João da Boa Vista")]
    SaoJoaoDaBoaVista,

    [Display(Name = "Bandeira do Sul")]
    BandeiraDoSul,

    [Display(Name = "São José do Rio Pardo")]
    SaoJoseDoRioPardo
}