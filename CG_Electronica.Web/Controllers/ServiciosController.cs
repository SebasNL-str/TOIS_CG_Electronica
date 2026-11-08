using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using CG_Electronica.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CG_Electronica.Web.Controllers;

[Authorize(Policy = "Permiso:Servicios.Ver")]
public class ServiciosController : Controller
{
    private readonly AppDbContext _context;
    private readonly IServicioAuditoria _servicioAuditoria;

    public ServiciosController(AppDbContext context, IServicioAuditoria servicioAuditoria)
    {
        _context = context;
        _servicioAuditoria = servicioAuditoria;
    }

    public async Task<IActionResult> Index()
    {
        var servicios = await _context.Servicios
        .OrderBy(s => s.NombreServicio)
        .ToListAsync();
        return View(servicios);
    }

    [Authorize(Policy = "Permiso:Servicios.Editar")]
    public IActionResult Create()
    {
        return View(new ServicioCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Permiso:Servicios.Editar")]
    public async Task<IActionResult> Create(ServicioCreateViewModel model)
    {
        if (await _context.Servicios.AnyAsync(s => s.NombreServicio.ToLower() == model.NombreServicio.ToLower()))
        {
            ModelState.AddModelError(nameof(model.NombreServicio), "Ya existe un servicio registrado con este nombre.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var servicio = new Servicio
        {
            NombreServicio = model.NombreServicio,
            PrecioVenta = model.PrecioVenta,
            Estado = model.Estado
        };

        _context.Servicios.Add(servicio);
        await _context.SaveChangesAsync();

        int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
        await _servicioAuditoria.RegistrarAccionAsync("CREAR_SERVICIO", $"SERVICIO: {servicio.IdItem}", $"Se creó el servicio {servicio.NombreServicio}.", idUsuario);

        TempData["Ok"] = "Servicio creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Edit
    [HttpGet]
    [Authorize(Policy = "Permiso:Servicios.Editar")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio == null) return NotFound();

        return View(servicio);
    }

    // POST: Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Permiso:Servicios.Editar")]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("IdItem,NombreServicio,PrecioVenta,Estado")] Servicio servicio)
    {
        if (id != servicio.IdItem) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var servicioDb = await _context.Servicios
                .FirstOrDefaultAsync(s => s.IdItem == id);

                if (servicioDb == null) return NotFound();

                // Actualizar únicamente los campos editables del servicio.
                servicioDb.NombreServicio = servicio.NombreServicio;
                servicioDb.PrecioVenta = servicio.PrecioVenta;
                servicioDb.Estado = servicio.Estado;

                await _context.SaveChangesAsync();

                int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
                await _servicioAuditoria.RegistrarAccionAsync("EDITAR_SERVICIO", $"SERVICIO: {servicioDb.IdItem}", $"Se actualizó el servicio {servicioDb.NombreServicio} (Precio Venta: {servicioDb.PrecioVenta:C}, Estado: {servicioDb.Estado}).", idUsuario);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Servicios.AnyAsync(e => e.IdItem == servicio.IdItem))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(servicio);
    }
}