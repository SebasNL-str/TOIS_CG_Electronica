using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models;

public class Cliente
{
    [Key]
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(150)]
    public string NombreCliente { get; set; } = string.Empty;

    [StringLength(30)]
    [Phone(ErrorMessage = "El teléfono no tiene un formato válido.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [StringLength(50)]
    public string Cedula { get; set; } = string.Empty;
    public string Estado { get; set; } = "Activo";
}