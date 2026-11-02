using System.Security.Claims;
using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using CG_Electronica.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers;

[Authorize]
public class FacturasController : Controller
{
    private readonly AppDbContext _context;
    private readonly IServicioVentas _servicioVentas;

    public FacturasController(AppDbContext context, IServicioVentas servicioVentas)
    { _context = context; _servicioVentas = servicioVentas; }

    [Authorize(Policy = "Permiso:Facturas.Ver")]
    public async Task<IActionResult> Index(string? busqueda, string? estado)
    {
        var query = _context.Facturas.Include(f => f.Cliente).Include(f => f.Usuario).AsQueryable();
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            busqueda = busqueda.Trim();
            query = query.Where(f => f.NumeroFactura.Contains(busqueda) ||
                (f.NombreClienteHistorico != null && f.NombreClienteHistorico.Contains(busqueda)));
        }
        if (!string.IsNullOrWhiteSpace(estado)) query = query.Where(f => f.Estado == estado);
        ViewBag.Busqueda = busqueda;
        ViewBag.Estado = estado;
        return View(await query.OrderByDescending(f => f.Fecha).ToListAsync());
    }

    [HttpGet]
    [Authorize(Policy = "Permiso:Ventas.Ver")]
    public async Task<IActionResult> Create()
    {
        await CargarListasAsync();
        return View(new ViewModelFacturaCreate { NumeroFactura = $"FAC-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid():N}"[..19].ToUpperInvariant() });
    }

    [HttpPost]
    [Authorize(Policy = "Permiso:Ventas.Ver")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ViewModelFacturaCreate model, List<ItemFacturaDto> items)
    {
        if (items is null || items.Count == 0) ModelState.AddModelError("", "Debe agregar al menos un ítem a la factura.");
        if (!ModelState.IsValid) { await CargarListasAsync(); return View(model); }
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario)) return Unauthorized();
        try
        {
            var factura = await _servicioVentas.RegistrarVentaAsync(new VentaRequestDTO
            {
                NumeroFactura = model.NumeroFactura, IdCliente = model.IdCliente,
                Detalles = (items ?? new List<ItemFacturaDto>()).Select(i => new DetalleVentaDTO { IdItem = i.IdItem, Cantidad = i.Cantidad }).ToList()
            }, idUsuario);
            return RedirectToAction(nameof(Details), new { id = factura.IdFactura });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await CargarListasAsync();
            return View(model);
        }
    }

    [Authorize(Policy = "Permiso:Facturas.Ver")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var factura = await _context.Facturas.Include(f => f.Cliente).Include(f => f.Usuario).Include(f => f.Detalles).ThenInclude(d => d.Item).SingleOrDefaultAsync(f => f.IdFactura == id);
        return factura is null ? NotFound() : View(factura);
    }

    [HttpPost]
    [Authorize(Policy = "Permiso:Facturas.Anular")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario)) return Unauthorized();
        try { await _servicioVentas.AnularFacturaAsync(id, idUsuario); TempData["Mensaje"] = "Factura anulada y stock reintegrado."; }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Clientes = new SelectList(await _context.Clientes.Where(c => c.Estado == "Activo").OrderBy(c => c.NombreCliente).ToListAsync(), "IdCliente", "NombreCliente");
        var items = await _context.ItemsFacturables.Where(i => i.Estado == "Activo").OrderBy(i => i.IdItem).ToListAsync();
        ViewBag.Items = items.Select(i => new { i.IdItem, Nombre = i switch { Producto p => p.Descripcion, Servicio s => s.NombreServicio, _ => $"Ítem {i.IdItem}" }, i.PrecioVenta }).ToList();
    }
}
