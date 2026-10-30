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
}
