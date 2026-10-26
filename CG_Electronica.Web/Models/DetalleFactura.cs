namespace CG_Electronica.Web.Models;

public class DetalleFactura
{
    public int IdDetalle { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitarioHistorico { get; set; }

    public int IdFactura { get; set; }
    public virtual Factura Factura { get; set; } = null!;

    public int IdItem { get; set; }
    public virtual ItemFacturable Item { get; set; } = null!;
}