using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace minas_valor_backend.Models;

[Table("Users")]
public class User
{
    [Key]
    public required int Id { get; set; }
    
    [Required(ErrorMessage = "Nome é obrigatório")]
    [Display(Name = "Nome")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Email é obrigatório")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Senha é obrigatória")]
    [Display(Name = "Senha")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Tipo de usuário é obrigatório")]
    [Display(Name = "Tipo de usuário")]
    public UserRole Role { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
}