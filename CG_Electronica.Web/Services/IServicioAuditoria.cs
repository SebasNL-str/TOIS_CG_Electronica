namespace CG_Electronica.Web.Services;

public interface IServicioAuditoria
{
    Task RegistrarAccionAsync(string accion, string registroAfectado, int idUsuario, string? observacion = null);
}