using System.Threading.Tasks;

namespace CG_Electronica.Web.Services;

public interface IServicioAuditoria
{
    Task RegistrarAsync(string accion, string registroAfectado, string observacion, int idUsuario);
    Task RegistrarAccionAsync(string accion, string registroAfectado, string observacion, int idUsuario);
}