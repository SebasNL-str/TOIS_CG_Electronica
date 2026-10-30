using CG_Electronica.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
    {
    }

    // =========================================================
    // SEGURIDAD Y USUARIOS
    // =========================================================

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();

    // =========================================================
    // CLIENTES Y CATÁLOGOS
    // =========================================================

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Categoria> Categorias => Set<Categoria>();

    // =========================================================
    // INVENTARIO E ITEMS FACTURABLES
    // =========================================================

    public DbSet<ItemFacturable> ItemsFacturables => Set<ItemFacturable>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    // =========================================================
    // FACTURACIÓN Y VENTAS
    // =========================================================

    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<DetalleFactura> DetallesFactura => Set<DetalleFactura>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================
        // 1. SEGURIDAD Y USUARIOS
        // =====================================================

        modelBuilder.Entity<Usuario>()
        .ToTable("USUARIO");

        modelBuilder.Entity<Rol>()
        .ToTable("ROL");

        modelBuilder.Entity<Permiso>()
        .ToTable("PERMISO");

        modelBuilder.Entity<RolPermiso>()
        .ToTable("ROL_PERMISO");

        // Configuración de Estado para ROL
        modelBuilder.Entity<Rol>()
        .Property(r => r.Estado)
        .HasColumnName("estado")
        .HasColumnType("varchar(30)")
        .HasMaxLength(30)
        .HasDefaultValue("Activo");

        // Configuración de Estado para USUARIO
        modelBuilder.Entity<Usuario>()
        .Property(u => u.Estado)
        .HasColumnName("estado")
        .HasColumnType("varchar(30)")
        .HasMaxLength(30);

        // Clave compuesta de la relación N:N entre roles y permisos.
        modelBuilder.Entity<RolPermiso>()
        .HasKey(rp => new { rp.IdRol, rp.IdPermiso });

        modelBuilder.Entity<RolPermiso>()
        .HasOne(rp => rp.Rol)
        .WithMany(r => r.RolPermisos)
        .HasForeignKey(rp => rp.IdRol)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolPermiso>()
        .HasOne(rp => rp.Permiso)
        .WithMany(p => p.RolPermisos)
        .HasForeignKey(rp => rp.IdPermiso)
        .OnDelete(DeleteBehavior.Cascade);

        // Usuario -> Rol: restringe la eliminación de roles con usuarios.
        modelBuilder.Entity<Usuario>()
        .HasOne(u => u.Rol)
        .WithMany(r => r.Usuarios)
        .HasForeignKey(u => u.IdRol)
        .OnDelete(DeleteBehavior.Restrict);

        // Índices únicos de seguridad.
        modelBuilder.Entity<Usuario>()
        .HasIndex(u => u.NombreUsuario)
        .IsUnique();

        modelBuilder.Entity<Rol>()
        .HasIndex(r => r.NombreRol)
        .IsUnique();

        modelBuilder.Entity<Permiso>()
        .HasIndex(p => p.NombrePermiso)
        .IsUnique();

        // =====================================================
        // 2. CLIENTES Y CATÁLOGOS
        // =====================================================

        modelBuilder.Entity<Cliente>()
        .ToTable("CLIENTE");

        modelBuilder.Entity<Marca>()
        .ToTable("MARCA");

        // Configuración de Estado para CLIENTE
        modelBuilder.Entity<Cliente>()
        .Property(c => c.Estado)
        .HasColumnName("estado")
        .HasColumnType("varchar(30)")
        .HasMaxLength(30);

        // Configuración de Estado para MARCA
        modelBuilder.Entity<Marca>()
        .Property(m => m.Estado)
        .HasColumnName("estado")
        .HasColumnType("varchar(30)")
        .HasMaxLength(30);

        // Mapeo explícito para CATEGORIA
        modelBuilder.Entity<Categoria>()
        .ToTable("CATEGORIA")
        .HasKey(c => c.IdCategoria);

        modelBuilder.Entity<Categoria>()
        .Property(c => c.IdCategoria)
        .HasColumnName("idCategoria");

        modelBuilder.Entity<Categoria>()
        .Property(c => c.NombreCategoria)
        .HasColumnName("nombreCategoria");

        modelBuilder.Entity<Categoria>()
        .Property(c => c.Estado)
        .HasColumnName("estado")
        .HasColumnType("varchar(30)")
        .HasMaxLength(30)
        .HasDefaultValue("Activo");

        modelBuilder.Entity<Cliente>()
        .HasIndex(c => c.Cedula)
        .IsUnique();

        modelBuilder.Entity<Marca>()
        .HasIndex(m => m.NombreMarca)
        .IsUnique();

        modelBuilder.Entity<Categoria>()
        .HasIndex(c => c.NombreCategoria)
        .IsUnique();

        // =====================================================
        // 3. HERENCIA TPT: ITEMFACTURABLE, PRODUCTO Y SERVICIO
        // =====================================================

        // Entidad base.
        modelBuilder.Entity<ItemFacturable>()
        .ToTable("ITEMFACTURABLE")
        .HasKey(i => i.IdItem);

        modelBuilder.Entity<ItemFacturable>()
        .Property(i => i.IdItem)
        .HasColumnName("idItem");

        modelBuilder.Entity<ItemFacturable>()
        .Property(i => i.PrecioVenta)
        .HasColumnName("precioVenta")
        .HasPrecision(18, 2);

        modelBuilder.Entity<ItemFacturable>()
        .Property(i => i.Estado)
        .HasColumnName("estado");

        // -----------------------------------------------------
        // PRODUCTO
        // -----------------------------------------------------

        modelBuilder.Entity<Producto>()
        .ToTable("PRODUCTO")
        .HasBaseType<ItemFacturable>();

        modelBuilder.Entity<Producto>()
        .Property(p => p.Descripcion)
        .HasColumnName("descripcion");

        modelBuilder.Entity<Producto>()
        .Property(p => p.Modelo)
        .HasColumnName("modelo");

        modelBuilder.Entity<Producto>()
        .Property(p => p.CodigoFabricante)
        .HasColumnName("codigoFabricante");

        modelBuilder.Entity<Producto>()
        .Property(p => p.PrecioCompra)
        .HasColumnName("precioCompra")
        .HasPrecision(18, 2);

        modelBuilder.Entity<Producto>()
        .Property(p => p.Existencias)
        .HasColumnName("existencias");

        modelBuilder.Entity<Producto>()
        .Property(p => p.ExistenciasMinimas)
        .HasColumnName("existenciasMinimas");

        modelBuilder.Entity<Producto>()
        .Property(p => p.IdMarca)
        .HasColumnName("idMarca");

        modelBuilder.Entity<Producto>()
        .Property(p => p.IdCategoria)
        .HasColumnName("idCategoria");

        // Relaciones explícitas: Producto -> Marca y Producto -> Categoria.
        modelBuilder.Entity<Producto>()
        .HasOne(p => p.Marca)
        .WithMany()
        .HasForeignKey(p => p.IdMarca);

        modelBuilder.Entity<Producto>()
        .HasOne(p => p.Categoria)
        .WithMany()
        .HasForeignKey(p => p.IdCategoria);

        // -----------------------------------------------------
        // SERVICIO
        // -----------------------------------------------------

        modelBuilder.Entity<Servicio>()
        .ToTable("SERVICIO")
        .HasBaseType<ItemFacturable>();

        modelBuilder.Entity<Servicio>()
        .Property(s => s.NombreServicio)
        .HasColumnName("nombreServicio");

        // =====================================================
        // 4. MOVIMIENTOS DE INVENTARIO
        // =====================================================

        modelBuilder.Entity<MovimientoInventario>()
        .ToTable("MOVIMIENTOINVENTARIO")
        .HasKey(m => m.IdMovimiento);

        modelBuilder.Entity<MovimientoInventario>()
        .Property(m => m.IdMovimiento)
        .HasColumnName("idMovimiento");

        modelBuilder.Entity<MovimientoInventario>()
        .Property(m => m.PrecioCompraUnitario)
        .HasPrecision(18, 2);

        modelBuilder.Entity<MovimientoInventario>()
        .HasOne(m => m.Producto)
        .WithMany(p => p.Movimientos)
        .HasForeignKey(m => m.IdProducto)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoInventario>()
        .HasOne(m => m.Usuario)
        .WithMany()
        .HasForeignKey(m => m.IdUsuario)
        .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 5. AUDITORÍA
        // =====================================================

        modelBuilder.Entity<Auditoria>()
        .ToTable("AUDITORIA")
        .HasKey(a => a.IdAuditoria);

        modelBuilder.Entity<Auditoria>()
        .Property(a => a.IdAuditoria)
        .HasColumnName("idAuditoria");

        modelBuilder.Entity<Auditoria>()
        .HasOne(a => a.Usuario)
        .WithMany()
        .HasForeignKey(a => a.IdUsuario)
        .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // 6. FACTURACIÓN
        // =====================================================

        modelBuilder.Entity<Factura>()
        .ToTable("FACTURA")
        .HasKey(f => f.IdFactura);

        modelBuilder.Entity<Factura>()
        .Property(f => f.IdFactura)
        .HasColumnName("idFactura");

        modelBuilder.Entity<Factura>()
        .Property(f => f.Iva)
        .HasPrecision(18, 2);

        modelBuilder.Entity<Factura>()
        .Property(f => f.Subtotal)
        .HasPrecision(18, 2);

        modelBuilder.Entity<Factura>()
        .Property(f => f.Total)
        .HasPrecision(18, 2);

        modelBuilder.Entity<Factura>()
        .HasOne(f => f.Usuario)
        .WithMany()
        .HasForeignKey(f => f.IdUsuario)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Factura>()
        .HasOne(f => f.Cliente)
        .WithMany()
        .HasForeignKey(f => f.IdCliente)
        .OnDelete(DeleteBehavior.Restrict);

        // Unicidad del número de factura.
        modelBuilder.Entity<Factura>()
        .HasIndex(f => f.NumeroFactura)
        .IsUnique();

        // =====================================================
        // 7. DETALLES DE FACTURA
        // =====================================================

        modelBuilder.Entity<DetalleFactura>()
        .ToTable("DETALLEFACTURA")
        .HasKey(d => d.IdDetalle);

        modelBuilder.Entity<DetalleFactura>()
        .Property(d => d.IdDetalle)
        .HasColumnName("idDetalle");

        modelBuilder.Entity<DetalleFactura>()
        .Property(d => d.PrecioUnitarioHistorico)
        .HasPrecision(18, 2);

        // Un mismo item no puede repetirse en varias líneas
        // de una misma factura.
        modelBuilder.Entity<DetalleFactura>()
        .HasIndex(d => new { d.IdFactura, d.IdItem })
        .IsUnique();

        // Factura -> Detalles: protege los registros contables
        // frente al borrado en cascada.
        modelBuilder.Entity<DetalleFactura>()
        .HasOne(d => d.Factura)
        .WithMany(f => f.Detalles)
        .HasForeignKey(d => d.IdFactura)
        .OnDelete(DeleteBehavior.Restrict);

        // ItemFacturable -> Detalles.
        modelBuilder.Entity<DetalleFactura>()
        .HasOne(d => d.Item)
        .WithMany(i => i.DetallesFactura)
        .HasForeignKey(d => d.IdItem)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
