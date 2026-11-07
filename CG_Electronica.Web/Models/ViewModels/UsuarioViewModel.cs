using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CG_Electronica.Web.Models.ViewModels
{
    public class UsuarioViewModel
    {
        [HiddenInput]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
        [Remote(action: "VerificarNombreUsuario", controller: "Usuarios", AdditionalFields = nameof(IdUsuario), HttpMethod = "GET")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = null!;

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string? Clave { get; set; } // Opcional en edición

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        // NUEVO: Estado
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Activo";
    }
}