namespace CG_Electronica.Web.Models;

public class Auditoria
{
    public int IdAuditoria { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string RegistroAfectado { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public string? Observacion { get; set; }

    public int IdUsuario { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
}