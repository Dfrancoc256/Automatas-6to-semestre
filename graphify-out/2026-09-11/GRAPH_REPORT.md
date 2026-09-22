# Graph Report - Automatas-6to-semestre  (2026-09-11)

## Corpus Check
- 49 files · ~178,672 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 485 nodes · 655 edges · 39 communities (33 shown, 3 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 39 edges (avg confidence: 0.84)
- Token cost: 268,698 input · 0 output

## Community Hubs (Navigation)
- Auth Backend (Controller/Service/DTOs)
- Frontend Package Dependencies
- Profile & Password Auth DTOs
- DB Context & Core Models
- Backend Launch Settings
- Analysis Result DTO Fields
- Backend Core & Credential/PDF Service
- JWT Token Service
- Registration Page (Frontend)
- Database Migrations
- Analysis Dashboard Page (Frontend)
- Usuario Model
- Dashboard Controller (Backend)
- Backend Project & NuGet Deps
- Profile Page (Frontend)
- Analysis Controller (Backend)
- Recaptcha Service
- History Page (Frontend)
- Graphify Tooling Reference (CLAUDE.md)
- Login Page (Frontend)
- Analysis Request DTO
- Audit Middleware
- Users Management Page (Frontend)
- Analysis History DTO
- Lexical Analysis Service
- Dashboard Layout (Frontend)
- TypeScript Config
- Login Audit / Bitacora Page (Frontend)
- Dashboard Home Page (Frontend)
- Reports Page (Frontend)
- UMG Logo & Branding Image
- Campus Entrance Gate Image
- Campus Background Image
- Auth Store (Frontend/Pinia)
- Campus Aerial Photo
- Campus Golden-Hour Photo

## God Nodes (most connected - your core abstractions)
1. `AnalisisResultadoDTO` - 30 edges
2. `Usuario` - 20 edges
3. `ResultadoAnalisis` - 16 edges
4. `AuthResponseDTO` - 15 edges
5. `BitacoraLogin` - 15 edges
6. `AuthController` - 13 edges
7. `RegisterDTO` - 13 edges
8. `AppDbContext` - 13 edges
9. `UsuarioPerfilDTO` - 12 edges
10. `Usuario` - 12 edges

## Surprising Connections (you probably didn't know these)
- `AnalisisController` --references--> `AppDbContext`  [EXTRACTED]
  backend/Controllers/AnalisisController.cs → backend/Data/AppDbContext.cs
- `AnalisisController` --references--> `IAnalisisLexicoService`  [EXTRACTED]
  backend/Controllers/AnalisisController.cs → backend/Services/AnalisisLexicoService.cs
- `AuthController` --references--> `ICredentialService`  [EXTRACTED]
  backend/Controllers/AuthController.cs → backend/Services/CredentialService.cs
- `AuthController` --references--> `IJwtService`  [EXTRACTED]
  backend/Controllers/AuthController.cs → backend/Services/JwtService.cs
- `AuthController` --references--> `IRecaptchaService`  [EXTRACTED]
  backend/Controllers/AuthController.cs → backend/Services/RecaptchaService.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Codebase Question Resolution Priority Flow** — claude_graphify_query, claude_graphify_path, claude_graphify_explain, claude_wiki_index_md, claude_graph_report_md [EXTRACTED 1.00]

## Communities (39 total, 3 thin omitted)

### Community 0 - "Auth Backend (Controller/Service/DTOs)"
Cohesion: 0.09
Nodes (29): AllowAnonymous, Authorize, HttpGet, HttpPost, IActionResult, ILogger, Task, AuthController (+21 more)

### Community 1 - "Frontend Package Dependencies"
Cohesion: 0.06
Nodes (31): dependencies, axios, @mdi/font, nuxt, pinia, @pinia/nuxt, vite-plugin-vuetify, vue (+23 more)

### Community 2 - "Profile & Password Auth DTOs"
Cohesion: 0.06
Nodes (34): DateTime, ActualizarPerfilDTO, FotoBase64, FotoModificadaBase64, MetodoNotificacion, NuevoPassword, PasswordActual, Telefono (+26 more)

### Community 3 - "DB Context & Core Models"
Cohesion: 0.07
Nodes (28): ModelBuilder, Usuario, AppDbContext, BitacoraLogins, ResultadosAnalisis, Usuarios, DateTime, BitacoraLogin (+20 more)

### Community 4 - "Backend Launch Settings"
Cohesion: 0.07
Nodes (28): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, applicationUrl (+20 more)

### Community 5 - "Analysis Result DTO Fields"
Cohesion: 0.08
Nodes (25): AnalisisResultadoDTO, Adjetivos, AnalisisId, Conectores, CorreosEncontrados, FechaAnalisis, FechasEncontradas, HorasEncontradas (+17 more)

### Community 6 - "Backend Core & Credential/PDF Service"
Cohesion: 0.15
Nodes (9): CredentialService, ICredentialService, LenguajesFormalesAPI.Data, LenguajesFormalesAPI.Models, LenguajesFormalesAPI.DTOs, LenguajesFormalesAPI.Controllers, LenguajesFormalesAPI.Services, Font (+1 more)

### Community 7 - "JWT Token Service"
Cohesion: 0.21
Nodes (8): Usuario, DateTime, IConfiguration, IJwtService, JwtService, Claim, ClaimsPrincipal, IEnumerable

### Community 8 - "Registration Page (Frontend)"
Cohesion: 0.11
Nodes (18): { api }, auth, canvasEl, capturarFoto(), cargando, descargando, detenerCamara(), error (+10 more)

### Community 9 - "Database Migrations"
Cohesion: 0.12
Nodes (11): DateTime, DateTime, ModelBuilder, InitialCreate, DateTime, ModelBuilder, AppDbContextModelSnapshot, LenguajesFormalesAPI.Migrations (+3 more)

### Community 10 - "Analysis Dashboard Page (Frontend)"
Cohesion: 0.12
Nodes (16): { api }, archivo, arrastrando, cargarArchivo(), categorias, contenido, errorMsg, fileInput (+8 more)

### Community 11 - "Usuario Model"
Cohesion: 0.12
Nodes (17): DateTime, Usuario, Activo, BitacoraLogins, Correo, FechaNacimiento, FechaRegistro, FotoModificada (+9 more)

### Community 12 - "Dashboard Controller (Backend)"
Cohesion: 0.32
Nodes (8): Authorize, HttpGet, IActionResult, Task, CambiarRolDTO, DashboardController, ControllerBase, HttpPatch

### Community 13 - "Backend Project & NuGet Deps"
Cohesion: 0.18
Nodes (10): net8.0, BCrypt.Net-Next (4.0.3), iTextSharp (5.5.13.4), Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0), Microsoft.AspNetCore.OpenApi (8.0.22), Microsoft.EntityFrameworkCore.Design (8.0.0), Npgsql.EntityFrameworkCore.PostgreSQL (8.0.10), QRCoder (1.6.0) (+2 more)

### Community 14 - "Profile Page (Frontend)"
Cohesion: 0.18
Nodes (8): { api }, auth, form, formRef, guardando, msgError, msgExito, perfil

### Community 15 - "Analysis Controller (Backend)"
Cohesion: 0.31
Nodes (7): HashSet, HttpGet, HttpPost, IActionResult, ILogger, Task, AnalisisController

### Community 16 - "Recaptcha Service"
Cohesion: 0.22
Nodes (8): IConfiguration, ILogger, Task, IRecaptchaService, RecaptchaService, Dictionary, IHostEnvironment, IHttpClientFactory

### Community 17 - "History Page (Frontend)"
Cohesion: 0.20
Nodes (7): { api }, cargando, detalle, detalleMet, dialogDetalle, headers, historial

### Community 18 - "Graphify Tooling Reference (CLAUDE.md)"
Cohesion: 0.33
Nodes (9): graphify-out/graph.json, graphify-out/GRAPH_REPORT.md, graphify (CLI tool), graphify explain command, graphify-out/ knowledge graph store, graphify path command, graphify query command, graphify update command (+1 more)

### Community 19 - "Login Page (Frontend)"
Cohesion: 0.22
Nodes (7): accesoExitoso, auth, cargando, error, form, mostrarPass, mostrarReset

### Community 20 - "Analysis Request DTO"
Cohesion: 0.25
Nodes (7): AnalisisRequestDTO, Contenido, Idioma, NombreArchivo, FrecuenciaToken, Frecuencia, Token

### Community 21 - "Audit Middleware"
Cohesion: 0.25
Nodes (6): ILogger, Task, AuditMiddleware, LenguajesFormalesAPI.Middleware, HttpContext, RequestDelegate

### Community 22 - "Users Management Page (Frontend)"
Cohesion: 0.25
Nodes (4): { api }, cargando, headers, usuarios

### Community 23 - "Analysis History DTO"
Cohesion: 0.29
Nodes (7): DateTime, HistorialAnalisisDTO, FechaAnalisis, Id, Idioma, NombreArchivo, TotalPalabras

### Community 24 - "Lexical Analysis Service"
Cohesion: 0.38
Nodes (4): HashSet, AnalisisLexicoService, IAnalisisLexicoService, Regex

### Community 25 - "Dashboard Layout (Frontend)"
Cohesion: 0.29
Nodes (4): auth, route, sidebarCollapsed, sidebarOpen

### Community 26 - "TypeScript Config"
Cohesion: 0.29
Nodes (6): compilerOptions, noImplicitOverride, noUncheckedIndexedAccess, strict, extends, ./.nuxt/tsconfig.json

### Community 27 - "Login Audit / Bitacora Page (Frontend)"
Cohesion: 0.40
Nodes (4): { api }, cargando, headers, registros

### Community 28 - "Dashboard Home Page (Frontend)"
Cohesion: 0.40
Nodes (3): { api }, auth, stats

### Community 29 - "Reports Page (Frontend)"
Cohesion: 0.50
Nodes (3): { api }, metricas, stats

### Community 30 - "UMG Logo & Branding Image"
Cohesion: 0.67
Nodes (3): Frontend Branding / Header Asset Usage, Universidad Mariano Gálvez de Guatemala Official Seal (Logo), Universidad Mariano Gálvez de Guatemala (UMG)

### Community 31 - "Campus Entrance Gate Image"
Cohesion: 1.00
Nodes (3): Campus Entrance Gate 1 / Guard Booth, UMG Campus Entrance Photo (Gate 1 / Parking 7), Universidad Mariano Gálvez Main Building

### Community 32 - "Campus Background Image"
Cohesion: 0.67
Nodes (3): UMG Campus Aerial Background Image (fondo-umg.png), Site Background / Landing Page Visual Asset, Universidad Mariano Gálvez (UMG) Campus

## Knowledge Gaps
- **236 isolated node(s):** `Idioma`, `Contenido`, `NombreArchivo`, `Token`, `Frecuencia` (+231 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 292 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `AnalisisResultadoDTO` connect `Analysis Result DTO Fields` to `Lexical Analysis Service`, `Analysis Request DTO`, `Analysis History DTO`, `Analysis Controller (Backend)`?**
  _High betweenness centrality (0.058) - this node is a cross-community bridge._
- **Why does `AppDbContext` connect `DB Context & Core Models` to `Auth Backend (Controller/Service/DTOs)`, `Backend Core & Credential/PDF Service`, `JWT Token Service`, `Dashboard Controller (Backend)`, `Analysis Controller (Backend)`?**
  _High betweenness centrality (0.058) - this node is a cross-community bridge._
- **Why does `LenguajesFormalesAPI.DTOs` connect `Backend Core & Credential/PDF Service` to `Lexical Analysis Service`, `Profile & Password Auth DTOs`, `Analysis Request DTO`?**
  _High betweenness centrality (0.054) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `BitacoraLogin` (e.g. with `.LoginAsync()` and `.LoginQrAsync()`) actually correct?**
  _`BitacoraLogin` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Idioma`, `Contenido`, `NombreArchivo` to the rest of the system?**
  _236 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Auth Backend (Controller/Service/DTOs)` be split into smaller, more focused modules?**
  _Cohesion score 0.09131205673758866 - nodes in this community are weakly interconnected._
- **Should `Frontend Package Dependencies` be split into smaller, more focused modules?**
  _Cohesion score 0.05555555555555555 - nodes in this community are weakly interconnected._