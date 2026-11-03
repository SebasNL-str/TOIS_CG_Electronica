using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CG_Electronica.Web.Controllers;

[Authorize(Policy = "Permiso:Auditoria.Ver")]
public class AuditoriaController : Controller
{
    private readonly AppDbContext _context;

    public AuditoriaController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(AuditoriaFiltroViewModel filtro)
    {
        var query = _context.Auditorias
            .Include(a => a.Usuario)
            .AsQueryable();

        // 1. Filtro por Acción
        if (!string.IsNullOrEmpty(filtro.Accion))
        {
            query = query.Where(a => a.Accion == filtro.Accion);
        }

        // 2. Filtro por Módulo (basado en el prefijo de registroAfectado, ej: "PRODUCTO:")
        if (!string.IsNullOrEmpty(filtro.Modulo))
        {
            query = query.Where(a => a.RegistroAfectado.StartsWith(filtro.Modulo));
        }

        // 3. Filtro por Usuario Responsable
        if (filtro.IdUsuario.HasValue && filtro.IdUsuario.Value > 0)
        {
            query = query.Where(a => a.IdUsuario == filtro.IdUsuario.Value);
        }

        // 4. Filtro por Fecha Inicial
        if (filtro.FechaInicio.HasValue)
        {
            query = query.Where(a => a.FechaHora >= filtro.FechaInicio.Value.Date);
        }

        // 5. Filtro por Fecha Final (incluyendo el final del día)
        if (filtro.FechaFin.HasValue)
        {
            var fechaFinMax = filtro.FechaFin.Value.Date.AddDays(1).AddSeconds(-1);
            query = query.Where(a => a.FechaHora <= fechaFinMax);
        }

        // 6. Búsqueda por Texto Libre
        if (!string.IsNullOrEmpty(filtro.BusquedaTexto))
        {
            var texto = filtro.BusquedaTexto.Trim();
            query = query.Where(a => a.Accion.Contains(texto) ||
                                     a.RegistroAfectado.Contains(texto) ||
                                     a.Observacion.Contains(texto));
        }

        var resultados = await query
            .OrderByDescending(a => a.FechaHora)
            .ToListAsync();

        // Poblar listas desplegables para los filtros
        var accionesDisponibles = await _context.Auditorias
            .Select(a => a.Accion)
            .Distinct()
            .OrderBy(a => a)
            .ToListAsync();

        var modulosDisponibles = new[]
        {
            new { Key = "USUARIO", Value = "Usuarios" },
            new { Key = "ROL", Value = "Roles" },
            new { Key = "PRODUCTO", Value = "Productos" },
            new { Key = "SERVICIO", Value = "Servicios" },
            new { Key = "CATEGORIA", Value = "Categorías" },
            new { Key = "MARCA", Value = "Marcas" },
            new { Key = "CLIENTE", Value = "Clientes" },
            new { Key = "FACTURA", Value = "Facturación" },
            new { Key = "MOVIMIENTO", Value = "Inventario" }
        };

        var usuariosList = await _context.Usuarios
            .Where(u => u.Estado == "Activo")
            .OrderBy(u => u.NombreUsuario)
            .ToListAsync();

        filtro.AccionesList = new SelectList(accionesDisponibles, filtro.Accion);
        filtro.ModulosList = new SelectList(modulosDisponibles, "Key", "Value", filtro.Modulo);
        filtro.UsuariosList = new SelectList(usuariosList, "IdUsuario", "NombreUsuario", filtro.IdUsuario);

        ViewBag.Filtro = filtro;
        return View(resultados);
    }
}