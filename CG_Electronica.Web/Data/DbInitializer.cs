using CG_Electronica.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        var hasher = new PasswordHasher<Usuario>();

        
        var permisosBase = new[]
        {
            ("Usuarios.Ver", "Ver usuarios"), ("Usuarios.Editar", "Crear, editar y eliminar usuarios"),
            ("Roles.Ver", "Ver roles"), ("Roles.Editar", "Gestionar roles y permisos"),
            ("Catalogos.Ver", "Ver marcas y categorías"), ("Catalogos.Editar", "Gestionar marcas y categorías"),
            ("Clientes.Ver", "Ver clientes"), ("Clientes.Editar", "Gestionar clientes"),
            ("Ventas.Ver", "Registrar ventas"), ("Facturas.Ver", "Consultar facturas"),
            ("Facturas.Anular", "Anular facturas"),
            ("Reportes.Ver", "Consultar reportes")
        };
        var permisosFaltantesIniciales = permisosBase.Where(p => !context.Permisos.Any(db => db.NombrePermiso == p.Item1)).Select(p => new Permiso { NombrePermiso = p.Item1, Descripcion = p.Item2 }).ToList();
        if (permisosFaltantesIniciales.Count > 0) { context.Permisos.AddRange(permisosFaltantesIniciales); context.SaveChanges(); }

        var rolAdmin = context.Roles
            .Include(r => r.RolPermisos)
            .FirstOrDefault(r => r.NombreRol == "Administrador");

        if (rolAdmin == null)
        {
            rolAdmin = new Rol
            {
                NombreRol = "Administrador",
                Descripcion = "Acceso total al sistema"
            };
            context.Roles.Add(rolAdmin);
            context.SaveChanges(); // Para obtener el IdRol generado
        }

        // Asegurar que el rol admin tenga TODOS los permisos
        var todosLosPermisos = context.Permisos.ToList();
        var permisosFaltantes = todosLosPermisos
            .Where(p => !rolAdmin.RolPermisos.Any(rp => rp.IdPermiso == p.IdPermiso))
            .ToList();

        foreach (var p in permisosFaltantes)
        {
            rolAdmin.RolPermisos.Add(new RolPermiso { IdRol = rolAdmin.IdRol, IdPermiso = p.IdPermiso });
        }
        if (permisosFaltantes.Any()) context.SaveChanges();

        
        var admin = context.Usuarios
            .FirstOrDefault(u => u.NombreUsuario == "admin");

        if (admin == null)
        {
            admin = new Usuario
            {
                Nombre = "Administrador del Sistema",
                NombreUsuario = "admin",
                Estado = "Activo",
                IdRol = rolAdmin.IdRol
            };
            admin.Contrasena = hasher.HashPassword(admin, "Admin123!");
            context.Usuarios.Add(admin);
        }
        else
        {
            // Si el hash guardado no verifica contra Admin123!, lo re-establecemos.
            // Esto corrige usuarios creados en pruebas con texto plano u otro hasher.
            if (hasher.VerifyHashedPassword(admin, admin.Contrasena, "Admin123!")
                    == PasswordVerificationResult.Failed)
            {
                admin.Contrasena = hasher.HashPassword(admin, "Admin123!");
            }
        }

        context.SaveChanges();
    }
}
