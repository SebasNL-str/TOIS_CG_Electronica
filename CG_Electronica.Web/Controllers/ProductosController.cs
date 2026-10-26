using CG_Electronica.Web.Data;
using CG_Electronica.Web.Models;
using CG_Electronica.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Controllers;

[Authorize]
public class ProductosController : Controller
{
    private readonly AppDbContext _context;
    private readonly IServicioInventario _servicioInventario;
    private readonly IServicioAuditoria _servicioAuditoria;

    public ProductosController(AppDbContext context, IServicioInventario servicioInventario, IServicioAuditoria servicioAuditoria)
    {
        _context = context;
        _servicioInventario = servicioInventario;
        _servicioAuditoria = servicioAuditoria;
    }

    public async Task<IActionResult> Index()
    {
        var productos = await _context.Productos
            .Include(p => p.Marca)
            .Include(p => p.Categoria)
            .ToListAsync();
        return View(productos);
    }

    public IActionResult Create()
    {
        CargarListasSelect();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto, decimal precioCompraInicial, int stockInicial)
    {
        // Limpiar validaciones de navegación que no vienen del formulario
        ModelState.Remove(nameof(producto.Marca));
        ModelState.Remove(nameof(producto.Categoria));
        ModelState.Remove(nameof(producto.DetallesFactura));
        ModelState.Remove(nameof(producto.Movimientos));

        if (ModelState.IsValid)
        {
            // 1. Crear el ItemFacturable base
            producto.Existencias = 0; // Se inicializa mediante movimiento de entrada inicial
            producto.PrecioCompra = precioCompraInicial;
            _context.ItemsFacturables.Add(producto);
            await _context.SaveChangesAsync(); // Esto genera el idItem compartido

            // 2. Si hay stock inicial, registrar la entrada mediante el servicio de inventario
            if (stockInicial > 0)
            {
                int idUsuario = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value != null 
                    ? int.Parse(HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value) 
                    : 1; // Ajustar según claims de sesión

                await _servicioInventario.RegistrarEntradaAsync(producto.IdItem, stockInicial, precioCompraInicial, "Entrada por compra", idUsuario, "Inventario inicial de creación");
            }

            return RedirectToAction(nameof(Index));
        }

        CargarListasSelect();
        return View(producto);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        CargarListasSelect();
        return View(producto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.IdItem) return NotFound();

        ModelState.Remove(nameof(producto.Marca));
        ModelState.Remove(nameof(producto.Categoria));
        ModelState.Remove(nameof(producto.DetallesFactura));
        ModelState.Remove(nameof(producto.Movimientos));

        if (ModelState.IsValid)
        {
            try
            {
                var productoDb = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.IdItem == id);
                if (productoDb == null) return NotFound();

                // Preservar existencias y precio de compra actuales (se manejan por inventario/compras)
                producto.Existencias = productoDb.Existencias;
                producto.PrecioCompra = productoDb.PrecioCompra;

                _context.Update(producto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(producto.IdItem)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        CargarListasSelect();
        return View(producto);
    }

    private bool ProductoExists(int id)
    {
        return _context.Productos.Any(e => e.IdItem == id);
    }

    private void CargarListasSelect()
    {
        ViewBag.Marcas = new SelectList(_context.Marcas.Where(m => m.Estado == "Activo"), "IdMarca", "NombreMarca");
        ViewBag.Categorias = new SelectList(_context.Categorias.Where(c => c.Estado == "Activo"), "IdCategoria", "NombreCategoria");
    }
}