using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;

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
    }
}