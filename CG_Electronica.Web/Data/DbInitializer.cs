using CG_Electronica.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        // Asegurar que la base de datos exista
        context.Database.Migrate();

        if (context.Roles.Any()) return; // Ya hay datos, no hacer nada

        // ---------- ROLES ----------
        var rolAdmin = new Rol
        {
            NombreRol = "Administrador",
            Descripcion = "Acceso total al sistema"
        };
        var rolVendedor = new Rol
        {
            NombreRol = "Vendedor",
            Descripcion = "Gestión de ventas y clientes"
        };
        context.Roles.AddRange(rolAdmin, rolVendedor);

        // ---------- PERMISOS ----------
        var permisos = new[]
        {
            new Permiso { NombrePermiso = "Usuarios.Ver",   Descripcion = "Ver usuarios" },
            new Permiso { NombrePermiso = "Usuarios.Editar", Descripcion = "Crear, editar y eliminar usuarios" },
            new Permiso { NombrePermiso = "Roles.Ver",      Descripcion = "Ver roles" },
            new Permiso { NombrePermiso = "Roles.Editar",   Descripcion = "Gestionar roles y permisos" },
            new Permiso { NombrePermiso = "Catalogos.Ver",  Descripcion = "Ver marcas y categorías" },
            new Permiso { NombrePermiso = "Catalogos.Editar", Descripcion = "Gestionar marcas y categorías" },
            new Permiso { NombrePermiso = "Clientes.Ver",   Descripcion = "Ver clientes" },
            new Permiso { NombrePermiso = "Clientes.Editar", Descripcion = "Gestionar clientes" }
        };
        context.Permisos.AddRange(permisos);

        // ---------- ROL_PERMISO ----------
        rolAdmin.RolPermisos = permisos
            .Select(p => new RolPermiso { Rol = rolAdmin, Permiso = p })
            .ToList();

        rolVendedor.RolPermisos = permisos
            .Where(p => p.NombrePermiso.StartsWith("Clientes") || p.NombrePermiso.StartsWith("Catalogos.Ver"))
            .Select(p => new RolPermiso { Rol = rolVendedor, Permiso = p })
            .ToList();

        // ---------- USUARIO ADMIN ----------
        var hasher = new PasswordHasher<Usuario>();
        var admin = new Usuario
        {
            Nombre = "Administrador del Sistema",
            NombreUsuario = "admin",
            IdRol = rolAdmin.IdRol, // Se resuelve al guardar por la navegación
            Rol = rolAdmin
        };
        admin.Contrasena = hasher.HashPassword(admin, "Admin123!");
        context.Usuarios.Add(admin);

        context.SaveChanges();
    }
}