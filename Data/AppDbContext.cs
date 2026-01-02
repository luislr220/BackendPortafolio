using Microsoft.EntityFrameworkCore;
using BackendPortafolio.Models;

namespace BackendPortafolio.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    //Definición de las tablas 
    public DbSet<Usuario> Usuarios {get; set;}
    public DbSet<Proyecto> Proyectos {get; set;}
    public DbSet<Tecnologia> Tecnologias {get; set;}
    public DbSet<ProyectoTecnologia> ProyectoTecnologias {get; set;}
    public DbSet<ProyectoImagen> ProyectoImagenes {get; set;}
    public DbSet<Verificacion2Fa> Verificacion2Fas {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de la llave primaria compuesta para la tabla intermedia
        modelBuilder.Entity<ProyectoTecnologia>()
            .HasKey(pt => new { pt.ProyectoId, pt.TecnologiaId });

        // Configuración adicional: asegurar que el correo sea único
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Correo)
            .IsUnique();
        // Configurar Borrado en Cascada para Proyectos cuando se borra un Usuario
        modelBuilder.Entity<Proyecto>()
            .HasOne(p => p.Usuario)
            .WithMany(u => u.Proyectos)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProyectoImagen>()
            .HasOne(pi => pi.Proyecto)
            .WithMany(p => p.ProyectoImagenes)
            .HasForeignKey(pi => pi.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);
        //Configuracion entre el usuario y el 2fa
        modelBuilder.Entity<Verificacion2Fa>()
            .HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}