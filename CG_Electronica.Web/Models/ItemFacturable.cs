namespace CG_Electronica.Web.Models;

public abstract class ItemFacturable
{
    public int IdItem { get; set; }
    public decimal PrecioVenta { get; set; }
    public string Estado { get; set; } = "Activo"; // Activo, Inactivo, Suspendido

    // Relaciones de navegación
    public virtual ICollection<DetalleFactura> DetallesFactura { get; set; } = new List<DetalleFactura>();
}
