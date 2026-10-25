using CG_Electronica.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CG_Electronica.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // --- DbSets Integrante 1 (Seguridad y Base) ---
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Cliente> Clientes => Set<Cliente>();

    // --- TODO: Integrante 2 debe agregar sus DbSets aquí ---
    // public DbSet<ItemFacturable> ItemFacturables => Set<ItemFacturable>();
    // public DbSet<Producto> Productos => Set<Producto>();
    // public DbSet<Servicio> Servicios => Set<Servicio>();
    // public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    // public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    // --- DbSets Integrante 3 (Facturación y Ventas) ---
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<DetalleFactura> DetallesFactura => Set<DetalleFactura>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- INICIO CONFIGURACIÓN INTEGRANTE 1 ---
        // Clave compuesta de la tabla intermedia N:N
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

        // USUARIO -> ROL (sin cascada, igual que en tu script T-SQL)
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.IdRol)
            .OnDelete(DeleteBehavior.Restrict);

        // Restricciones de unicidad (Paso 1 del .md)
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();

        modelBuilder.Entity<Rol>()
            .HasIndex(r => r.NombreRol)
            .IsUnique();

        modelBuilder.Entity<Permiso>()
            .HasIndex(p => p.NombrePermiso)
            .IsUnique();

        modelBuilder.Entity<Marca>()
            .HasIndex(m => m.NombreMarca)
            .IsUnique();

        modelBuilder.Entity<Categoria>()
            .HasIndex(c => c.NombreCategoria)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Cedula)
            .IsUnique();

        // Nombres de tablas en español
        modelBuilder.Entity<Usuario>().ToTable("USUARIO");
        modelBuilder.Entity<Rol>().ToTable("ROL");
        modelBuilder.Entity<Permiso>().ToTable("PERMISO");
        modelBuilder.Entity<RolPermiso>().ToTable("ROL_PERMISO");
        modelBuilder.Entity<Marca>().ToTable("MARCA");
        modelBuilder.Entity<Categoria>().ToTable("CATEGORIA");
        modelBuilder.Entity<Cliente>().ToTable("CLIENTE");
        // --- FIN CONFIGURACIÓN INTEGRANTE 1 ---


        // --- INICIO CONFIGURACIÓN INTEGRANTE 3 ---
        // Unicidad del número de factura[cite: 5, 6]
        modelBuilder.Entity<Factura>()
            .HasIndex(f => f.NumeroFactura)
            .IsUnique();

        // Unicidad compuesta: un mismo item no puede repetirse en líneas distintas de la misma factura[cite: 5, 6]
        modelBuilder.Entity<DetalleFactura>()
            .HasIndex(d => new { d.IdFactura, d.IdItem })
            .IsUnique();

        // Composición Factura -> DetalleFactura (Borrado en cascada configurado a restricción por seguridad contable)
        modelBuilder.Entity<Factura>()
            .HasMany(f => f.Detalles)
            .WithOne(d => d.Factura)
            .HasForeignKey(d => d.IdFactura)
            .OnDelete(DeleteBehavior.Restrict);

        // Nombres de tablas en español (siguiendo el estándar)[cite: 13]
        modelBuilder.Entity<Factura>().ToTable("FACTURA");
        modelBuilder.Entity<DetalleFactura>().ToTable("DETALLEFACTURA");
        // --- FIN CONFIGURACIÓN INTEGRANTE 3 ---
    }
}