# Graph Report - Automatas-6to-semestre  (2026-09-11)

## Corpus Check
- 54 files · ~180,727 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 558 nodes · 792 edges · 47 communities (40 shown, 4 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 51 edges (avg confidence: 0.84)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `852b28e1`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- AuthController
- package.json
- UsuarioPerfilDTO
- ResultadoAnalisis
- http
- AnalisisResultadoDTO
- LenguajesFormalesAPI.Models
- Usuario
- registro.vue
- LenguajesFormalesAPI.Services
- analisis.vue
- Usuario
- DashboardController
- LenguajesFormalesAPI.csproj
- perfil.vue
- AnalisisController
- FacialService
- historial.vue
- graphify-out/ knowledge graph store
- pages/index.vue
- AnalisisDTOs.cs
- AuditMiddleware
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
- AuthResponseDTO
- Task
- AuthDTOs.cs
- RegisterDTO
- AppDbContext
- BitacoraLogin
- LoginFacialDTO
- ActualizarPerfilDTO

## God Nodes (most connected - your core abstractions)
1. `AnalisisResultadoDTO` - 30 edges
2. `Usuario` - 21 edges
3. `AuthResponseDTO` - 17 edges
4. `BitacoraLogin` - 16 edges
5. `ResultadoAnalisis` - 16 edges
6. `RegisterDTO` - 15 edges
7. `AuthController` - 14 edges
8. `AppDbContext` - 14 edges
9. `Usuario` - 14 edges
10. `AuthService` - 14 edges

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

## Communities (47 total, 4 thin omitted)

### Community 0 - "AuthController"
Cohesion: 0.15
Nodes (18): AllowAnonymous, Authorize, HttpGet, HttpPost, IActionResult, ILogger, Task, AuthController (+10 more)

### Community 1 - "package.json"
Cohesion: 0.05
Nodes (36): cargarModelos(), obtenerDescriptor(), useFacialRecognition(), dependencies, axios, face-api.js, @mdi/font, nuxt (+28 more)

### Community 2 - "UsuarioPerfilDTO"
Cohesion: 0.18
Nodes (11): DateTime, UsuarioPerfilDTO, Correo, FechaRegistro, FotoModificada, Id, MetodoNotificacion, Nickname (+3 more)

### Community 3 - "ResultadoAnalisis"
Cohesion: 0.18
Nodes (11): DateTime, ResultadoAnalisis, DetalleJson, FechaAnalisis, Id, Idioma, NombreArchivo, TotalPalabras (+3 more)

### Community 4 - "http"
Cohesion: 0.07
Nodes (28): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, applicationUrl (+20 more)

### Community 5 - "AnalisisResultadoDTO"
Cohesion: 0.08
Nodes (25): List, AnalisisResultadoDTO, Adjetivos, AnalisisId, Conectores, CorreosEncontrados, FechaAnalisis, FechasEncontradas (+17 more)

### Community 7 - "Usuario"
Cohesion: 0.14
Nodes (12): Usuario, CredentialService, ICredentialService, DateTime, IConfiguration, IJwtService, JwtService, Claim (+4 more)

### Community 8 - "registro.vue"
Cohesion: 0.09
Nodes (21): { api }, auth, canvasEl, capturarFoto(), cargando, { cargarModelos, obtenerDescriptor }, descargando, descriptorFacial (+13 more)

### Community 9 - "LenguajesFormalesAPI.Services"
Cohesion: 0.08
Nodes (19): DateTime, MigrationBuilder, DateTime, ModelBuilder, InitialCreate, MigrationBuilder, DateTime, ModelBuilder (+11 more)

### Community 10 - "analisis.vue"
Cohesion: 0.12
Nodes (16): { api }, archivo, arrastrando, cargarArchivo(), categorias, contenido, errorMsg, fileInput (+8 more)

### Community 11 - "Usuario"
Cohesion: 0.11
Nodes (18): DateTime, Usuario, Activo, BitacoraLogins, Correo, EncodingFacial, FechaNacimiento, FechaRegistro (+10 more)

### Community 12 - "DashboardController"
Cohesion: 0.39
Nodes (7): Authorize, HttpGet, IActionResult, Task, CambiarRolDTO, DashboardController, HttpPatch

### Community 13 - "LenguajesFormalesAPI.csproj"
Cohesion: 0.18
Nodes (10): net8.0, BCrypt.Net-Next (4.0.3), iTextSharp (5.5.13.4), Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0), Microsoft.AspNetCore.OpenApi (8.0.22), Microsoft.EntityFrameworkCore.Design (8.0.0), Npgsql.EntityFrameworkCore.PostgreSQL (8.0.10), QRCoder (1.6.0) (+2 more)

### Community 14 - "perfil.vue"
Cohesion: 0.10
Nodes (18): { api }, auth, camaraFacialActiva, cancelarEnrolamiento(), canvasEnrolEl, capturarYEnrolar(), { cargarModelos, obtenerDescriptor }, detenerCamaraEnrol() (+10 more)

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

### Community 19 - "pages/index.vue"
Cohesion: 0.11
Nodes (17): accesoExitoso, { api }, auth, canvasFacialEl, cargando, { cargarModelos, obtenerDescriptor }, detenerCamaraFacial(), error (+9 more)

### Community 20 - "AnalisisDTOs.cs"
Cohesion: 0.25
Nodes (7): AnalisisRequestDTO, Contenido, Idioma, NombreArchivo, FrecuenciaToken, Frecuencia, Token

### Community 21 - "AuditMiddleware"
Cohesion: 0.25
Nodes (6): ILogger, Task, AuditMiddleware, LenguajesFormalesAPI.Middleware, HttpContext, RequestDelegate

### Community 22 - "usuarios.vue"
Cohesion: 0.25
Nodes (4): { api }, cargando, headers, usuarios

### Community 23 - "HistorialAnalisisDTO"
Cohesion: 0.29
Nodes (7): DateTime, HistorialAnalisisDTO, FechaAnalisis, Id, Idioma, NombreArchivo, TotalPalabras

### Community 24 - "AnalisisLexicoService"
Cohesion: 0.38
Nodes (4): HashSet, AnalisisLexicoService, IAnalisisLexicoService, Regex

### Community 25 - "dashboard.vue"
Cohesion: 0.29
Nodes (4): auth, route, sidebarCollapsed, sidebarOpen

### Community 26 - "compilerOptions"
Cohesion: 0.29
Nodes (6): compilerOptions, noImplicitOverride, noUncheckedIndexedAccess, strict, extends, ./.nuxt/tsconfig.json

### Community 27 - "bitacora.vue"
Cohesion: 0.40
Nodes (4): { api }, cargando, headers, registros

### Community 28 - "dashboard/index.vue"
Cohesion: 0.40
Nodes (3): { api }, auth, stats

### Community 29 - "reportes.vue"
Cohesion: 0.50
Nodes (3): { api }, metricas, stats

### Community 30 - "Universidad Mariano Gálvez de Guatemala Official Seal (Logo)"
Cohesion: 0.67
Nodes (3): Frontend Branding / Header Asset Usage, Universidad Mariano Gálvez de Guatemala Official Seal (Logo), Universidad Mariano Gálvez de Guatemala (UMG)

### Community 31 - "Campus Entrance Gate 1 / Guard Booth"
Cohesion: 1.00
Nodes (3): Campus Entrance Gate 1 / Guard Booth, UMG Campus Entrance Photo (Gate 1 / Parking 7), Universidad Mariano Gálvez Main Building

### Community 32 - "UMG Campus Aerial Background Image (fondo-umg.png)"
Cohesion: 0.67
Nodes (3): UMG Campus Aerial Background Image (fondo-umg.png), Site Background / Landing Page Visual Asset, Universidad Mariano Gálvez (UMG) Campus

### Community 39 - "AuthResponseDTO"
Cohesion: 0.22
Nodes (10): AuthResponseDTO, CodigoQr, Expiracion, FotoModificada, Nickname, Rol, Token, ILogger (+2 more)

### Community 40 - "Task"
Cohesion: 0.24
Nodes (6): LoginDTO, Identificador, Password, RecaptchaToken, Task, IAuthService

### Community 41 - "AuthDTOs.cs"
Cohesion: 0.20
Nodes (8): CambiarPasswordDTO, NuevoPassword, Token, LoginQrDTO, CodigoQr, RecaptchaToken, ResetPasswordDTO, Correo

### Community 42 - "RegisterDTO"
Cohesion: 0.20
Nodes (10): RegisterDTO, Correo, Descriptor, FechaNacimiento, FotoBase64, MetodoNotificacion, Nickname, Password (+2 more)

### Community 43 - "AppDbContext"
Cohesion: 0.22
Nodes (8): ModelBuilder, Usuario, AppDbContext, BitacoraLogins, ResultadosAnalisis, Usuarios, DbContext, DbSet

### Community 44 - "BitacoraLogin"
Cohesion: 0.22
Nodes (9): DateTime, BitacoraLogin, FechaHora, Id, IpOrigen, Metodo, Resultado, UserAgent (+1 more)

### Community 45 - "LoginFacialDTO"
Cohesion: 0.29
Nodes (6): List, EnrolarFacialDTO, Descriptor, LoginFacialDTO, Descriptor, RecaptchaToken

### Community 46 - "ActualizarPerfilDTO"
Cohesion: 0.29
Nodes (7): ActualizarPerfilDTO, FotoBase64, FotoModificadaBase64, MetodoNotificacion, NuevoPassword, PasswordActual, Telefono

## Knowledge Gaps
- **258 isolated node(s):** `Idioma`, `Contenido`, `NombreArchivo`, `Token`, `Frecuencia` (+253 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 321 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **4 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `LenguajesFormalesAPI.Data` connect `LenguajesFormalesAPI.Services` to `FacialService`, `LenguajesFormalesAPI.Models`?**
  _High betweenness centrality (0.059) - this node is a cross-community bridge._
- **Why does `AppDbContext` connect `AppDbContext` to `ResultadoAnalisis`, `LenguajesFormalesAPI.Models`, `Usuario`, `AuthResponseDTO`, `DashboardController`, `BitacoraLogin`, `AnalisisController`, `FacialService`?**
  _High betweenness centrality (0.056) - this node is a cross-community bridge._
- **Why does `AnalisisResultadoDTO` connect `AnalisisResultadoDTO` to `AnalisisLexicoService`, `AnalisisDTOs.cs`, `HistorialAnalisisDTO`, `AnalisisController`?**
  _High betweenness centrality (0.051) - this node is a cross-community bridge._
- **Are the 3 inferred relationships involving `BitacoraLogin` (e.g. with `.LoginAsync()` and `.LoginFacialAsync()`) actually correct?**
  _`BitacoraLogin` has 3 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Idioma`, `Contenido`, `NombreArchivo` to the rest of the system?**
  _258 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `package.json` be split into smaller, more focused modules?**
  _Cohesion score 0.05110336817653891 - nodes in this community are weakly interconnected._
- **Should `http` be split into smaller, more focused modules?**
  _Cohesion score 0.07389162561576355 - nodes in this community are weakly interconnected._