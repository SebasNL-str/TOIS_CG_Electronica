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
public class MarcasController : Controller
{
    private readonly AppDbContext _context;
    private readonly IServicioAuditoria _servicioAuditoria;

    public MarcasController(AppDbContext context, IServicioAuditoria servicioAuditoria)
    {
        _context = context;
        _servicioAuditoria = servicioAuditoria;
    }

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

        var nuevaMarca = new Marca { NombreMarca = model.NombreMarca, Estado = model.Estado };
        _context.Marcas.Add(nuevaMarca);
        await _context.SaveChangesAsync();

        int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
        await _servicioAuditoria.RegistrarAccionAsync("CREAR_MARCA", $"MARCA: {nuevaMarca.IdMarca}", $"Se creó la marca {nuevaMarca.NombreMarca}.", idUsuario);

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

        int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
        await _servicioAuditoria.RegistrarAccionAsync("EDITAR_MARCA", $"MARCA: {marca.IdMarca}", $"Se actualizó la marca {marca.NombreMarca} (Estado: {marca.Estado}).", idUsuario);

        TempData["Ok"] = "Marca actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Permiso:Catalogos.Editar")]
    public async Task<IActionResult> Delete(int id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null)
        {
            TempData["Error"] = $"Error 404: No se encontró ninguna marca con el ID {id}.";
            return RedirectToAction(nameof(Index));
        }

        // Consulta de conteo
        var tieneProductos = await _context.Database
            .SqlQuery<int>($"SELECT COUNT(*) AS Value FROM PRODUCTO WHERE idMarca = {id}")
            .FirstAsync();

        if (tieneProductos > 0)
        {
            // Diagnóstico: Te dirá el nombre, el ID recibido y cuántos productos contó C#
            TempData["Error"] = $"[BLOQUEADO] La marca '{marca.NombreMarca}' (ID: {id}) tiene {tieneProductos} producto(s) asociado(s).";
            return RedirectToAction(nameof(Index));
        }

        // Si C# cree que tiene 0 productos, te lo dirá en un mensaje verde en vez de quedarse callado
        TempData["Ok"] = $"[PERMITIDO] La marca '{marca.NombreMarca}' (ID: {id}) tiene 0 productos según C#. Intentando abrir vista...";
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
            string nombreMarcaInactivada = marca.NombreMarca;

            // 1. BORRADO LÓGICO: Cambiamos estado y actualizamos en lugar de Remove
            marca.Estado = "Inactivo";
            _context.Marcas.Update(marca);
            await _context.SaveChangesAsync();

            // 2. AUDITORÍA: Adaptamos el registro para reflejar que fue una inactivación
            int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
            await _servicioAuditoria.RegistrarAccionAsync("INACTIVAR_MARCA", $"MARCA: {id}", $"Se inactivó la marca {nombreMarcaInactivada}.", idUsuario);

            TempData["Ok"] = "Marca inactivada correctamente.";
        }
        return RedirectToAction(nameof(Index));
    }
}