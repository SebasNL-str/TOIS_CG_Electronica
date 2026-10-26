namespace CG_Electronica.Web.Models;

public class Factura
{
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal Iva { get; set; }
    public string Estado { get; set; } = "Emitida"; // Emitida, Anulada
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public DateTime? FechaAnulacion { get; set; }

    // Datos históricos del cliente
    public string? NombreClienteHistorico { get; set; }
    public string? TelefonoClienteHistorico { get; set; }
    public string? CedulaClienteHistorica { get; set; }

    public int IdUsuario { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;

    public int IdCliente { get; set; }
    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
}