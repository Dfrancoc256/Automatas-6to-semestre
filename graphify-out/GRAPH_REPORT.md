# Graph Report - Automatas-6to-semestre  (2026-10-09)

## Corpus Check
- 69 files · ~363,332 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 684 nodes · 954 edges · 59 communities (47 shown, 5 thin omitted)
- Extraction: 93% EXTRACTED · 7% INFERRED · 0% AMBIGUOUS · INFERRED: 66 edges (avg confidence: 0.84)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `9a89ab52`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- AuthController
- package.json
- UsuarioPerfilDTO
- RegistrationForm.vue
- http
- AnalisisResultadoDTO
- login.vue
- Usuario
- Task
- analisis.vue
- Usuario
- LenguajesFormalesAPI.Services
- LenguajesFormalesAPI.csproj
- perfil.vue
- AnalisisController
- FacialService
- historial.vue
- graphify-out/ knowledge graph store
- AnalisisDTOs.cs
- RecaptchaV2.vue
- usuarios.vue
- HistorialAnalisisDTO
- AnalisisLexicoService
- dashboard.vue
- compilerOptions
- bitacora.vue
- dashboard/index.vue
- reportes.vue
- Universidad Mariano Gálvez de Guatemala Official Seal (Logo)
- Campus Entrance Gate 1 / Guard Booth
- UMG Campus Aerial Background Image (fondo-umg.png)
- stores/auth.ts
- Campus UMG Aerial Photo
- UMG Campus Aerial Photo (Volcán de Agua Backdrop)
- FaceScanner.vue
- DashboardController
- RegisterDTO
- FotografiaUsuario
- useMediaUrl
- ResultadoAnalisis
- AuthDTOs.cs
- AuthService
- AppDbContext
- BitacoraLogin
- LoginFacialDTO
- ActualizarPerfilDTO
- AuthResponseDTO
- LenguajesFormalesAPI.Models
- FilterSelector.vue
- StickerSelector.vue
- AuditMiddleware
- WhatsApp con Evolution API en Render

## God Nodes (most connected - your core abstractions)
1. `AnalisisResultadoDTO` - 30 edges
2. `Usuario` - 21 edges
3. `AuthResponseDTO` - 17 edges
4. `AuthService` - 17 edges
5. `AuthController` - 16 edges
6. `RegisterDTO` - 16 edges
7. `AppDbContext` - 16 edges
8. `BitacoraLogin` - 16 edges
9. `ResultadoAnalisis` - 16 edges
10. `Usuario` - 15 edges

## Surprising Connections (you probably didn't know these)
- `AnalisisController` --references--> `AppDbContext`  [EXTRACTED]
  backend/Controllers/AnalisisController.cs → backend/Data/AppDbContext.cs
- `AnalisisController` --references--> `IAnalisisLexicoService`  [EXTRACTED]
  backend/Controllers/AnalisisController.cs → backend/Services/AnalisisLexicoService.cs
- `AuthController` --references--> `IAuthService`  [EXTRACTED]
  backend/Controllers/AuthController.cs → backend/Services/AuthService.cs
- `AuthController` --references--> `ICredentialService`  [EXTRACTED]
  backend/Controllers/AuthController.cs → backend/Services/CredentialService.cs
- `AuthController` --references--> `IJwtService`  [EXTRACTED]
  backend/Controllers/AuthController.cs → backend/Services/JwtService.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Codebase Question Resolution Priority Flow** — claude_graphify_query, claude_graphify_path, claude_graphify_explain, claude_wiki_index_md, claude_graph_report_md [EXTRACTED 1.00]

## Communities (59 total, 5 thin omitted)

### Community 0 - "AuthController"
Cohesion: 0.15
Nodes (19): AllowAnonymous, Authorize, DateTime, HttpGet, HttpPost, IActionResult, ILogger, Task (+11 more)

### Community 1 - "package.json"
Cohesion: 0.05
Nodes (41): CajaRostro, calentar(), cargarModelos(), detectarRostro(), Fuente, obtenerDescriptor(), opcionesDetector(), useFacialRecognition() (+33 more)

### Community 2 - "UsuarioPerfilDTO"
Cohesion: 0.18
Nodes (11): DateTime, UsuarioPerfilDTO, Correo, FechaRegistro, FotoModificada, Id, MetodoNotificacion, Nickname (+3 more)

### Community 3 - "RegistrationForm.vue"
Cohesion: 0.05
Nodes (39): { api }, arrastrandoSticker, auth, bypassDesarrollo, { calentar }, cargando, cargarImagen(), config (+31 more)

### Community 4 - "http"
Cohesion: 0.07
Nodes (28): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, applicationUrl (+20 more)

### Community 5 - "AnalisisResultadoDTO"
Cohesion: 0.08
Nodes (25): List, AnalisisResultadoDTO, Adjetivos, AnalisisId, Conectores, CorreosEncontrados, FechaAnalisis, FechasEncontradas (+17 more)

### Community 6 - "login.vue"
Cohesion: 0.07
Nodes (24): accesoExitoso, { api }, auth, bypassDesarrollo, { calentar }, cargando, config, direccionGiro (+16 more)

### Community 7 - "Usuario"
Cohesion: 0.14
Nodes (11): Usuario, CredentialService, ICredentialService, DateTime, IConfiguration, IJwtService, JwtService, Claim (+3 more)

### Community 9 - "Task"
Cohesion: 0.24
Nodes (6): LoginDTO, Identificador, Password, RecaptchaToken, Task, IAuthService

### Community 10 - "analisis.vue"
Cohesion: 0.12
Nodes (16): { api }, archivo, arrastrando, cargarArchivo(), categorias, contenido, errorMsg, fileInput (+8 more)

### Community 11 - "Usuario"
Cohesion: 0.11
Nodes (18): DateTime, Usuario, Activo, BitacoraLogins, Correo, EncodingFacial, FechaNacimiento, FechaRegistro (+10 more)

### Community 12 - "LenguajesFormalesAPI.Services"
Cohesion: 0.06
Nodes (33): HttpPost, IActionResult, Task, EmailController, HttpPost, IActionResult, Task, WhatsAppController (+25 more)

### Community 13 - "LenguajesFormalesAPI.csproj"
Cohesion: 0.15
Nodes (12): net8.0, BCrypt.Net-Next (4.0.3), MailKit (4.17.0), Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0), Microsoft.AspNetCore.OpenApi (8.0.22), Microsoft.EntityFrameworkCore.Design (8.0.0), Microsoft.EntityFrameworkCore.InMemory (8.0.0), Npgsql.EntityFrameworkCore.PostgreSQL (8.0.10) (+4 more)

### Community 14 - "perfil.vue"
Cohesion: 0.09
Nodes (16): { api }, auth, { calentar }, camaraFacialActiva, enrolando, escanerEnrol, form, formRef (+8 more)

### Community 15 - "AnalisisController"
Cohesion: 0.31
Nodes (7): HashSet, HttpGet, HttpPost, IActionResult, ILogger, Task, AnalisisController

### Community 16 - "FacialService"
Cohesion: 0.16
Nodes (13): HttpGet, HttpPost, IActionResult, ILogger, Task, FacialController, ILogger, List (+5 more)

### Community 17 - "historial.vue"
Cohesion: 0.20
Nodes (7): { api }, cargando, detalle, detalleMet, dialogDetalle, headers, historial

### Community 18 - "graphify-out/ knowledge graph store"
Cohesion: 0.33
Nodes (9): graphify-out/graph.json, graphify-out/GRAPH_REPORT.md, graphify (CLI tool), graphify explain command, graphify-out/ knowledge graph store, graphify path command, graphify query command, graphify update command (+1 more)

### Community 20 - "AnalisisDTOs.cs"
Cohesion: 0.25
Nodes (7): AnalisisRequestDTO, Contenido, Idioma, NombreArchivo, FrecuenciaToken, Frecuencia, Token

### Community 21 - "RecaptchaV2.vue"
Cohesion: 0.25
Nodes (6): contenedor, emit, GrecaptchaApi, mensaje, props, Window

### Community 22 - "usuarios.vue"
Cohesion: 0.22
Nodes (5): { api }, cargando, error, headers, usuarios

### Community 23 - "HistorialAnalisisDTO"
Cohesion: 0.29
Nodes (7): DateTime, HistorialAnalisisDTO, FechaAnalisis, Id, Idioma, NombreArchivo, TotalPalabras

### Community 24 - "AnalisisLexicoService"
Cohesion: 0.38
Nodes (4): HashSet, AnalisisLexicoService, IAnalisisLexicoService, Regex

### Community 25 - "dashboard.vue"
Cohesion: 0.22
Nodes (6): auth, fotoSidebarUrl, { fotoUrl }, route, sidebarCollapsed, sidebarOpen

### Community 26 - "compilerOptions"
Cohesion: 0.29
Nodes (6): compilerOptions, noImplicitOverride, noUncheckedIndexedAccess, strict, extends, ./.nuxt/tsconfig.json

### Community 27 - "bitacora.vue"
Cohesion: 0.33
Nodes (5): { api }, cargando, error, headers, registros

### Community 28 - "dashboard/index.vue"
Cohesion: 0.33
Nodes (4): { api }, auth, modulos, stats

### Community 29 - "reportes.vue"
Cohesion: 0.33
Nodes (5): { api }, cargando, error, metricas, stats

### Community 30 - "Universidad Mariano Gálvez de Guatemala Official Seal (Logo)"
Cohesion: 0.67
Nodes (3): Frontend Branding / Header Asset Usage, Universidad Mariano Gálvez de Guatemala Official Seal (Logo), Universidad Mariano Gálvez de Guatemala (UMG)

### Community 31 - "Campus Entrance Gate 1 / Guard Booth"
Cohesion: 1.00
Nodes (3): Campus Entrance Gate 1 / Guard Booth, UMG Campus Entrance Photo (Gate 1 / Parking 7), Universidad Mariano Gálvez Main Building

### Community 32 - "UMG Campus Aerial Background Image (fondo-umg.png)"
Cohesion: 0.67
Nodes (3): UMG Campus Aerial Background Image (fondo-umg.png), Site Background / Landing Page Visual Asset, Universidad Mariano Gálvez (UMG) Campus

### Community 39 - "FaceScanner.vue"
Cohesion: 0.17
Nodes (15): calcularCaptura(), { calentar, detectarRostro, obtenerDescriptor }, CapturaFacial, capturar(), ciclo(), emit, Estado, evaluarEncuadre() (+7 more)

### Community 40 - "DashboardController"
Cohesion: 0.36
Nodes (7): Authorize, HttpGet, IActionResult, Task, CambiarRolDTO, DashboardController, HttpPatch

### Community 41 - "RegisterDTO"
Cohesion: 0.18
Nodes (11): RegisterDTO, Correo, Descriptor, FechaNacimiento, FotoBase64, FotoModificadaBase64, MetodoNotificacion, Nickname (+3 more)

### Community 42 - "FotografiaUsuario"
Cohesion: 0.18
Nodes (11): DateTime, FotografiaUsuario, Estado, FechaRegistro, HashContenido, Id, ReemplazadaEn, Tipo (+3 more)

### Community 45 - "ResultadoAnalisis"
Cohesion: 0.18
Nodes (11): DateTime, ResultadoAnalisis, DetalleJson, FechaAnalisis, Id, Idioma, NombreArchivo, TotalPalabras (+3 more)

### Community 46 - "AuthDTOs.cs"
Cohesion: 0.20
Nodes (8): CambiarPasswordDTO, NuevoPassword, Token, LoginQrDTO, CodigoQr, RecaptchaToken, ResetPasswordDTO, Correo

### Community 47 - "AuthService"
Cohesion: 0.38
Nodes (3): ILogger, Usuario, AuthService

### Community 48 - "AppDbContext"
Cohesion: 0.22
Nodes (8): AppDbContext, BitacoraLogins, FotografiasUsuario, ResultadosAnalisis, Usuarios, DbContext, DbSet, ModelBuilder

### Community 49 - "BitacoraLogin"
Cohesion: 0.22
Nodes (9): DateTime, BitacoraLogin, FechaHora, Id, IpOrigen, Metodo, Resultado, UserAgent (+1 more)

### Community 50 - "LoginFacialDTO"
Cohesion: 0.29
Nodes (6): List, EnrolarFacialDTO, Descriptor, LoginFacialDTO, Descriptor, RecaptchaToken

### Community 51 - "ActualizarPerfilDTO"
Cohesion: 0.29
Nodes (7): ActualizarPerfilDTO, FotoBase64, FotoModificadaBase64, MetodoNotificacion, NuevoPassword, PasswordActual, Telefono

### Community 52 - "AuthResponseDTO"
Cohesion: 0.29
Nodes (7): AuthResponseDTO, CodigoQr, Expiracion, FotoModificada, Nickname, Rol, Token

### Community 54 - "FilterSelector.vue"
Cohesion: 0.50
Nodes (4): emit, props, seleccionar(), stickers

### Community 55 - "StickerSelector.vue"
Cohesion: 0.67
Nodes (3): emit, seleccionar(), stickers

### Community 57 - "AuditMiddleware"
Cohesion: 0.25
Nodes (6): ILogger, Task, AuditMiddleware, LenguajesFormalesAPI.Middleware, HttpContext, RequestDelegate

### Community 58 - "WhatsApp con Evolution API en Render"
Cohesion: 0.29
Nodes (6): 1. Desplegar en Render (≈ 10 min), 2. Crear la instancia y vincular el número, 3. Pegar las claves en el backend, 4. Probar, Notas, WhatsApp con Evolution API en Render

## Knowledge Gaps
- **332 isolated node(s):** `Idioma`, `Contenido`, `NombreArchivo`, `Token`, `Frecuencia` (+327 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 418 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **5 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `AppDbContext` connect `AppDbContext` to `Usuario`, `DashboardController`, `FotografiaUsuario`, `ResultadoAnalisis`, `AnalisisController`, `AuthService`, `BitacoraLogin`, `FacialService`, `LenguajesFormalesAPI.Models`?**
  _High betweenness centrality (0.045) - this node is a cross-community bridge._
- **Why does `LenguajesFormalesAPI.DTOs` connect `LenguajesFormalesAPI.Services` to `AnalisisLexicoService`, `AnalisisDTOs.cs`, `AuthDTOs.cs`?**
  _High betweenness centrality (0.043) - this node is a cross-community bridge._
- **Why does `AnalisisResultadoDTO` connect `AnalisisResultadoDTO` to `AnalisisLexicoService`, `AnalisisDTOs.cs`, `HistorialAnalisisDTO`, `AnalisisController`?**
  _High betweenness centrality (0.038) - this node is a cross-community bridge._
- **What connects `Idioma`, `Contenido`, `NombreArchivo` to the rest of the system?**
  _332 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `AuthController` be split into smaller, more focused modules?**
  _Cohesion score 0.14623655913978495 - nodes in this community are weakly interconnected._
- **Should `package.json` be split into smaller, more focused modules?**
  _Cohesion score 0.05180388529139685 - nodes in this community are weakly interconnected._
- **Should `RegistrationForm.vue` be split into smaller, more focused modules?**
  _Cohesion score 0.050505050505050504 - nodes in this community are weakly interconnected._