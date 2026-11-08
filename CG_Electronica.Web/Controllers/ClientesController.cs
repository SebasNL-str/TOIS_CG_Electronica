using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using CG_Electronica.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CG_Electronica.Web.Controllers;

[Authorize(Policy = "Permiso:Clientes.Ver")]
public class ClientesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IServicioAuditoria _servicioAuditoria;

    public ClientesController(AppDbContext context, IServicioAuditoria servicioAuditoria)
    {
        _context = context;
        _servicioAuditoria = servicioAuditoria;
    }

    [Authorize(Policy = "Permiso:Clientes.Ver")]
    public async Task<IActionResult> Index()
    {
        var clientes = await _context.Clientes.ToListAsync();
        return View(clientes);
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

        var nuevoCliente = new Cliente
        {
            NombreCliente = model.NombreCliente,
            Telefono = model.Telefono,
            Cedula = model.Cedula
        };

        _context.Clientes.Add(nuevoCliente);
        await _context.SaveChangesAsync();

        int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
        await _servicioAuditoria.RegistrarAccionAsync("CREAR_CLIENTE", $"CLIENTE: {nuevoCliente.IdCliente}", $"Se creó el cliente {nuevoCliente.NombreCliente} (Cédula: {nuevoCliente.Cedula}).", idUsuario);

        TempData["Ok"] = "Cliente creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Edit
    [Authorize(Policy = "Permiso:Clientes.Editar")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        var model = new Models.ViewModels.ClienteViewModel
        {
            IdCliente = cliente.IdCliente,
            Cedula = cliente.Cedula,
            NombreCliente = cliente.NombreCliente,
            Telefono = cliente.Telefono,
            Estado = cliente.Estado // <--- Mapeamos el estado
        };

        return View(model);
    }


    // POST: Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Permiso:Clientes.Editar")]
    public async Task<IActionResult> Edit(int id, Models.ViewModels.ClienteViewModel model)
    {
        if (id != model.IdCliente) return NotFound();

        if (ModelState.IsValid)
        {
            bool existeCedula = await _context.Clientes
                .AnyAsync(c => c.Cedula == model.Cedula && c.IdCliente != id);

            if (existeCedula)
            {
                ModelState.AddModelError("Cedula", "Ya existe otro cliente con esta cédula.");
            }
            else
            {
                var clienteDb = await _context.Clientes.FindAsync(id);
                if (clienteDb == null) return NotFound();

                clienteDb.Cedula = model.Cedula;
                clienteDb.NombreCliente = model.NombreCliente;
                clienteDb.Telefono = model.Telefono;
                clienteDb.Estado = model.Estado; // <--- Guardamos el estado

                _context.Update(clienteDb);
                await _context.SaveChangesAsync();

                int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
                await _servicioAuditoria.RegistrarAccionAsync("EDITAR_CLIENTE", $"CLIENTE: {clienteDb.IdCliente}", $"Se actualizó el cliente {clienteDb.NombreCliente} (Estado: {clienteDb.Estado}).", idUsuario);

                TempData["Ok"] = "Cliente actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
        }
        return View(model);
    }

    [Authorize(Policy = "Permiso:Clientes.Editar")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();
        return View(cliente);
    }

    // POST: Delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Permiso:Clientes.Editar")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            
            cliente.Estado = "Inactivo";

            _context.Update(cliente);
            await _context.SaveChangesAsync();

            int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
            await _servicioAuditoria.RegistrarAccionAsync("INACTIVAR_CLIENTE", $"CLIENTE: {cliente.IdCliente}", $"Se inactivó al cliente {cliente.NombreCliente}.", idUsuario);

            TempData["Ok"] = "Cliente inactivado correctamente.";
        }
        return RedirectToAction(nameof(Index));
    }
}