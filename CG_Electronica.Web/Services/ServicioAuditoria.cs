using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using System;
using System.Threading.Tasks;

namespace CG_Electronica.Web.Services;

public class ServicioAuditoria : IServicioAuditoria
{
    private readonly AppDbContext _context;

    public ServicioAuditoria(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(string accion, string registroAfectado, string observacion, int idUsuario)
    {
        await RegistrarAccionAsync(accion, registroAfectado, observacion, idUsuario);
    }

    public async Task RegistrarAccionAsync(string accion, string registroAfectado, string observacion, int idUsuario)
    {
        try
        {
            var auditoria = new Auditoria
            {
                Accion = accion,
                RegistroAfectado = registroAfectado,
                FechaHora = DateTime.Now,
                Observacion = observacion,
                IdUsuario = idUsuario
            };

            _context.Auditorias.Add(auditoria);
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Protección transaccional para evitar interrumpir la operación principal si ocurre un fallo de auditoría
        }
    }
}