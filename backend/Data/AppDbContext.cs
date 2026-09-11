using Microsoft.EntityFrameworkCore;
using LenguajesFormalesAPI.Models;

namespace LenguajesFormalesAPI.Data;

public class AppDbContext : DbContext
{
    // Hash BCrypt estable de la contraseña inicial. No debe generarse dentro de
    // OnModelCreating porque alteraría el modelo en cada ejecución/migración.
    private const string AdminPasswordHash =
        "$2a$11$/BUgxH0omkS815S/Mn0Gj.uyDU1bBvlsof/OIkazK6aINOEAxYuoi";

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<BitacoraLogin> BitacoraLogins { get; set; }
    public DbSet<ResultadoAnalisis> ResultadosAnalisis { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Correo).IsUnique();
            entity.HasIndex(u => u.Nickname).IsUnique();
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_usuarios_correo", "length(trim(correo)) > 3");
                table.HasCheckConstraint("CK_usuarios_telefono", "length(trim(telefono)) BETWEEN 8 AND 20");
                table.HasCheckConstraint("CK_usuarios_nickname", "length(trim(nickname)) BETWEEN 3 AND 50");
                table.HasCheckConstraint("CK_usuarios_notificacion", "metodo_notificacion IN ('email', 'whatsapp', 'ambos')");
                table.HasCheckConstraint("CK_usuarios_rol", "rol IN ('ADMIN', 'SUPERVISOR', 'ANALISTA')");
            });
        });

        modelBuilder.Entity<BitacoraLogin>(entity =>
        {
            entity.HasOne(b => b.Usuario)
                  .WithMany(u => u.BitacoraLogins)
                  .HasForeignKey(b => b.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_bitacora_resultado", "resultado IN ('exitoso', 'fallido')");
                table.HasCheckConstraint("CK_bitacora_metodo", "metodo IN ('password', 'facial', 'qr')");
            });
        });

        modelBuilder.Entity<ResultadoAnalisis>(entity =>
        {
            entity.HasOne(r => r.Usuario)
                  .WithMany(u => u.ResultadosAnalisis)
                  .HasForeignKey(r => r.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_resultado_idioma", "idioma IN ('español', 'inglés', 'ruso', 'chino', 'árabe')");
                table.HasCheckConstraint("CK_resultado_archivo", "length(trim(nombre_archivo)) > 0");
                table.HasCheckConstraint("CK_resultado_totales", "total_palabras >= 0 AND total_tokens >= 0");
            });
        });

        // Seed: usuario administrador inicial
        modelBuilder.Entity<Usuario>().HasData(new Usuario
        {
            Id = 1,
            Correo = "admin@lenguajes.umg.edu.gt",
            Telefono = "50200000000",
            FechaNacimiento = new DateTime(1990, 1, 1),
            Nickname = "admin",
            PasswordHash = AdminPasswordHash,
            MetodoNotificacion = "email",
            Rol = "ADMIN",
            Activo = true,
            FechaRegistro = new DateTime(2026, 1, 1)
        });
    }
}
