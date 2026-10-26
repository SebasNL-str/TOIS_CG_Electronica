using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;

namespace CG_Electronica.Web.Services;

public class ServicioAuditoria : IServicioAuditoria
{
    private readonly AppDbContext _context;

    public ServicioAuditoria(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAccionAsync(string accion, string registroAfectado, int idUsuario, string? observacion = null)
    {
        var auditoria = new Auditoria
        {
            Accion = accion,
            RegistroAfectado = registroAfectado,
            IdUsuario = idUsuario,
            Observacion = observacion,
            FechaHora = DateTime.Now
        };

        _context.Auditorias.Add(auditoria);
        await _context.SaveChangesAsync();
    }
}