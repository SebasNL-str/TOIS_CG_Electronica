using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        var producto = await _context.Productos.FindAsync(idProducto);
        if (producto == null)
            throw new InvalidOperationException("El producto especificado no existe.");

        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

        // Actualizar existencias y costo
        producto.Existencias += cantidad;
        producto.PrecioCompra = precioCompraUnitario;

        var movimiento = new MovimientoInventario
        {
            IdProducto = idProducto,
            TipoMovimiento = "Entrada",
            Motivo = motivo,
            Cantidad = cantidad,
            PrecioCompraUnitario = precioCompraUnitario,
            FechaHora = DateTime.Now,
            IdUsuario = idUsuario,
            Observacion = observacion
        };

        _context.MovimientosInventario.Add(movimiento);
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();

        // Auditoría
        await _servicioAuditoria.RegistrarAccionAsync(
            "ENTRADA_INVENTARIO",
            $"PRODUCTO: {idProducto}",
            $"Entrada de {cantidad} unidades por '{motivo}'. Obs: {observacion ?? "Sin observación"}",
            idUsuario
        );
    }

    public async Task RegistrarSalidaAsync(int idProducto, int cantidad, string motivo, int idUsuario, string? observacion = null)
    {
        var producto = await _context.Productos.FindAsync(idProducto);
        if (producto == null)
            throw new InvalidOperationException("El producto especificado no existe.");

        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

        if (producto.Existencias < cantidad)
            throw new InvalidOperationException($"Stock insuficiente. Existencias actuales: {producto.Existencias}, Cantidad solicitada: {cantidad}");

        // Actualizar existencias
        producto.Existencias -= cantidad;

        var movimiento = new MovimientoInventario
        {
            IdProducto = idProducto,
            TipoMovimiento = "Salida",
            Motivo = motivo,
            Cantidad = cantidad,
            PrecioCompraUnitario = producto.PrecioCompra,
            FechaHora = DateTime.Now,
            IdUsuario = idUsuario,
            Observacion = observacion
        };

        _context.MovimientosInventario.Add(movimiento);
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();

        // Auditoría
        await _servicioAuditoria.RegistrarAccionAsync(
            "SALIDA_INVENTARIO",
            $"PRODUCTO: {idProducto}",
            $"Salida de {cantidad} unidades por '{motivo}'. Obs: {observacion ?? "Sin observación"}",
            idUsuario
        );
    }

    public async Task RegistrarAjusteAsync(int idProducto, int nuevaCantidad, int idUsuario, string observacion)
    {
        var producto = await _context.Productos.FindAsync(idProducto);
        if (producto == null)
            throw new InvalidOperationException("El producto especificado no existe.");

        int diferencia = nuevaCantidad - producto.Existencias;
        if (diferencia == 0) return;

        string tipo = diferencia > 0 ? "Entrada" : "Salida";
        int cantidadAbs = Math.Abs(diferencia);

        producto.Existencias = nuevaCantidad;

        var movimiento = new MovimientoInventario
        {
            IdProducto = idProducto,
            TipoMovimiento = tipo,
            Motivo = "Ajuste de inventario",
            Cantidad = cantidadAbs,
            PrecioCompraUnitario = producto.PrecioCompra,
            FechaHora = DateTime.Now,
            IdUsuario = idUsuario,
            Observacion = observacion
        };

        _context.MovimientosInventario.Add(movimiento);
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();

        // Auditoría
        await _servicioAuditoria.RegistrarAccionAsync(
            "AJUSTE_INVENTARIO",
            $"PRODUCTO: {idProducto}",
            $"Ajuste manual de inventario. Nueva existencia: {nuevaCantidad}. Obs: {observacion}",
            idUsuario
        );
    }

    public async Task<List<Producto>> ObtenerProductosConStockMinimoAsync()
    {
        return await _context.Productos
            .Include(p => p.Marca)
            .Include(p => p.Categoria)
            .Where(p => p.Estado == "Activo" && p.Existencias <= p.ExistenciasMinimas)
            .ToListAsync();
    }
}