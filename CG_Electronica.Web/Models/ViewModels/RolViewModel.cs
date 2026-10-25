using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CG_Electronica.Web.Models.ViewModels
{
    public class RolViewModel
    {
        [HiddenInput]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
        [Remote(action: "VerificarNombreRol", controller: "Roles", AdditionalFields = nameof(IdRol), HttpMethod = "GET")]
        [Display(Name = "Nombre del Rol")]
        public string NombreRol { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Máximo {1} caracteres.")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        // Lista para manejar los checkboxes en la vista
        public List<PermisoCheckboxViewModel> Permisos { get; set; } = new List<PermisoCheckboxViewModel>();
    }

    // Clase auxiliar para cada checkbox de permiso
    public class PermisoCheckboxViewModel
    {
        public int IdPermiso { get; set; }
        public string NombrePermiso { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Seleccionado { get; set; }
    }
}