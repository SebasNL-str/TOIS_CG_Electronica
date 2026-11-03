using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Services;

public interface IServicioVentas
{
    Task<Factura> RegistrarVentaAsync(VentaRequestDTO request, int idUsuario);
    Task AnularFacturaAsync(int idFactura, int idUsuario);
}

public sealed class ServicioVentas : IServicioVentas
{
    private const decimal TasaIva = 0.15m;
    private readonly AppDbContext _context;
    private readonly IServicioInventario _servicioInventario;
    private readonly IServicioAuditoria _servicioAuditoria;

    public ServicioVentas(AppDbContext context, IServicioInventario servicioInventario, IServicioAuditoria servicioAuditoria)
    { _context = context; _servicioInventario = servicioInventario; _servicioAuditoria = servicioAuditoria; }

    public async Task<Factura> RegistrarVentaAsync(VentaRequestDTO request, int idUsuario)
    {
        if (request.Detalles is null || request.Detalles.Count == 0) throw new ArgumentException("La factura debe contener al menos un detalle.");
        if (request.Detalles.Any(d => d.Cantidad <= 0)) throw new ArgumentException("Todas las cantidades deben ser mayores que cero.");
        if (request.Detalles.Select(d => d.IdItem).Distinct().Count() != request.Detalles.Count) throw new ArgumentException("No se puede repetir un artículo en la misma factura.");
        if (string.IsNullOrWhiteSpace(request.NumeroFactura) || request.NumeroFactura.Length > 50) throw new ArgumentException("El número de factura es obligatorio y no puede superar 50 caracteres.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var numero = request.NumeroFactura.Trim();
            if (await _context.Facturas.AnyAsync(f => f.NumeroFactura == numero)) throw new InvalidOperationException("Ya existe una factura con ese número.");
            var cliente = await _context.Clientes.SingleOrDefaultAsync(c => c.IdCliente == request.IdCliente && c.Estado == "Activo") ?? throw new KeyNotFoundException("El cliente seleccionado no existe o está inactivo.");
            var ids = request.Detalles.Select(d => d.IdItem).ToArray();
            var items = await _context.ItemsFacturables.Where(i => ids.Contains(i.IdItem)).ToListAsync();
            if (items.Count != ids.Length) throw new KeyNotFoundException("Uno o más artículos no existen.");

            var detalles = new List<DetalleFactura>();
            decimal subtotal = 0m;
            foreach (var dto in request.Detalles)
            {
                var item = items.Single(i => i.IdItem == dto.IdItem);
                if (item.Estado != "Activo") throw new InvalidOperationException($"El artículo {dto.IdItem} no está activo.");
                if (item is Producto producto && producto.Existencias < dto.Cantidad) throw new InvalidOperationException($"Stock insuficiente para '{producto.Descripcion}'. Disponible: {producto.Existencias}, solicitado: {dto.Cantidad}.");
                var precio = Math.Round(item.PrecioVenta, 2);
                subtotal += Math.Round(precio * dto.Cantidad, 2);
                detalles.Add(new DetalleFactura { IdItem = item.IdItem, Cantidad = dto.Cantidad, PrecioUnitarioHistorico = precio });
            }
            subtotal = Math.Round(subtotal, 2);
            var factura = new Factura
            {
                NumeroFactura = numero, Fecha = DateTime.Now, Estado = "Emitida", Subtotal = subtotal,
                Iva = Math.Round(subtotal * TasaIva, 2), Total = Math.Round(subtotal * (1 + TasaIva), 2),
                IdCliente = cliente.IdCliente, IdUsuario = idUsuario, NombreClienteHistorico = cliente.NombreCliente,
                TelefonoClienteHistorico = cliente.Telefono, CedulaClienteHistorica = cliente.Cedula, Detalles = detalles
            };
            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();
            foreach (var detalle in detalles)
            {
                if (items.Single(i => i.IdItem == detalle.IdItem) is Producto producto)
                    await _servicioInventario.RegistrarSalidaAsync(producto.IdItem, detalle.Cantidad, "Salida por venta", idUsuario, $"Factura N°: {factura.NumeroFactura}");
            }
            
            // Corrección en RegistrarAccionAsync (Emitir Venta): Orden corregido -> (Acción, Entidad, Descripción, IdUsuario)
            await _servicioAuditoria.RegistrarAccionAsync(
                "EMITIR_FACTURA", 
                $"FACTURA: {factura.NumeroFactura}", 
                $"Venta por ${factura.Total:N2}", 
                idUsuario
            );

            await transaction.CommitAsync();
            return factura;
        }
        catch { await transaction.RollbackAsync(); throw; }
    }

    public async Task AnularFacturaAsync(int idFactura, int idUsuario)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var factura = await _context.Facturas.Include(f => f.Detalles).ThenInclude(d => d.Item).SingleOrDefaultAsync(f => f.IdFactura == idFactura) ?? throw new KeyNotFoundException("Factura no encontrada.");
            if (factura.Estado == "Anulada") throw new InvalidOperationException("La factura ya está anulada.");
            factura.Estado = "Anulada";
            factura.FechaAnulacion = DateTime.Now;
            foreach (var detalle in factura.Detalles)
            {
                if (detalle.Item is Producto producto)
                    await _servicioInventario.RegistrarEntradaAsync(producto.IdItem, detalle.Cantidad, producto.PrecioCompra, "Entrada por devolución de cliente", idUsuario, $"Anulación factura N°: {factura.NumeroFactura}");
            }
            await _context.SaveChangesAsync();
            
            // Corrección en RegistrarAccionAsync (Anular Venta): Se agregaron los 4 argumentos obligatorios (Acción, Entidad, Descripción, IdUsuario)
            await _servicioAuditoria.RegistrarAccionAsync(
                "ANULAR_FACTURA", 
                $"FACTURA: {factura.NumeroFactura}", 
                $"Se anuló la factura N°: {factura.NumeroFactura}", 
                idUsuario
            );

            await transaction.CommitAsync();
        }
        catch { await transaction.RollbackAsync(); throw; }
    }
}