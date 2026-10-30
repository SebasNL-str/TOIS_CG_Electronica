using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models.ViewModels;

public class ServicioCreateViewModel
{
    [Required(ErrorMessage = "El nombre del servicio es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string NombreServicio { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio de venta es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a cero.")]
    public decimal PrecioVenta { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = "Activo";
}