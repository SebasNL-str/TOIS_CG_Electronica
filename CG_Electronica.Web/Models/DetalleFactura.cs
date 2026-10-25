using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CG_Electronica.Web.Models
{
    public class DetalleFactura
    {
        [Key]
        public int IdDetalle { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal PrecioUnitarioHistorico { get; set; } // Nunca usar precioVenta directo para consultas históricas[cite: 6]

        [Required]
        public int IdFactura { get; set; }
        [ForeignKey("IdFactura")]
        public Factura Factura { get; set; } = null!;

        [Required]
        public int IdItem { get; set; }
        [ForeignKey("IdItem")]
        public ItemFacturable Item { get; set; } = null!; // Creado por Integrante 2[cite: 6]
    }
}