using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CG_Electronica.Web.Models.ViewModels;

public class ClienteViewModel
{
    [HiddenInput]
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(150)]
    [Display(Name = "Nombre")]
    public string NombreCliente { get; set; } = string.Empty;

    [StringLength(30)]
    [Phone(ErrorMessage = "Formato de teléfono inválido.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [StringLength(50)]
    [Display(Name = "Cédula")]
    // Validación REMOTA: consulta al servidor sin salir del formulario (Bootstrap la muestra igual)
    [Remote("VerificarCedula", "Clientes", AdditionalFields = nameof(IdCliente),
            ErrorMessage = "Ya existe un cliente registrado con esta cédula.")]
    public string Cedula { get; set; } = string.Empty;
}