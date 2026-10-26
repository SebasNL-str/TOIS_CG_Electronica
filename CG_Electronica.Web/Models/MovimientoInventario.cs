namespace CG_Electronica.Web.Models;

public class MovimientoInventario
{
    public int IdMovimiento { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty; // Entrada, Salida
    public string Motivo { get; set; } = string.Empty; // Enum/Check en SQL
    public int Cantidad { get; set; }
    public string? Observacion { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public decimal PrecioCompraUnitario { get; set; }

    public int IdUsuario { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;

    public int IdProducto { get; set; }
    public virtual Producto Producto { get; set; } = null!;
}