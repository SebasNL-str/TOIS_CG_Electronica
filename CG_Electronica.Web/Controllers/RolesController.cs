using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using CG_Electronica.Web.Services; // <-- Agregado para Auditoría
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // <-- Agregado para ID de usuario

namespace CG_Electronica.Web.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IServicioAuditoria _servicioAuditoria; // <-- Inyectamos auditoría

        public RolesController(AppDbContext context, IServicioAuditoria servicioAuditoria)
        {
            _context = context;
            _servicioAuditoria = servicioAuditoria;
        }

        // 1. GET: Listado de Roles
        [Authorize(Policy = "Permiso:Roles.Ver")]
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles
                .Include(r => r.RolPermisos)
                .ToListAsync();

            return View(roles);
        }

        // 2. GET: Validación Remota
        [HttpGet]
        public async Task<IActionResult> VerificarNombreRol(string nombreRol, int idRol)
        {
            var existe = await _context.Roles
                .AnyAsync(r => r.NombreRol.ToLower() == nombreRol.ToLower() && r.IdRol != idRol);

            if (existe)
                return Json($"El rol '{nombreRol}' ya existe.");

            return Json(true);
        }

        // 3. GET: Crear Rol
        [Authorize(Policy = "Permiso:Roles.Editar")]
        public async Task<IActionResult> Create()
        {
            var model = new RolViewModel();
            var permisosDb = await _context.Permisos.ToListAsync();

            foreach (var permiso in permisosDb)
            {
                model.Permisos.Add(new PermisoCheckboxViewModel
                {
                    IdPermiso = permiso.IdPermiso,
                    NombrePermiso = permiso.NombrePermiso,
                    Descripcion = permiso.Descripcion,
                    Seleccionado = false
                });
            }

            return View(model);
        }

        // 4. POST: Guardar Nuevo Rol
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permiso:Roles.Editar")]
        public async Task<IActionResult> Create(RolViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool existe = await _context.Roles.AnyAsync(r => r.NombreRol.ToLower() == model.NombreRol.ToLower());
                if (existe)
                {
                    ModelState.AddModelError("NombreRol", "El nombre de rol ya existe.");
                }
                else
                {
                    var nuevoRol = new Rol
                    {
                        NombreRol = model.NombreRol,
                        Descripcion = model.Descripcion,
                        Estado = model.Estado
                    };

                    _context.Roles.Add(nuevoRol);
                    await _context.SaveChangesAsync();

                    var seleccionados = model.Permisos.Where(p => p.Seleccionado).ToList();
                    foreach (var item in seleccionados)
                    {
                        _context.RolPermisos.Add(new RolPermiso
                        {
                            IdRol = nuevoRol.IdRol,
                            IdPermiso = item.IdPermiso
                        });
                    }

                    await _context.SaveChangesAsync();

                    // AUDITORÍA: Registrar creación
                    int idUsuarioActual = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
                    await _servicioAuditoria.RegistrarAccionAsync("CREAR_ROL", $"ROL: {nuevoRol.IdRol}", $"Se creó el rol {nuevoRol.NombreRol}.", idUsuarioActual);

                    TempData["Ok"] = "Rol creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        // 5. GET: Editar Rol
        [Authorize(Policy = "Permiso:Roles.Editar")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var rolDb = await _context.Roles
                .Include(r => r.RolPermisos)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (rolDb == null) return NotFound();

            var model = new RolViewModel
            {
                IdRol = rolDb.IdRol,
                NombreRol = rolDb.NombreRol,
                Descripcion = rolDb.Descripcion,
                Estado = rolDb.Estado
            };

            var permisosDb = await _context.Permisos.ToListAsync();

            foreach (var permiso in permisosDb)
            {
                bool tienePermiso = rolDb.RolPermisos.Any(rp => rp.IdPermiso == permiso.IdPermiso);

                model.Permisos.Add(new PermisoCheckboxViewModel
                {
                    IdPermiso = permiso.IdPermiso,
                    NombrePermiso = permiso.NombrePermiso,
                    Descripcion = permiso.Descripcion,
                    Seleccionado = tienePermiso
                });
            }

            return View(model);
        }

        // 6. POST: Guardar Edición
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permiso:Roles.Editar")]
        public async Task<IActionResult> Edit(int id, RolViewModel model)
        {
            if (id != model.IdRol) return NotFound();

            if (ModelState.IsValid)
            {
                bool existe = await _context.Roles.AnyAsync(r => r.NombreRol.ToLower() == model.NombreRol.ToLower() && r.IdRol != id);
                if (existe)
                {
                    ModelState.AddModelError("NombreRol", "El nombre de rol ya está registrado.");
                }
                else
                {
                    var rolDb = await _context.Roles
                        .Include(r => r.RolPermisos)
                        .FirstOrDefaultAsync(r => r.IdRol == id);

                    if (rolDb == null) return NotFound();

                    if (rolDb.NombreRol.ToLower() == "administrador" && model.Estado != "Activo")
                    {
                        ModelState.AddModelError("Estado", "El rol de Administrador principal no puede ser inactivado.");
                        return View(model);
                    }

                    rolDb.NombreRol = model.NombreRol;
                    rolDb.Descripcion = model.Descripcion;
                    rolDb.Estado = model.Estado;

                    _context.RolPermisos.RemoveRange(rolDb.RolPermisos);

                    var seleccionados = model.Permisos.Where(p => p.Seleccionado).ToList();
                    foreach (var item in seleccionados)
                    {
                        rolDb.RolPermisos.Add(new RolPermiso
                        {
                            IdRol = rolDb.IdRol,
                            IdPermiso = item.IdPermiso
                        });
                    }

                    _context.Update(rolDb);
                    await _context.SaveChangesAsync();

                    // AUDITORÍA: Registrar edición
                    int idUsuarioActual = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
                    await _servicioAuditoria.RegistrarAccionAsync("EDITAR_ROL", $"ROL: {rolDb.IdRol}", $"Se actualizó el rol {rolDb.NombreRol} (Estado: {rolDb.Estado}).", idUsuarioActual);

                    TempData["Ok"] = "Rol actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        // 7. GET: Confirmar Eliminación
        [Authorize(Policy = "Permiso:Roles.Editar")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var rol = await _context.Roles
                .Include(r => r.RolPermisos)
                .FirstOrDefaultAsync(m => m.IdRol == id);

            if (rol == null) return NotFound();

            if (rol.NombreRol.ToLower() == "administrador")
            {
                TempData["Error"] = "El rol de Administrador no puede ser eliminado por seguridad del sistema.";
                return RedirectToAction(nameof(Index));
            }

            return View(rol);
        }

        // 8. POST: Delete (Inactivación Lógica)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permiso:Roles.Editar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol != null)
            {
                if (rol.NombreRol.ToLower() == "administrador")
                {
                    TempData["Error"] = "No se puede inactivar el rol de Administrador.";
                    return RedirectToAction(nameof(Index));
                }

                string nombreRolInactivado = rol.NombreRol;

                // BORRADO LÓGICO
                rol.Estado = "Inactivo";
                _context.Update(rol);
                await _context.SaveChangesAsync();

                // AUDITORÍA: Registrar inactivación
                int idUsuarioActual = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
                await _servicioAuditoria.RegistrarAccionAsync("INACTIVAR_ROL", $"ROL: {id}", $"Se inactivó el rol {nombreRolInactivado}.", idUsuarioActual);

                TempData["Ok"] = "Rol inactivado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}