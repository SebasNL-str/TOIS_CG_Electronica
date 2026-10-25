using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: Listado de Roles
        [Authorize(Policy = "Permiso:Roles.Ver")]
        public async Task<IActionResult> Index()
        {
            // Traemos los roles incluyendo sus permisos asignados para contar cuántos tienen
            var roles = await _context.Roles
                .Include(r => r.RolPermisos)
                .ToListAsync();

            return View(roles);
        }

        // 2. GET: Validación Remota para Nombre de Rol único
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

            // Cargamos todos los permisos disponibles en la base de datos como no seleccionados
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
                        Descripcion = model.Descripcion
                    };

                    _context.Roles.Add(nuevoRol);
                    await _context.SaveChangesAsync(); // Guardamos para que genere el IdRol

                    // Filtramos los permisos que el usuario marcó con el check
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

            // Buscamos el rol INCLUYENDO sus permisos actuales
            var rolDb = await _context.Roles
                .Include(r => r.RolPermisos)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (rolDb == null) return NotFound();

            var model = new RolViewModel
            {
                IdRol = rolDb.IdRol,
                NombreRol = rolDb.NombreRol,
                Descripcion = rolDb.Descripcion
            };

            var permisosDb = await _context.Permisos.ToListAsync();

            foreach (var permiso in permisosDb)
            {
                // Si el rol ya tiene este permiso en la BD, lo marcamos como seleccionado (true)
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

                    // Evitamos que le cambien el nombre al rol "Administrador" (medida de seguridad)
                    if (rolDb.NombreRol.ToLower() == "administrador" && model.NombreRol.ToLower() != "administrador")
                    {
                        ModelState.AddModelError("NombreRol", "No se puede cambiar el nombre al rol principal de Administrador.");
                        return View(model);
                    }

                    rolDb.NombreRol = model.NombreRol;
                    rolDb.Descripcion = model.Descripcion;

                    // TRUCO PARA ACTUALIZAR RELACIÓN N:N -> Borramos todos los permisos viejos y agregamos los nuevos marcados
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

                    TempData["Ok"] = "Rol actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }
    }
}