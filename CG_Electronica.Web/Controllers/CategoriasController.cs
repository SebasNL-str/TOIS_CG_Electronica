using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers;

[Authorize(Policy = "Permiso:Catalogos.Ver")]
public class CategoriasController : Controller
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index()
        => View(await _context.Categorias.OrderBy(c => c.NombreCategoria).ToListAsync());

    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public IActionResult Create() => View(new CategoriaViewModel());

    [HttpPost]
    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoriaViewModel model)
    {
        if (await _context.Categorias.AnyAsync(c => c.NombreCategoria == model.NombreCategoria))
            ModelState.AddModelError(nameof(model.NombreCategoria), "Ya existe una categoría con ese nombre.");

        if (!ModelState.IsValid) return View(model);

        _context.Categorias.Add(new Categoria { NombreCategoria = model.NombreCategoria });
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Categoría creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public async Task<IActionResult> Edit(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();
        return View(new CategoriaViewModel
        {
            IdCategoria = categoria.IdCategoria,
            NombreCategoria = categoria.NombreCategoria
        });
    }

    [HttpPost]
    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoriaViewModel model)
    {
        if (await _context.Categorias.AnyAsync(c => c.NombreCategoria == model.NombreCategoria && c.IdCategoria != model.IdCategoria))
            ModelState.AddModelError(nameof(model.NombreCategoria), "Ya existe OTRA categoría con ese nombre.");

        if (!ModelState.IsValid) return View(model);

        var categoria = await _context.Categorias.FindAsync(model.IdCategoria);
        if (categoria == null) return NotFound();

        categoria.NombreCategoria = model.NombreCategoria;
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Categoría actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();
        return View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria != null)
        {
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Categoría eliminada correctamente.";
        }
        return RedirectToAction(nameof(Index));
    }
}