using CG_Electronica.Web.Models;

namespace CG_Electronica.Web.Models.ViewModels;

public class DashboardViewModel
{
    // Métricas principales del sistema
    public decimal TotalVentasPeriodo { get; set; }
    public int CantidadFacturasEmitidas { get; set; }
    public int CantidadFacturasAnuladas { get; set; }
    public int TotalClientes { get; set; }
    public int TotalProductos { get; set; }
    public int ProductosStockBajo { get; set; }

    // Listado para el widget de actividad reciente
    public List<Auditoria> UltimasAuditorias { get; set; } = new();

    // Banderas de control de permisos para la interfaz
    public bool TienePermisoCatalogos { get; set; }
    public bool TienePermisoClientes { get; set; }
    public bool TienePermisoAuditoria { get; set; }
    public bool TienePermisoInventario { get; set; }
}
