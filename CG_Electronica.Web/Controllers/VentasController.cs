using System.Security.Claims;
using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models.ViewModels;
using CG_Electronica.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers;

[Authorize(Policy = "Permiso:Ventas.Ver")]
public class VentasController : Controller
{
    private readonly AppDbContext _context;
    private readonly IServicioVentas _servicioVentas;

    public VentasController(AppDbContext context, IServicioVentas servicioVentas)
    { _context = context; _servicioVentas = servicioVentas; }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        ViewBag.Clientes = new SelectList(await _context.Clientes.Where(c => c.Estado == "Activo").OrderBy(c => c.NombreCliente).ToListAsync(), "IdCliente", "NombreCliente");
        ViewBag.Items = await _context.ItemsFacturables.Where(i => i.Estado == "Activo").OrderBy(i => i.IdItem).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarVenta([FromBody] VentaRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario)) return Unauthorized(new { success = false, mensaje = "Sesión inválida o expirada." });
        try
        {
            var factura = await _servicioVentas.RegistrarVentaAsync(request, idUsuario);
            return Ok(new { success = true, facturaId = factura.IdFactura, mensaje = "Venta registrada con éxito." });
        }
        catch (Exception ex) { return BadRequest(new { success = false, mensaje = ex.Message }); }
    }
}
