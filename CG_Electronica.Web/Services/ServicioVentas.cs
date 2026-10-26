/*using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Models.ViewModels;

namespace CG_Electronica.Web.Services
{
    public interface IServicioVentas 
    {
        Task<Factura> RegistrarVentaAsync(VentaRequestDTO request, int idUsuario);
        Task<bool> AnularFacturaAsync(int idFactura, int idUsuario);
    }

    public class ServicioVentas : IServicioVentas
    {
        private readonly AppDbContext _context;
        //private readonly IServicioInventario _servicioInventario; // Interface del Integrante 2
        //private readonly IServicioAuditoria _servicioAuditoria;   // Interface del Integrante 2

        /*public ServicioVentas(AppDbContext context, IServicioInventario servicioInventario, IServicioAuditoria servicioAuditoria)
        {
            _context = context;
            _servicioInventario = servicioInventario;
            _servicioAuditoria = servicioAuditoria;
        }
        
        public async Task<Factura> RegistrarVentaAsync(VentaRequestDTO request, int idUsuario)
        {
            // Valida que la factura contenga al menos un detalle[cite: 6]
            if (request.Detalles == null || !request.Detalles.Any())
                throw new ArgumentException("La factura debe contener al menos un detalle.");

            // La creación de la factura, detalles, salida de inventario y auditoría deben ocurrir dentro de una única transacción SQL[cite: 6, 7]
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cliente = await _context.Clientes.FindAsync(request.IdCliente) 
                              ?? throw new Exception("Cliente no encontrado.");

                // Captura los datos históricos del cliente[cite: 6]
                var nuevaFactura = new Factura
                {
                    NumeroFactura = request.NumeroFactura,
                    Fecha = DateTime.Now,
                    Estado = "Emitida",
                    NombreClienteHistorico = cliente.NombreCliente,
                    TelefonoClienteHistorico = cliente.Telefono,
                    CedulaClienteHistorica = cliente.Cedula,
                    IdCliente = cliente.IdCliente,
                    IdUsuario = idUsuario,
                    Subtotal = 0,
                    Iva = 0,
                    Total = 0
                };

                _context.Facturas.Add(nuevaFactura);
                await _context.SaveChangesAsync(); 

                decimal subtotalAcumulado = 0;

                foreach (var det in request.Detalles)
                {
                    var item = await _context.ItemFacturables
                                             .FirstOrDefaultAsync(i => i.IdItem == det.IdItem) 
                                             ?? throw new Exception($"Item {det.IdItem} no existe.");

                    // Fija el precioUnitarioHistorico[cite: 6]
                    var nuevoDetalle = new DetalleFactura
                    {
                        IdFactura = nuevaFactura.IdFactura,
                        IdItem = item.IdItem,
                        Cantidad = det.Cantidad,
                        PrecioUnitarioHistorico = item.PrecioVenta
                    };
                    
                    // Cálculos monetarios usando Math.Round(val, 2)[cite: 6]
                    subtotalAcumulado += Math.Round(det.Cantidad * item.PrecioVenta, 2);
                    _context.DetallesFactura.Add(nuevoDetalle);

                    // Revalida obligatoriamente en el servidor verificando si es Producto[cite: 6]
                    if (item is Producto producto)
                    {
                        if (producto.Existencias < det.Cantidad)
                            throw new InvalidOperationException($"Stock insuficiente para el producto {producto.Descripcion}.");

                        // Invoca a ServicioInventario para generar movimientos de salida para cada producto[cite: 6]
                        await _servicioInventario.RegistrarSalidaInventario(
                            producto.IdItem, det.Cantidad, "Salida por venta", idUsuario);
                    }
                    // Si la línea de detalle pertenece a un SERVICIO, no solicita salida de inventario[cite: 6]
                }

                nuevaFactura.Subtotal = subtotalAcumulado;
                nuevaFactura.Iva = Math.Round(subtotalAcumulado * 0.15m, 2); 
                nuevaFactura.Total = nuevaFactura.Subtotal + nuevaFactura.Iva;

                await _context.SaveChangesAsync();

                // Invoca a ServicioAuditoria para registrar la operación de venta[cite: 6]
                await _servicioAuditoria.RegistrarEvento(
                    "Creación de Factura", $"Factura ID: {nuevaFactura.IdFactura}", idUsuario);

                // Confirma la transacción[cite: 6]
                await transaction.CommitAsync();

                return nuevaFactura;
            }
            catch (Exception)
            {
                // Si ocurre cualquier falla, revierte todo[cite: 6]
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> AnularFacturaAsync(int idFactura, int idUsuario)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var factura = await _context.Facturas
                    .Include(f => f.Detalles)
                    .ThenInclude(d => d.Item)
                    .FirstOrDefaultAsync(f => f.IdFactura == idFactura) 
                    ?? throw new Exception("Factura no encontrada.");

                if (factura.Estado == "Anulada") throw new InvalidOperationException("Ya está anulada.");

                // Cambia el estado de la factura a Anulada y asigna fechaAnulacion[cite: 6]
                factura.Estado = "Anulada";
                factura.FechaAnulacion = DateTime.Now;

                // Genera movimientos compensatorios de entrada en inventario para los productos incluidos[cite: 6]
                foreach (var det in factura.Detalles)
                {
                    if (det.Item is Producto producto)
                    {
                        await _servicioInventario.RegistrarEntradaInventario(
                            producto.IdItem, det.Cantidad, "Entrada por devolución de cliente", idUsuario);
                    }
                }

                await _context.SaveChangesAsync();
                
                // Genera un registro en la tabla de AUDITORIA[cite: 6]
                await _servicioAuditoria.RegistrarEvento(
                    "Anulación de Factura", $"Factura ID: {idFactura}", idUsuario);
                
                // Ejecuta todo de manera transaccional[cite: 6]
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
*/