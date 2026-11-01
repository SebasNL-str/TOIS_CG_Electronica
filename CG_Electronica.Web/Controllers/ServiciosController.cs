
using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers;

[Authorize(Policy = "Permiso:Catalogos.Ver")]
public class ServiciosController : Controller
{
    private readonly AppDbContext _context;

    public ServiciosController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var servicios = await _context.Servicios
        .OrderBy(s => s.NombreServicio)
        .ToListAsync();
        return View(servicios);
    }

    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public IActionResult Create()
    {
        return View(new ServicioCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Permiso:Catalogos.Editar")]
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

        TempData["Ok"] = "Servicio creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Servicios/Edit/5
    [HttpGet]
    [Authorize(Policy = "Permiso:ServiciosEditar")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio == null) return NotFound();

        return View(servicio);
    }

    // POST: Servicios/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Permiso:ServiciosEditar")]
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
