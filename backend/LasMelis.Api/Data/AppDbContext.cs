using LasMelis.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LasMelis.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<TipoTurno> TiposTurno => Set<TipoTurno>();
    public DbSet<AsignacionTurno> Asignaciones => Set<AsignacionTurno>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasIndex(e => e.Dni).IsUnique();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Dni).IsRequired().HasMaxLength(15);
        });

        modelBuilder.Entity<TipoTurno>(entity =>
        {
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<AsignacionTurno>(entity =>
        {
            entity.HasIndex(a => new { a.EmpleadoId, a.TipoTurnoId, a.Fecha }).IsUnique();

            entity.HasOne(a => a.Empleado)
                .WithMany(e => e.Asignaciones)
                .HasForeignKey(a => a.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.TipoTurno)
                .WithMany(t => t.Asignaciones)
                .HasForeignKey(a => a.TipoTurnoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.PasswordHash).IsRequired();
        });

        // Seed: tipos de turno base
        modelBuilder.Entity<TipoTurno>().HasData(
            new TipoTurno { Id = 1, Nombre = "Mañana", HoraInicio = new TimeOnly(6, 0), HoraFin = new TimeOnly(14, 0) },
            new TipoTurno { Id = 2, Nombre = "Tarde", HoraInicio = new TimeOnly(14, 0), HoraFin = new TimeOnly(22, 0) },
            new TipoTurno { Id = 3, Nombre = "Noche", HoraInicio = new TimeOnly(22, 0), HoraFin = new TimeOnly(6, 0) }
        );

        // Seed: usuario admin (hash BCrypt precalculado de "Havanna2026!" — ver README para credenciales)
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "$2a$11$iLsqejKxqzMKuME2tQt9M.NPvVUygEJnY/ho7HBr2BMu4mDFAg9Mu"
            }
        );
    }
}
