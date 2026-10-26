using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Services;

public class ServicioInventario : IServicioInventario
{
    private readonly AppDbContext _context;
    private readonly IServicioAuditoria _servicioAuditoria;

    public ServicioInventario(AppDbContext context, IServicioAuditoria servicioAuditoria)
    {
        _context = context;
        _servicioAuditoria = servicioAuditoria;
    }

    public async Task RegistrarEntradaAsync(int idProducto, int cantidad, decimal precioCompraUnitario, string motivo, int idUsuario, string? observacion = null)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(cantidad));

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var producto = await _context.Productos.FindAsync(idProducto);
            if (producto == null) throw new KeyNotFoundException("El producto especificado no existe.");

            // Actualizar stock y último costo de compra
            producto.Existencias += cantidad;
            producto.PrecioCompra = precioCompraUnitario;

            var movimiento = new MovimientoInventario
            {
                TipoMovimiento = "Entrada",
                Motivo = motivo, // Ej: 'Entrada por compra', 'Entrada por devolución de cliente'
                Cantidad = cantidad,
                PrecioCompraUnitario = precioCompraUnitario,
                IdProducto = idProducto,
                IdUsuario = idUsuario,
                Observacion = observacion,
                FechaHora = DateTime.Now
            };

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();

            await _servicioAuditoria.RegistrarAccionAsync(
                "ENTRADA_INVENTARIO",
                $"PRODUCTO: {idProducto}, Cantidad: {cantidad}",
                idUsuario,
                observacion ?? $"Entrada por {motivo}"
            );

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RegistrarSalidaAsync(int idProducto, int cantidad, string motivo, int idUsuario, string? observacion = null)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(cantidad));

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var producto = await _context.Productos.FindAsync(idProducto);
            if (producto == null) throw new KeyNotFoundException("El producto especificado no existe.");

            // Validación estricta de stock disponible (Bloquea venta si stock < cantidad solicitada)
            if (producto.Existencias < cantidad)
            {
                throw new InvalidOperationException($"Stock insuficiente. Stock actual: {producto.Existencias}, Cantidad solicitada: {cantidad}.");
            }

            producto.Existencias -= cantidad;

            var movimiento = new MovimientoInventario
            {
                TipoMovimiento = "Salida",
                Motivo = motivo, // Ej: 'Salida por venta', 'Salida por producto dañado'
                Cantidad = cantidad,
                PrecioCompraUnitario = producto.PrecioCompra,
                IdProducto = idProducto,
                IdUsuario = idUsuario,
                Observacion = observacion,
                FechaHora = DateTime.Now
            };

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();

            await _servicioAuditoria.RegistrarAccionAsync(
                "SALIDA_INVENTARIO",
                $"PRODUCTO: {idProducto}, Cantidad: {cantidad}",
                idUsuario,
                observacion ?? $"Salida por {motivo}"
            );

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RegistrarAjusteAsync(int idProducto, int nuevaExistencia, int idUsuario, string observacion)
    {
        if (nuevaExistencia < 0) throw new ArgumentException("Las existencias no pueden ser negativas.", nameof(nuevaExistencia));

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var producto = await _context.Productos.FindAsync(idProducto);
            if (producto == null) throw new KeyNotFoundException("El producto especificado no existe.");

            int diferencia = nuevaExistencia - producto.Existencias;
            if (diferencia == 0) return;

            string tipoMovimiento = diferencia > 0 ? "Entrada" : "Salida";
            int cantidadMovimiento = Math.Abs(diferencia);

            producto.Existencias = nuevaExistencia;

            var movimiento = new MovimientoInventario
            {
                TipoMovimiento = tipoMovimiento,
                Motivo = "Ajuste de inventario",
                Cantidad = cantidadMovimiento,
                PrecioCompraUnitario = producto.PrecioCompra,
                IdProducto = idProducto,
                IdUsuario = idUsuario,
                Observacion = observacion,
                FechaHora = DateTime.Now
            };

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();

            await _servicioAuditoria.RegistrarAccionAsync(
                "AJUSTE_INVENTARIO",
                $"PRODUCTO: {idProducto}, Ajuste a: {nuevaExistencia}",
                idUsuario,
                observacion
            );

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Producto>> ObtenerProductosConStockMinimoAsync()
    {
        return await _context.Productos
            .Where(p => p.Existencias <= p.ExistenciasMinimas && p.Estado == "Activo")
            .ToListAsync();
    }
}