using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CG_Electronica.Web.Models.ViewModels
{
    public class UsuarioViewModel
    {
        [HiddenInput]
        public int IdUsuario { get; set; }

        // 1. Nombre Completo: Entre 3 y 150 caracteres, solo letras y espacios
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; } = null!;

        // 2. Nombre de Usuario: Entre 3 y 100 caracteres, sin espacios, letras, números, puntos y guiones
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre {2} y {1} caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9._]+$", ErrorMessage = "El usuario no puede tener espacios ni caracteres especiales (solo letras, números, '.' o '_').")]
        [Remote(action: "VerificarNombreUsuario", controller: "Usuarios", AdditionalFields = nameof(IdUsuario), HttpMethod = "GET")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; } = null!;

        // 3. Contraseña: Mínimo 6 caracteres (opcional en edición, obligatoria en creación)
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Clave { get; set; }

        // 4. Rol: Obligatorio y debe ser un ID válido mayor a 0
        [Required(ErrorMessage = "Debe seleccionar un rol para el usuario.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido de la lista.")]
        [Display(Name = "Rol Asignado")]
        public int IdRol { get; set; }

        // 5. Estado: Obligatorio
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Activo";
    }
}