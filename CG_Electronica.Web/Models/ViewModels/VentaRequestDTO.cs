using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models.ViewModels
{
    public class VentaRequestDTO
    {
        [Required(ErrorMessage = "El número de factura es obligatorio.")]
        public string NumeroFactura { get; set; } = null!;
        
        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        public int IdCliente { get; set; }

        [Required]
        public List<DetalleVentaDTO> Detalles { get; set; } = new();
    }

    public class DetalleVentaDTO
    {
        [Required]
        public int IdItem { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }
    }
}