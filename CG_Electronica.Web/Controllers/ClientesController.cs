using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers;

[Authorize(Policy = "Permiso:Clientes.Ver")]
public class ClientesController : Controller
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var lista = await _context.Clientes
            .OrderBy(c => c.NombreCliente)
            .ToListAsync();
        return View(lista);
    }

    // Validación remota de cédula única (la usa el [Remote] del ViewModel)
    [HttpGet] // Remote usa POST por defecto con antiforgery; lo dejamos GET para simplicidad:
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> VerificarCedula(string cedula, int idCliente)
    {
        var existe = await _context.Clientes
            .AnyAsync(c => c.Cedula == cedula && c.IdCliente != idCliente);
        return Json(!existe);
    }

    [Authorize(Policy = "Permiso:Clientes.Editar")]
    public IActionResult Create() => View(new ClienteViewModel());

    [HttpPost]
    [Authorize(Policy = "Permiso:Clientes.Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteViewModel model)
    {
        // Validación SERVER-SIDE de la regla de negocio (no confiar solo en el cliente)
        if (await _context.Clientes.AnyAsync(c => c.Cedula == model.Cedula))
            ModelState.AddModelError(nameof(model.Cedula), "Ya existe un cliente registrado con esta cédula.");

        if (!ModelState.IsValid) return View(model);

        _context.Clientes.Add(new Cliente
        {
            NombreCliente = model.NombreCliente,
            Telefono = model.Telefono,
            Cedula = model.Cedula
        });
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Cliente creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Permiso:Clientes.Editar")]
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        return View(new ClienteViewModel
        {
            IdCliente = cliente.IdCliente,
            NombreCliente = cliente.NombreCliente,
            Telefono = cliente.Telefono,
            Cedula = cliente.Cedula
        });
    }

    [HttpPost]
    [Authorize(Policy = "Permiso:Clientes.Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClienteViewModel model)
    {
        if (await _context.Clientes.AnyAsync(c => c.Cedula == model.Cedula && c.IdCliente != model.IdCliente))
            ModelState.AddModelError(nameof(model.Cedula), "Ya existe OTRO cliente registrado con esta cédula.");

        if (!ModelState.IsValid) return View(model);

        var cliente = await _context.Clientes.FindAsync(model.IdCliente);
        if (cliente == null) return NotFound();

        cliente.NombreCliente = model.NombreCliente;
        cliente.Telefono = model.Telefono;
        cliente.Cedula = model.Cedula;
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "Permiso:Clientes.Editar")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();
        return View(cliente);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Policy = "Permiso:Clientes.Editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Cliente eliminado correctamente.";
        }
        return RedirectToAction(nameof(Index));
    }
}
