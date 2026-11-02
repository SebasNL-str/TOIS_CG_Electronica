namespace CG_Electronica.Web.Models.ViewModels;

public class ReporteVentasViewModel
{
    public DateTime Desde { get; set; } = DateTime.Today.AddDays(-30);
    public DateTime Hasta { get; set; } = DateTime.Today;
    public int CantidadFacturas { get; set; }
    public decimal TotalVendido { get; set; }
    public List<ProductoVendidoViewModel> MasVendidos { get; set; } = new();
    public List<FacturaResumenViewModel> Facturas { get; set; } = new();
    public List<ProductoStockViewModel> BajoStock { get; set; } = new();
}

public record ProductoVendidoViewModel(string Nombre, int Cantidad, decimal Importe);
public record FacturaResumenViewModel(string Numero, DateTime Fecha, string Cliente, decimal Total, string Estado);
public record ProductoStockViewModel(string Nombre, int Existencias, int Minimo);
