using CG_Electronica.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CG_Electronica.Web.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        var hasher = new PasswordHasher<Usuario>();

        // ---------- PERMISOS (solo si no existen) ----------
        if (!context.Permisos.Any())
        {
            context.Permisos.AddRange(
                new Permiso { NombrePermiso = "Usuarios.Ver", Descripcion = "Ver usuarios" },
                new Permiso { NombrePermiso = "Usuarios.Editar", Descripcion = "Crear, editar y eliminar usuarios" },
                new Permiso { NombrePermiso = "Roles.Ver", Descripcion = "Ver roles" },
                new Permiso { NombrePermiso = "Roles.Editar", Descripcion = "Gestionar roles y permisos" },
                new Permiso { NombrePermiso = "Catalogos.Ver", Descripcion = "Ver marcas y categorías" },
                new Permiso { NombrePermiso = "Catalogos.Editar", Descripcion = "Gestionar marcas y categorías" },
                new Permiso { NombrePermiso = "Clientes.Ver", Descripcion = "Ver clientes" },
                new Permiso { NombrePermiso = "Clientes.Editar", Descripcion = "Gestionar clientes" }
            );
            context.SaveChanges();
        }

        // ---------- ROL ADMINISTRADOR ----------
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

        // ---------- USUARIO ADMIN (siempre existe y con contraseña válida) ----------
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
