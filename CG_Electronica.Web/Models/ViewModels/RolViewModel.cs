using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CG_Electronica.Web.Models.ViewModels
{
    public class RolViewModel
    {
        [HiddenInput]
        public int IdRol { get; set; }

        // 1. Nombre del Rol: Entre 3 y 100 caracteres, solo letras y espacios (ej. "Cajero", "Supervisor de Ventas")
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre del rol debe tener entre {2} y {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre del rol solo puede contener letras y espacios.")]
        [Remote(action: "VerificarNombreRol", controller: "Roles", AdditionalFields = nameof(IdRol), HttpMethod = "GET")]
        [Display(Name = "Nombre del Rol")]
        public string NombreRol { get; set; } = null!;

        // 2. Descripción: Opcional, pero con límite estricto de 255 caracteres para evitar desbordes en SQL Server
        [StringLength(255, ErrorMessage = "La descripción no puede exceder los {1} caracteres.")]
        [Display(Name = "Descripción del Rol")]
        public string? Descripcion { get; set; }

        // 3. Estado: Obligatorio
        [Required(ErrorMessage = "El estado del rol es obligatorio.")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Activo";

        // Colección para los checkboxes de permisos
        public List<PermisoCheckboxViewModel> Permisos { get; set; } = new List<PermisoCheckboxViewModel>();
    }

    public class PermisoCheckboxViewModel
    {
        public int IdPermiso { get; set; }
        public string NombrePermiso { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Seleccionado { get; set; }
    }
}