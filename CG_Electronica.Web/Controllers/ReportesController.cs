using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using CG_Electronica.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers;

[Authorize(Policy = "Permiso:Reportes.Ver")]
public class ReportesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IServicioInventario _servicioInventario;

    public ReportesController(AppDbContext context, IServicioInventario servicioInventario)
    { _context = context; _servicioInventario = servicioInventario; }

    public async Task<IActionResult> Ventas(DateTime? desde, DateTime? hasta)
    {
        var inicio = (desde ?? DateTime.Today.AddDays(-30)).Date;
        var fin = (hasta ?? DateTime.Today).Date.AddDays(1);
        var facturas = await _context.Facturas.Include(f => f.Detalles).ThenInclude(d => d.Item).Where(f => f.Fecha >= inicio && f.Fecha < fin).OrderByDescending(f => f.Fecha).ToListAsync();
        var detalles = facturas.Where(f => f.Estado == "Emitida").SelectMany(f => f.Detalles).ToList();
        var vm = new ReporteVentasViewModel
        {
            Desde = inicio, Hasta = fin.AddDays(-1), CantidadFacturas = facturas.Count(f => f.Estado == "Emitida"),
            TotalVendido = facturas.Where(f => f.Estado == "Emitida").Sum(f => f.Total),
            Facturas = facturas.Select(f => new FacturaResumenViewModel(f.NumeroFactura, f.Fecha, f.NombreClienteHistorico ?? "Sin nombre", f.Total, f.Estado)).ToList(),
            MasVendidos = detalles.GroupBy(d => d.Item).Select(g => new ProductoVendidoViewModel(NombreItem(g.Key), g.Sum(d => d.Cantidad), g.Sum(d => d.Cantidad * d.PrecioUnitarioHistorico))).OrderByDescending(x => x.Cantidad).Take(10).ToList()
        };
        var bajo = await _servicioInventario.ObtenerProductosConStockMinimoAsync();
        vm.BajoStock = bajo.Select(p => new ProductoStockViewModel(p.Descripcion, p.Existencias, p.ExistenciasMinimas)).ToList();
        return View(vm);
    }

    private static string NombreItem(ItemFacturable item) => item switch { Producto p => p.Descripcion, Servicio s => s.NombreServicio, _ => $"Ítem {item.IdItem}" };
}
