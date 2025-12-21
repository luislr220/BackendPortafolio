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
    }
}