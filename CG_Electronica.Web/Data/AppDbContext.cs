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

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        // Nombres de tablas en español, tal como en tu script
        modelBuilder.Entity<Usuario>().ToTable("USUARIO");
        modelBuilder.Entity<Rol>().ToTable("ROL");
        modelBuilder.Entity<Permiso>().ToTable("PERMISO");
        modelBuilder.Entity<RolPermiso>().ToTable("ROL_PERMISO");
        modelBuilder.Entity<Marca>().ToTable("MARCA");
        modelBuilder.Entity<Categoria>().ToTable("CATEGORIA");
        modelBuilder.Entity<Cliente>().ToTable("CLIENTE");
    }
}