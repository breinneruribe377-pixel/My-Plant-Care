using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyPlantCare.Models;

namespace MyPlantCare.Data;

/// <summary>
/// Mapea las entidades a las tablas creadas con database/01_schema.sql.
/// El esquema NO lo crea EF (no se usan migraciones): la fuente de verdad es el script DDL.
/// </summary>
public class PlantCareDbContext : DbContext
{
    public PlantCareDbContext(DbContextOptions<PlantCareDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Planta> Plantas => Set<Planta>();
    public DbSet<Cuidado> Cuidados => Set<Cuidado>();

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        // Las fechas se guardan en UTC; al leerlas se marcan como UTC para que el JSON salga con "Z".
        builder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        builder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var tipoCuidadoConverter = new ValueConverter<TipoCuidado, string>(
            v => TipoCuidadoDb.ToDb(v),
            v => TipoCuidadoDb.FromDb(v));

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuario");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            e.Property(u => u.FechaRegistro).HasColumnName("fecha_registro").HasColumnType("datetime");
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Planta>(e =>
        {
            e.ToTable("planta");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.UsuarioId).HasColumnName("usuario_id");
            e.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            e.Property(p => p.Especie).HasColumnName("especie").HasMaxLength(100);
            e.Property(p => p.Ubicacion).HasColumnName("ubicacion").HasMaxLength(100);
            e.Property(p => p.FotoUrl).HasColumnName("foto").HasMaxLength(255);
            e.Property(p => p.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("datetime");

            e.HasOne(p => p.Usuario)
                .WithMany(u => u.Plantas)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cuidado>(e =>
        {
            e.ToTable("cuidado");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.PlantaId).HasColumnName("planta_id");
            e.Property(c => c.Tipo).HasColumnName("tipo").HasMaxLength(30).HasConversion(tipoCuidadoConverter);
            e.Property(c => c.FrecuenciaDias).HasColumnName("frecuencia_dias");
            e.Property(c => c.UltimaVezRealizado).HasColumnName("ultima_vez_realizado").HasColumnType("datetime");
            e.Property(c => c.ProximaFecha).HasColumnName("proxima_fecha").HasColumnType("datetime");

            e.HasOne(c => c.Planta)
                .WithMany(p => p.Cuidados)
                .HasForeignKey(c => c.PlantaId)
                .OnDelete(DeleteBehavior.Cascade); // Igual que ON DELETE CASCADE del DDL
        });
    }
}

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) { }
}

public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter()
        : base(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v) { }
}
