using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LenguajesFormalesAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_nacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    nickname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    metodo_notificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    foto_original = table.Column<string>(type: "text", nullable: true),
                    foto_modificada = table.Column<string>(type: "text", nullable: true),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.AddCheckConstraint("CK_usuarios_correo", "usuarios", "length(trim(correo)) > 3");
            migrationBuilder.AddCheckConstraint("CK_usuarios_telefono", "usuarios", "length(trim(telefono)) BETWEEN 8 AND 20");
            migrationBuilder.AddCheckConstraint("CK_usuarios_nickname", "usuarios", "length(trim(nickname)) BETWEEN 3 AND 50");
            migrationBuilder.AddCheckConstraint("CK_usuarios_notificacion", "usuarios", "metodo_notificacion IN ('email', 'whatsapp', 'ambos')");
            migrationBuilder.AddCheckConstraint("CK_usuarios_rol", "usuarios", "rol IN ('ADMIN', 'SUPERVISOR', 'ANALISTA')");

            migrationBuilder.CreateTable(
                name: "bitacora_login",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip_origen = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    resultado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    metodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bitacora_login", x => x.id);
                    table.ForeignKey(
                        name: "FK_bitacora_login_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint("CK_bitacora_resultado", "bitacora_login", "resultado IN ('exitoso', 'fallido')");
            migrationBuilder.AddCheckConstraint("CK_bitacora_metodo", "bitacora_login", "metodo IN ('password', 'facial', 'qr')");

            migrationBuilder.CreateTable(
                name: "resultado_analisis",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    idioma = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombre_archivo = table.Column<string>(type: "text", nullable: false),
                    total_palabras = table.Column<int>(type: "integer", nullable: false),
                    total_tokens = table.Column<int>(type: "integer", nullable: false),
                    detalle_json = table.Column<string>(type: "text", nullable: false),
                    fecha_analisis = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resultado_analisis", x => x.id);
                    table.ForeignKey(
                        name: "FK_resultado_analisis_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint("CK_resultado_idioma", "resultado_analisis", "idioma IN ('español', 'inglés', 'ruso', 'chino', 'árabe')");
            migrationBuilder.AddCheckConstraint("CK_resultado_archivo", "resultado_analisis", "length(trim(nombre_archivo)) > 0");
            migrationBuilder.AddCheckConstraint("CK_resultado_totales", "resultado_analisis", "total_palabras >= 0 AND total_tokens >= 0");

            migrationBuilder.InsertData(
                table: "usuarios",
                columns: new[] { "id", "activo", "correo", "fecha_nacimiento", "fecha_registro", "foto_modificada", "foto_original", "metodo_notificacion", "nickname", "password_hash", "rol", "telefono" },
                values: new object[] { 1, true, "admin@lenguajes.umg.edu.gt", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "email", "admin", "$2a$11$/BUgxH0omkS815S/Mn0Gj.uyDU1bBvlsof/OIkazK6aINOEAxYuoi", "ADMIN", "50200000000" });

            migrationBuilder.CreateIndex(
                name: "IX_bitacora_login_usuario_id",
                table: "bitacora_login",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_resultado_analisis_usuario_id",
                table: "resultado_analisis",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_correo",
                table: "usuarios",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_nickname",
                table: "usuarios",
                column: "nickname",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bitacora_login");

            migrationBuilder.DropTable(
                name: "resultado_analisis");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
