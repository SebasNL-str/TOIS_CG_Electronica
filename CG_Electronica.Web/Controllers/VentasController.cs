/*using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CG_Electronica.Web.Services; // Ajusta según tu carpeta de servicios
using CG_Electronica.Web.Models.ViewModels;// Ajusta según tu carpeta de ViewModels

namespace CG_Electronica.Web.Controllers
{
    // OBLIGATORIO: Protege todo el controlador para que solo entren usuarios logueados
    [Authorize]
    public class VentasController : Controller
    {
        private readonly IServicioVentas _servicioVentas;

        public VentasController(IServicioVentas servicioVentas)
        {
            _servicioVentas = servicioVentas;
        }

        // GET: /Ventas/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /Ventas/RegistrarVenta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarVenta([FromBody] VentaRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // === LECTURA DEL USUARIO ACTIVO ===
                // Buscamos exactamente el ClaimTypes.NameIdentifier que el Integrante 1 guardó en el AccountController
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Intentamos parsear el string del Claim a int (ya que Usuario.IdUsuario es int)
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int idUsuario))
                {
                    return Unauthorized(new { success = false, mensaje = "Sesión inválida o expirada." });
                }

                // Ahora sí, inyectamos el idUsuario 100% real al servicio transaccional
                var factura = await _servicioVentas.RegistrarVentaAsync(request, idUsuario);

                return Json(new { success = true, facturaId = factura.IdFactura, mensaje = "Venta registrada con éxito." });
            }
            catch (Exception ex)
            {
                // Manejo del rollback/error: le devolvemos al frontend qué falló (ej. "Stock insuficiente")
                return Json(new { success = false, mensaje = ex.Message });
            }
        }
    }
}
*/