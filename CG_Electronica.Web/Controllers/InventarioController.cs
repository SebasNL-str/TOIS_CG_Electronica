using CG_Electronica.Web.Data;
using CG_Electronica.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CG_Electronica.Web.Controllers;

[Authorize]
public class InventarioController : Controller
{
    private readonly AppDbContext _context;
    private readonly IServicioInventario _servicioInventario;

    public InventarioController(AppDbContext context, IServicioInventario servicioInventario)
    {
        _context = context;
        _servicioInventario = servicioInventario;
    }

    // Kardex / Historial de Movimientos
    public async Task<IActionResult> Index(int? idProducto)
    {
        ViewBag.Productos = new SelectList(_context.Productos, "IdItem", "Descripcion", idProducto);

        var query = _context.MovimientosInventario
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .AsQueryable();

        if (idProducto.HasValue)
        {
            query = query.Where(m => m.IdProducto == idProducto.Value);
        }

        var movimientos = await query.OrderByDescending(m => m.FechaHora).ToListAsync();
        return View(movimientos);
    }

    // Registrar movimiento manual (Entrada por compra, Ajuste, etc.)
    public IActionResult RegistrarMovimiento()
    {
        ViewBag.Productos = new SelectList(_context.Productos, "IdItem", "Descripcion");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarMovimiento(int idProducto, string tipoMovimiento, string motivo, int cantidad, decimal precioCompraUnitario, string observacion)
    {
        try
        {
            int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;

            if (tipoMovimiento == "Entrada")
            {
                await _servicioInventario.RegistrarEntradaAsync(idProducto, cantidad, precioCompraUnitario, motivo, idUsuario, observacion);
            }
            else if (tipoMovimiento == "Salida")
            {
                await _servicioInventario.RegistrarSalidaAsync(idProducto, cantidad, motivo, idUsuario, observacion);
            }

            TempData["Mensaje"] = "Movimiento registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Productos = new SelectList(_context.Productos, "IdItem", "Descripcion", idProducto);
            return View();
        }
    }
}