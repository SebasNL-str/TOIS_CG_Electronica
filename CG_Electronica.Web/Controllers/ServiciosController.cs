using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers;

[Authorize]
public class ServiciosController : Controller
{
    private readonly AppDbContext _context;

    public ServiciosController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Servicios.ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Servicio servicio)
    {
        ModelState.Remove(nameof(servicio.DetallesFactura));

        if (ModelState.IsValid)
        {
            _context.ItemsFacturables.Add(servicio);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(servicio);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio == null) return NotFound();

        return View(servicio);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Servicio servicio)
    {
        if (id != servicio.IdItem) return NotFound();

        ModelState.Remove(nameof(servicio.DetallesFactura));

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(servicio);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Servicios.Any(s => s.IdItem == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(servicio);
    }
}