using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public HomeController(AppDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel();

        // 1. Estadísticas de Facturación / Ventas
        try
        {
            model.TotalVentasPeriodo = await _context.Facturas
            .Where(f => f.Estado != "Anulada")
            .SumAsync(f => (decimal?)f.Total) ?? 0m;

            model.CantidadFacturasEmitidas = await _context.Facturas
            .CountAsync(f => f.Estado != "Anulada");

            model.CantidadFacturasAnuladas = await _context.Facturas
            .CountAsync(f => f.Estado == "Anulada");
        }
        catch
        {
            model.TotalVentasPeriodo = 0;
            model.CantidadFacturasEmitidas = 0;
            model.CantidadFacturasAnuladas = 0;
        }

        // 2. Conteo de Clientes
        try
        {
            model.TotalClientes = await _context.Clientes.CountAsync();
        }
        catch
        {
            model.TotalClientes = 0;
        }

        // 3. Conteo de Productos y Alerta de Stock Bajo (Existencias <= 5)
        try
        {
            model.TotalProductos = await _context.Productos.CountAsync();
            model.ProductosStockBajo = await _context.Productos.CountAsync(p => p.Existencias <= 5);
        }
        catch
        {
            model.TotalProductos = 0;
            model.ProductosStockBajo = 0;
        }

        // 4. Verificación de Permisos del Usuario Autenticado (Corrección de CS0019 y CS8602)
        var authInventarioResult = await _authorizationService.AuthorizeAsync(User, "Permiso:Inventario.Ver");
        bool isAuthenticated = User.Identity?.IsAuthenticated ?? false;

        model.TienePermisoCatalogos = (await _authorizationService.AuthorizeAsync(User, "Permiso:Catalogos.Ver")).Succeeded;
        model.TienePermisoClientes = (await _authorizationService.AuthorizeAsync(User, "Permiso:Clientes.Ver")).Succeeded;
        model.TienePermisoAuditoria = (await _authorizationService.AuthorizeAsync(User, "Permiso:Auditoria.Ver")).Succeeded;
        model.TienePermisoInventario = authInventarioResult.Succeeded || isAuthenticated;

        // 5. Actividad Reciente de Auditoría (Respetando el permiso correspondiente)
        if (model.TienePermisoAuditoria)
        {
            try
            {
                model.UltimasAuditorias = await _context.Auditorias
                .Include(a => a.Usuario)
                .OrderByDescending(a => a.FechaHora)
                .Take(5)
                .ToListAsync();
            }
            catch
            {
                model.UltimasAuditorias = new List<Auditoria>();
            }
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
