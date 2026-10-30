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

        // Reutiliza la transacción existente si la hay; de lo contrario, crea una nueva
        var existingTransaction = _context.Database.CurrentTransaction;
        var transaction = existingTransaction == null ? await _context.Database.BeginTransactionAsync() : null;
        try
        {
            var producto = await _context.Productos.FindAsync(idProducto);
            if (producto == null) throw new KeyNotFoundException("El producto especificado no existe.");

            producto.Existencias += cantidad;
            producto.PrecioCompra = precioCompraUnitario;

            var movimiento = new MovimientoInventario
            {
                TipoMovimiento = "Entrada",
                Motivo = motivo,
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

            if (transaction != null) await transaction.CommitAsync();
        }
        catch
        {
            if (transaction != null) await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    public async Task RegistrarSalidaAsync(int idProducto, int cantidad, string motivo, int idUsuario, string? observacion = null)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(cantidad));

        // Reutiliza la transacción existente si la hay; de lo contrario, crea una nueva
        var existingTransaction = _context.Database.CurrentTransaction;
        var transaction = existingTransaction == null ? await _context.Database.BeginTransactionAsync() : null;
        try
        {
            var producto = await _context.Productos.FindAsync(idProducto);
            if (producto == null) throw new KeyNotFoundException("El producto especificado no existe.");

            // Validación estricta de stock disponible
            if (producto.Existencias < cantidad)
            {
                throw new InvalidOperationException($"Stock insuficiente para '{producto.Descripcion}'. Stock actual: {producto.Existencias}, Cantidad solicitada: {cantidad}.");
            }

            producto.Existencias -= cantidad;

            var movimiento = new MovimientoInventario
            {
                TipoMovimiento = "Salida",
                Motivo = motivo,
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

            if (transaction != null) await transaction.CommitAsync();
        }
        catch
        {
            if (transaction != null) await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    public async Task RegistrarAjusteAsync(int idProducto, int nuevaExistencia, int idUsuario, string observacion)
    {
        if (nuevaExistencia < 0) throw new ArgumentException("Las existencias no pueden ser negativas.", nameof(nuevaExistencia));

        // Reutiliza la transacción existente si la hay; de lo contrario, crea una nueva
        var existingTransaction = _context.Database.CurrentTransaction;
        var transaction = existingTransaction == null ? await _context.Database.BeginTransactionAsync() : null;
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

            if (transaction != null) await transaction.CommitAsync();
        }
        catch
        {
            if (transaction != null) await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    public async Task<List<Producto>> ObtenerProductosConStockMinimoAsync()
    {
        return await _context.Productos
        .Where(p => p.Existencias <= p.ExistenciasMinimas && p.Estado == "Activo")
        .ToListAsync();
    }
}
