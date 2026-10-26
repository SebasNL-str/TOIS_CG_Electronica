using CG_Electronica.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers;

[Authorize]
public class AuditoriaController : Controller
{
    private readonly AppDbContext _context;

    public AuditoriaController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var auditorias = await _context.Auditorias
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.FechaHora)
            .Take(100) // Últimos 100 registros por rendimiento
            .ToListAsync();

        return View(auditorias);
    }
}