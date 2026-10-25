using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CG_Electronica.Web.Models.ViewModels
{
    public class UsuarioViewModel
    {
        [HiddenInput]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo {1} caracteres.")]
        [Remote(action: "VerificarNombreUsuario", controller: "Usuarios", AdditionalFields = nameof(IdUsuario), HttpMethod = "GET")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; } = null!;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = null!;

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string? Clave { get; set; } // Es nullable porque en la edición será opcional

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        public string Estado { get; set; } = "Activo";
    }
}