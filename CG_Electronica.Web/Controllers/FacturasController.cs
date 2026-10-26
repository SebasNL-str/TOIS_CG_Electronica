using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CG_Electronica.Web.Models.ViewModels;

namespace CG_Electronica.Web.Controllers;

[Authorize]
public class FacturasController : Controller
{
    private readonly AppDbContext _context;
    private readonly IServicioInventario _servicioInventario;
    private readonly IServicioAuditoria _servicioAuditoria;
    private const decimal TasaIva = 0.15m; // IVA fijo del 15%

    public FacturasController(AppDbContext context, IServicioInventario servicioInventario, IServicioAuditoria servicioAuditoria)
    {
        _context = context;
        _servicioInventario = servicioInventario;
        _servicioAuditoria = servicioAuditoria;
    }

    public async Task<IActionResult> Index()
    {
        var facturas = await _context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Usuario)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync();

        return View(facturas);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Clientes = new SelectList(_context.Clientes.Where(c => c.Estado == "Activo"), "IdCliente", "NombreCliente");
        // Solo productos activos con stock disponible para la venta
        ViewBag.Productos = new SelectList(_context.Productos.Where(p => p.Estado == "Activo" && p.Existencias > 0), "IdItem", "Descripcion");
        
        // Generar número de factura automático secuencial o basado en ticks/fecha
        ViewModelFacturaCreate model = new()
        {
            NumeroFactura = $"FAC-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ViewModelFacturaCreate model, List<ItemFacturaDto> items)
    {
        if (items == null || !items.Any())
        {
            ModelState.AddModelError("", "Debe agregar al menos un ítem a la factura.");
            CargarListasSelect();
            return View(model);
        }

        int idUsuario = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cliente = await _context.Clientes.FindAsync(model.IdCliente);
            if (cliente == null) throw new KeyNotFoundException("El cliente seleccionado no existe.");

            decimal subtotal = 0;
            var detallesFactura = new List<DetalleFactura>();

            // 1. Validar y procesar cada ítem del detalle
            foreach (var itemDto in items)
            {
                var itemFacturable = await _context.ItemsFacturables.FindAsync(itemDto.IdItem);
                if (itemFacturable == null || itemFacturable.Estado != "Activo")
                    throw new InvalidOperationException($"El ítem con ID {itemDto.IdItem} no existe o no está activo.");

                // Si es un producto, validar stock estrictamente
                if (itemFacturable is Producto producto)
                {
                    if (producto.Existencias < itemDto.Cantidad)
                    {
                        throw new InvalidOperationException($"Stock insuficiente para el producto '{producto.Descripcion}'. Disponible: {producto.Existencias}, Solicitado: {itemDto.Cantidad}.");
                    }
                }

                decimal precioUnitario = itemFacturable.PrecioVenta;
                decimal totalLinea = precioUnitario * itemDto.Cantidad;
                subtotal += totalLinea;

                detallesFactura.Add(new DetalleFactura
                {
                    IdItem = itemFacturable.IdItem,
                    Cantidad = itemDto.Cantidad,
                    PrecioUnitarioHistorico = precioUnitario
                });
            }

            decimal ivaCalculado = subtotal * TasaIva;
            decimal totalCalculado = subtotal + ivaCalculado;

            // 2. Crear cabecera de la factura con datos históricos del cliente
            var factura = new Factura
            {
                NumeroFactura = model.NumeroFactura,
                Fecha = DateTime.Now,
                Subtotal = subtotal,
                Iva = ivaCalculado,
                Total = totalCalculado,
                Estado = "Emitida",
                IdCliente = cliente.IdCliente,
                IdUsuario = idUsuario,
                NombreClienteHistorico = cliente.NombreCliente,
                TelefonoClienteHistorico = cliente.Telefono,
                CedulaClienteHistorica = cliente.Cedula
            };

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync(); // Genera IdFactura

            // 3. Asociar detalles y descontar inventario de forma atómica
            foreach (var detalle in detallesFactura)
            {
                detalle.IdFactura = factura.IdFactura;
                _context.DetallesFactura.Add(detalle);

                // Si el ítem es un producto, registrar la salida de inventario vinculada
                var producto = await _context.Productos.FindAsync(detalle.IdItem);
                if (producto != null)
                {
                    await _servicioInventario.RegistrarSalidaAsync(
                        producto.IdItem,
                        detalle.Cantidad,
                        "Salida por venta",
                        idUsuario,
                        $"Factura N°: {factura.NumeroFactura}"
                    );
                }
            }

            await _context.SaveChangesAsync();

            // 4. Registrar en auditoría
            await _servicioAuditoria.RegistrarAccionAsync(
                "EMITIR_FACTURA",
                $"FACTURA: {factura.NumeroFactura}, Total: ${totalCalculado:N2}",
                idUsuario,
                $"Venta realizada al cliente {cliente.NombreCliente}"
            );

            await transaction.CommitAsync();
            return RedirectToAction(nameof(Details), new { id = factura.IdFactura });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", $"Error al procesar la venta: {ex.Message}");
            CargarListasSelect();
            return View(model);
        }
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var factura = await _context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Usuario)
            .Include(f => f.Detalles)
                .ThenInclude(d => d.Item)
            .FirstOrDefaultAsync(f => f.IdFactura == id);

        if (factura == null) return NotFound();

        return View(factura);
    }

    private void CargarListasSelect()
    {
        ViewBag.Clientes = new SelectList(_context.Clientes.Where(c => c.Estado == "Activo"), "IdCliente", "NombreCliente");
        ViewBag.Productos = new SelectList(_context.Productos.Where(p => p.Estado == "Activo" && p.Existencias > 0), "IdItem", "Descripcion");
    }
}
