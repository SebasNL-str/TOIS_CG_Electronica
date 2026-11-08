using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using CG_Electronica.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CG_Electronica.Web.Controllers;

[Authorize(Policy = "Permiso:Catalogos.Ver")]
public class CategoriasController : Controller
{
    private readonly AppDbContext _context;
    private readonly IServicioAuditoria _servicioAuditoria;

    public CategoriasController(AppDbContext context, IServicioAuditoria servicioAuditoria)
    {
        _context = context;
        _servicioAuditoria = servicioAuditoria;
    }

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

        var nuevaCategoria = new Categoria
        {
            NombreCategoria = model.NombreCategoria,
            Estado = "Activo" // <--- Asignamos el estado inicial requerido por la BD
        };

        _context.Categorias.Add(nuevaCategoria);
        await _context.SaveChangesAsync();

        int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
        await _servicioAuditoria.RegistrarAccionAsync("CREAR_CATEGORIA", $"CATEGORIA: {nuevaCategoria.IdCategoria}", $"Se creó la categoría {nuevaCategoria.NombreCategoria}.", idUsuario);

        TempData["Ok"] = "Categoría creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();

        var model = new Models.ViewModels.CategoriaViewModel
        {
            IdCategoria = categoria.IdCategoria,
            NombreCategoria = categoria.NombreCategoria,
            Estado = categoria.Estado // <--- Mapeamos el estado
        };

        return View(model);
    }


    // POST: Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public async Task<IActionResult> Edit(int id, Models.ViewModels.CategoriaViewModel model)
    {
        if (id != model.IdCategoria) return NotFound();

        if (ModelState.IsValid)
        {
            bool existe = await _context.Categorias
                .AnyAsync(c => c.NombreCategoria.ToLower() == model.NombreCategoria.ToLower() && c.IdCategoria != id);

            if (existe)
            {
                ModelState.AddModelError("NombreCategoria", "Ya existe otra categoría con este nombre.");
            }
            else
            {
                var categoriaDb = await _context.Categorias.FindAsync(id);
                if (categoriaDb == null) return NotFound();

                categoriaDb.NombreCategoria = model.NombreCategoria;
                categoriaDb.Estado = model.Estado; // <--- Guardamos el estado

                _context.Update(categoriaDb);
                await _context.SaveChangesAsync();

                int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
                await _servicioAuditoria.RegistrarAccionAsync("EDITAR_CATEGORIA", $"CATEGORIA: {categoriaDb.IdCategoria}", $"Se actualizó la categoría {categoriaDb.NombreCategoria} (Estado: {categoriaDb.Estado}).", idUsuario);

                TempData["Ok"] = "Categoría actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
        }
        return View(model);
    }

    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();
        return View(categoria);
    }

    // POST: Delete 
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria != null)
        {
            
            categoria.Estado = "Inactivo";

            _context.Update(categoria);
            await _context.SaveChangesAsync();

            int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
            await _servicioAuditoria.RegistrarAccionAsync("INACTIVAR_CATEGORIA", $"CATEGORIA: {categoria.IdCategoria}", $"Se inactivó la categoría {categoria.NombreCategoria}.", idUsuario);

            TempData["Ok"] = "Categoría inactivada correctamente.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> VerificarNombreCategoria(string nombreCategoria, int idCategoria = 0)
    {
        bool existe = await _context.Categorias
        .AnyAsync(c => c.NombreCategoria.ToLower() == nombreCategoria.ToLower() && c.IdCategoria != idCategoria);

        if (existe)
        {
            return Json($"Ya existe una categoría con el nombre '{nombreCategoria}'.");
        }

        return Json(true);
    }
}