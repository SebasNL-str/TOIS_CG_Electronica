using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CG_Electronica.Web.Data;

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
    }
}