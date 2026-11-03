using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CG_Electronica.Web.Models.ViewModels;

public class AuditoriaFiltroViewModel
{
    public string? Accion { get; set; }
    public string? Modulo { get; set; }
    public int? IdUsuario { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? BusquedaTexto { get; set; }

    public SelectList? AccionesList { get; set; }
    public SelectList? ModulosList { get; set; }
    public SelectList? UsuariosList { get; set; }
}