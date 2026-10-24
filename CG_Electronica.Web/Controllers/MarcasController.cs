using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers;

[Authorize(Policy = "Permiso:Catalogos.Ver")]
public class MarcasController : Controller
{
    private readonly AppDbContext _context;

    public MarcasController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index()
        => View(await _context.Marcas.OrderBy(m => m.NombreMarca).ToListAsync());

    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public IActionResult Create() => View(new MarcaViewModel());

    [HttpPost]
    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MarcaViewModel model)
    {
        if (await _context.Marcas.AnyAsync(m => m.NombreMarca == model.NombreMarca))
            ModelState.AddModelError(nameof(model.NombreMarca), "Ya existe una marca con ese nombre.");

        if (!ModelState.IsValid) return View(model);

        _context.Marcas.Add(new Marca { NombreMarca = model.NombreMarca, Estado = model.Estado });
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Marca creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public async Task<IActionResult> Edit(int id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null) return NotFound();
        return View(new MarcaViewModel
        {
            IdMarca = marca.IdMarca,
            NombreMarca = marca.NombreMarca,
            Estado = marca.Estado
        });
    }

    [HttpPost]
    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MarcaViewModel model)
    {
        if (await _context.Marcas.AnyAsync(m => m.NombreMarca == model.NombreMarca && m.IdMarca != model.IdMarca))
            ModelState.AddModelError(nameof(model.NombreMarca), "Ya existe OTRA marca con ese nombre.");

        if (!ModelState.IsValid) return View(model);

        var marca = await _context.Marcas.FindAsync(model.IdMarca);
        if (marca == null) return NotFound();

        marca.NombreMarca = model.NombreMarca;
        marca.Estado = model.Estado;
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Marca actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public async Task<IActionResult> Delete(int id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null) return NotFound();

        // Evitar borrar marcas con productos asociados (integridad con el módulo de otros integrantes)
        var tieneProductos = await _context.Database
            .SqlQuery<int>($"SELECT COUNT(*) AS Value FROM PRODUCTO WHERE idMarca = {id}")
            .FirstAsync();
        if (tieneProductos > 0)
        {
            TempData["Error"] = "No se puede eliminar: la marca tiene productos asociados.";
            return RedirectToAction(nameof(Index));
        }
        return View(marca);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca != null)
        {
            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Marca eliminada correctamente.";
        }
        return RedirectToAction(nameof(Index));
    }
}