using CG_Electronica.Web.Models;

namespace CG_Electronica.Web.Services;

public interface IServicioInventario
{
    Task RegistrarEntradaAsync(int idProducto, int cantidad, decimal precioCompraUnitario, string motivo, int idUsuario, string? observacion = null);
    Task RegistrarSalidaAsync(int idProducto, int cantidad, string motivo, int idUsuario, string? observacion = null);
    Task RegistrarAjusteAsync(int idProducto, int nuevaExistencia, int idUsuario, string observacion);
    Task<List<Producto>> ObtenerProductosConStockMinimoAsync();
}