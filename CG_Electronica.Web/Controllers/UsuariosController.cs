using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Para PasswordHasher
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Para los SelectList
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // Listado de usuarios, requiere permiso de Ver
        [Authorize(Policy = "Permiso:Usuarios.Ver")]
        public async Task<IActionResult> Index()
        {
            // Hacemos Include del Rol para poder mostrar su nombre en la tabla
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .ToListAsync();

            return View(usuarios);
        }

        // 1. Validación Remota (GET) para verificar que el usuario no exista
        [HttpGet]
        public async Task<IActionResult> VerificarNombreUsuario(string nombreUsuario, int idUsuario)
        {
            var existe = await _context.Usuarios
                .AnyAsync(u => u.NombreUsuario.ToLower() == nombreUsuario.ToLower() && u.IdUsuario != idUsuario);

            if (existe)
                return Json($"El nombre de usuario '{nombreUsuario}' ya está registrado.");

            return Json(true);
        }

        // 2. GET: Formulario de Creación
        [Authorize(Policy = "Permiso:Usuarios.Editar")]
        public async Task<IActionResult> Create()
        {
            // Cargamos los roles disponibles para el <select>
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol");
            return View(new Models.ViewModels.UsuarioViewModel());
        }

        // 3. POST: Guardar el nuevo usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permiso:Usuarios.Editar")]
        public async Task<IActionResult> Create(Models.ViewModels.UsuarioViewModel model)
        {
            // Verificamos si escribió contraseña (en la creación es obligatoria, aunque en el ViewModel la pusimos opcional para la edición)
            if (string.IsNullOrEmpty(model.Clave))
            {
                ModelState.AddModelError("Clave", "La contraseña es obligatoria para un nuevo usuario.");
            }

            if (ModelState.IsValid)
            {
                // Doble validación en el servidor de NombreUsuario único
                bool existe = await _context.Usuarios
                    .AnyAsync(u => u.NombreUsuario.ToLower() == model.NombreUsuario.ToLower());

                if (existe)
                {
                    ModelState.AddModelError("NombreUsuario", "El nombre de usuario ya está registrado.");
                }
                else
                {
                    var nuevoUsuario = new Usuario
                    {
                        Nombre = model.Nombre,
                        NombreUsuario = model.NombreUsuario,
                        IdRol = model.IdRol
                    };

                    // HASHEO DE CONTRASEÑA (Nunca se guarda en texto plano)
                    var hasher = new PasswordHasher<Usuario>();
                    nuevoUsuario.Contrasena = hasher.HashPassword(nuevoUsuario, model.Clave!);

                    _context.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();

                    TempData["Ok"] = "Usuario creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Si hubo error, recargamos el select de roles
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol", model.IdRol);
            return View(model);
        }

        // GET: Edit
        [Authorize(Policy = "Permiso:Usuarios.Editar")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            var model = new Models.ViewModels.UsuarioViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                NombreUsuario = usuario.NombreUsuario,
                IdRol = usuario.IdRol,
                Estado = usuario.Estado // <--- Mapeamos el estado
            };

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol", model.IdRol);
            return View(model);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permiso:Usuarios.Editar")]
        public async Task<IActionResult> Edit(int id, Models.ViewModels.UsuarioViewModel model)
        {
            if (id != model.IdUsuario) return NotFound();

            if (ModelState.IsValid)
            {
                bool existe = await _context.Usuarios
                    .AnyAsync(u => u.NombreUsuario.ToLower() == model.NombreUsuario.ToLower() && u.IdUsuario != id);

                if (existe)
                {
                    ModelState.AddModelError("NombreUsuario", "El nombre de usuario ya está registrado por otro.");
                }
                else
                {
                    var usuarioDb = await _context.Usuarios.FindAsync(id);
                    if (usuarioDb == null) return NotFound();

                    // Protección: No permitir inactivar al admin principal
                    if (usuarioDb.NombreUsuario.ToLower() == "admin" && model.Estado != "Activo")
                    {
                        ModelState.AddModelError("Estado", "El usuario administrador principal no puede ser inactivado.");
                        ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol", model.IdRol);
                        return View(model);
                    }

                    usuarioDb.Nombre = model.Nombre;
                    usuarioDb.NombreUsuario = model.NombreUsuario;
                    usuarioDb.IdRol = model.IdRol;
                    usuarioDb.Estado = model.Estado; // <--- Guardamos el estado

                    if (!string.IsNullOrEmpty(model.Clave))
                    {
                        var hasher = new PasswordHasher<Usuario>();
                        usuarioDb.Contrasena = hasher.HashPassword(usuarioDb, model.Clave);
                    }

                    _context.Update(usuarioDb);
                    await _context.SaveChangesAsync();

                    TempData["Ok"] = "Usuario actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol", model.IdRol);
            return View(model);
        }


        // 6. GET: Confirmar Eliminación
        [Authorize(Policy = "Permiso:Usuarios.Editar")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null) return NotFound();

            // Protección extra por si se llega por URL
            if (usuario.NombreUsuario.ToLower() == "admin")
            {
                TempData["Error"] = "No está permitido eliminar la cuenta del administrador principal.";
                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        // POST: Delete (BORRADO LÓGICO / INACTIVACIÓN)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permiso:Usuarios.Editar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                if (usuario.NombreUsuario.ToLower() == "admin")
                {
                    TempData["Error"] = "No se puede inactivar al administrador principal.";
                    return RedirectToAction(nameof(Index));
                }

                // BORRADO LÓGICO: Cambiamos estado a Inactivo
                usuario.Estado = "Inactivo";

                _context.Update(usuario);
                await _context.SaveChangesAsync();

                TempData["Ok"] = "Usuario inactivado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}