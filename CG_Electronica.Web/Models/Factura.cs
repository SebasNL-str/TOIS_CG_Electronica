/*using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CG_Electronica.Web.Models
{
    public class Factura
    {
        [Key]
        public int IdFactura { get; set; }

        [Required, MaxLength(50)]
        public string NumeroFactura { get; set; } = null!;

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Iva { get; set; } // Importe monetario histórico del IVA[cite: 6]

        [Required, MaxLength(30)]
        public string Estado { get; set; } = "Emitida"; // Valores permitidos: Emitida, Anulada[cite: 6]

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Subtotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Total { get; set; }

        public DateTime? FechaAnulacion { get; set; }

        [Required, MaxLength(150)]
        public string NombreClienteHistorico { get; set; } = null!;

        [MaxLength(30)]
        public string? TelefonoClienteHistorico { get; set; }

        [MaxLength(50)]
        public string? CedulaClienteHistorica { get; set; }

        [Required]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!; // Creado por Integrante 1[cite: 6]

        [Required]
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente Cliente { get; set; } = null!; // Creado por Integrante 1[cite: 6]

        public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    }
}
*/