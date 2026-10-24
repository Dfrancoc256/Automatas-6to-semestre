# Guía de estudio: Analizador Léxico UMG

Esta guía explica el proyecto a partir del código que existe en el repositorio. La meta es que puedas entenderlo, explicarlo en clase y relacionar cada parte con su tecnología.

## 0. Idea general

El sistema permite que una persona se registre, inicie sesión y analice un archivo de texto en uno de cinco idiomas: español, inglés, ruso, chino o árabe. La API tokeniza el contenido y usa un **Autómata Finito Determinista (AFD)** para reconocer palabras pertenecientes a categorías léxicas, como pronombres, verbos, conectores y preposiciones.

También incluye historial de análisis, reportes para supervisión, gestión de usuarios, perfil, reconocimiento facial, credenciales PDF con QR y notificaciones.

```text
Navegador
  │
  ▼
Frontend Nuxt / Vue
  │  solicitudes HTTP (/api)
  ▼
Nginx ───────────────► API ASP.NET Core (.NET 8)
                              │
                  ┌───────────┼─────────────┐
                  ▼           ▼             ▼
             PostgreSQL   archivos       servicios externos
                          de fotos       correo, Google,
                                         reCAPTCHA, WhatsApp
```

---

## Fase 1. Conocer la estructura del repositorio

| Carpeta o archivo | Responsabilidad |
|---|---|
| `frontend/` | Aplicación que usa la persona en el navegador. |
| `backend/` | API, reglas de negocio, seguridad, AFD y acceso a datos. |
| `backend/database/schema.sql` | Esquema de referencia de PostgreSQL. |
| `infra/nginx/` | Configuración del proxy inverso y cabeceras de seguridad. |
| `docker-compose.yml` | Define cómo se ejecutan frontend, backend y Nginx en contenedores. |
| `.env.production` | Variables privadas del servidor. No se sube al repositorio. |

**Pregunta para estudiar:** ¿Por qué separar frontend y backend?

**Respuesta:** el frontend se concentra en la experiencia visual; el backend protege la lógica, las credenciales, la base de datos y las reglas de seguridad. Ambos se pueden desplegar o modificar sin mezclar responsabilidades.

---

## Fase 2. Frontend: lo que ve el usuario

### Tecnologías

| Tecnología | Tipo | Uso dentro del proyecto |
|---|---|---|
| Vue 3 | Biblioteca de interfaz | Componentes reactivos, eventos, `ref`, `computed` y plantillas. |
| Nuxt 3 | Framework sobre Vue | Rutas por archivos, layouts, middleware, configuración y compilación. |
| TypeScript | Lenguaje | Tipado para reducir errores en el frontend. |
| Vuetify 3 | Framework de componentes UI | Tablas, botones, formularios, diálogos, alertas y tarjetas. |
| Pinia | Gestión de estado | Conserva la sesión y datos del usuario en `frontend/stores/auth.ts`. |
| Axios | Cliente HTTP | Envía solicitudes del frontend a la API. |
| face-api.js | Biblioteca de visión | Obtiene el descriptor facial en el navegador. |
| Canvas API | API nativa del navegador | Aplica filtros y stickers a la foto de perfil. |
| Vite | Herramienta de construcción | Nuxt la usa para desarrollo y empaquetado rápido. |

### Dónde empezar a leer

1. `frontend/nuxt.config.ts`: configuración principal de Nuxt, URL de API, estilos globales y módulos.
2. `frontend/layouts/dashboard.vue`: estructura permanente del panel, menú lateral y barra superior.
3. `frontend/pages/login.vue`: inicio de sesión, reCAPTCHA, Google y reconocimiento facial.
4. `frontend/pages/dashboard/index.vue`: panel principal y métricas.
5. `frontend/pages/dashboard/analisis.vue`: carga de texto y vista de resultados del AFD.
6. `frontend/stores/auth.ts`: estado de autenticación en el navegador.
7. `frontend/composables/useApi.ts`: configuración reutilizable para llamar a la API.

### Cómo funciona una página Nuxt

Nuxt convierte la ubicación de un archivo dentro de `pages/` en una ruta:

| Archivo | URL |
|---|---|
| `pages/index.vue` | `/` |
| `pages/login.vue` | `/login` |
| `pages/dashboard/index.vue` | `/dashboard` |
| `pages/dashboard/analisis.vue` | `/dashboard/analisis` |

Ejemplo conceptual:

```ts
const { api } = useApi()
const { data } = await api.get('/analisis/historial')
```

El navegador llama a `/api/analisis/historial`; Nginx lo redirige al backend y el resultado se muestra reactivamente en Vue.

**Pregunta para estudiar:** ¿Vue y Nuxt son lo mismo?

**Respuesta:** no. Vue crea componentes reactivos; Nuxt organiza una aplicación completa sobre Vue, aportando rutas, middleware, configuración, compilación y convenciones de proyecto.

---

## Fase 3. Backend: reglas y API

### Tecnologías

| Tecnología | Tipo | Uso dentro del proyecto |
|---|---|---|
| C# | Lenguaje | Implementación de servicios, controladores y modelos. |
| .NET 8 | Plataforma | Runtime y herramientas de ejecución. |
| ASP.NET Core | Framework web | API REST, controladores, middleware, autenticación y autorización. |
| Entity Framework Core | ORM | Convierte consultas C# en operaciones de base de datos. |
| Npgsql | Proveedor de EF Core | Conecta Entity Framework Core con PostgreSQL. |
| Swagger / OpenAPI | Documentación de API | Disponible en desarrollo para probar endpoints. |

### Estructura backend

| Carpeta | Función |
|---|---|
| `Controllers/` | Reciben solicitudes HTTP y devuelven respuestas. |
| `Services/` | Contienen las reglas de negocio. |
| `Models/` | Representan entidades persistidas, por ejemplo `Usuario`. |
| `DTOs/` | Definen los datos que entran o salen de la API. |
| `Data/` | Contiene `AppDbContext`, el puente hacia la base de datos. |
| `Middleware/` | Procesan cada solicitud antes o después de los controladores. |
| `Interfaces/` | Contratos de servicios para desacoplar componentes. |

### Punto de entrada

El archivo `backend/Program.cs` construye la aplicación. Allí se configuran:

1. PostgreSQL o una base temporal en memoria para desarrollo.
2. JWT y autenticación con Google.
3. CORS para permitir que Nuxt use la API.
4. Límite de solicitudes para autenticación y reconocimiento facial.
5. Servicios como `AuthService`, `AnalisisLexicoService`, correo, WhatsApp, QR y facial.
6. El orden de middleware.
7. Los controladores con `app.MapControllers()`.

### Ejemplo del recorrido de una solicitud

```text
POST /api/analisis
       │
       ▼
AnalisisController
       │ valida archivo, idioma y sesión
       ▼
AnalisisLexicoService
       │ tokeniza y ejecuta AFD
       ▼
ResultadoAnalisis + PostgreSQL
       │
       ▼
Respuesta JSON para Nuxt
```

**Pregunta para estudiar:** ¿por qué no colocar toda la lógica dentro de un controlador?

**Respuesta:** el controlador debe atender HTTP; el servicio debe resolver la lógica. Esto vuelve el código más reutilizable, más fácil de probar y más ordenado.

---

## Fase 4. Base de datos

La base de datos de producción es **PostgreSQL**. El backend la usa mediante Entity Framework Core y el proveedor Npgsql.

Entidades importantes:

| Entidad | Información que conserva |
|---|---|
| `Usuario` | Identidad, rol, correo, teléfono, contraseña cifrada, fotos y configuración. |
| `ResultadoAnalisis` | Archivo analizado, idioma, métricas, fecha y detalle del resultado. |
| `BitacoraLogin` | Resultado y método de cada acceso. La IP se conserva internamente para auditoría, pero ya no se expone en la pantalla. |
| Tokens de restablecimiento | Datos temporales para recuperación de contraseña. |

Entity Framework permite escribir consultas como:

```csharp
var usuarios = await _db.Usuarios
    .Where(u => u.Activo)
    .ToListAsync();
```

El proveedor Npgsql convierte esa consulta a SQL para PostgreSQL.

---

## Fase 5. El AFD: núcleo académico

El AFD se encuentra en:

- `backend/Services/AutomataFinitoDeterminista.cs`
- `backend/Services/AnalisisLexicoService.cs`

### Definición formal

Un AFD se expresa como:

```text
M = (Q, Σ, δ, q0, F)
```

| Símbolo | En el proyecto |
|---|---|
| `Q` | Estados: inicio, prefijos creados desde el léxico, estados finales y estado pozo. |
| `Σ` | Alfabeto: caracteres que aparecen en las palabras del léxico del idioma. |
| `δ` | Función de transición: diccionario que lleva `(estado, símbolo)` a un único estado. |
| `q0` | Estado inicial `0`. |
| `F` | Estados finales asociados a una o más categorías léxicas. |

### Cómo se construye

1. El servicio contiene léxicos por idioma: pronombres, verbos, conectores, preposiciones, etc.
2. `CrearDesdeLexico` inserta cada palabra carácter por carácter como un **trie**.
3. Cada prefijo se vuelve un estado.
4. El último estado de una palabra se marca como final y conserva su categoría.
5. Se añade un **estado pozo**.
6. Toda transición faltante se dirige al estado pozo; por eso la función `δ` queda total y determinista.

Ejemplo simplificado para las palabras `yo` y `y`:

```text
q0 --y--> q1 (final: conector)
q0 --y--> q1 --o--> q2 (final: pronombre)
```

No hay varios destinos para la misma pareja `(estado, símbolo)`. Por eso es determinista. Tampoco hay transiciones epsilon.

### Ejecución

`Evaluar(entrada)` inicia en `q0`, lee un símbolo a la vez y aplica:

```text
estadoSiguiente = δ(estadoActual, símbolo)
```

Al terminar:

- Si el estado es final, el token queda aceptado y devuelve sus categorías.
- Si termina en el estado pozo o en un estado no final, no es una coincidencia del léxico.

Para chino existe `BuscarPrefijoMasLargo`, porque normalmente el texto no separa palabras con espacios. El método encuentra la palabra aceptada más larga que comienza en una posición dada.

**Pregunta para estudiar:** ¿por qué se utiliza AFD y no AFND?

**Respuesta:** la clasificación de cada token debe dar un recorrido único, rápido y explicable. No se necesitan caminos alternativos ni transiciones epsilon. Un AFD además facilita mostrar los estados recorridos en la interfaz.

> Importante: el AFD reconoce el léxico definido por el sistema. No pretende sustituir un analizador morfológico completo de lenguaje natural.

---

## Fase 6. Autenticación y seguridad

| Mecanismo | Qué protege |
|---|---|
| JWT | Representa una sesión firmada. Se recibe mediante cookie `HttpOnly` o encabezado `Authorization`. |
| BCrypt | Genera hashes de contraseñas; el sistema nunca guarda texto plano. |
| Middleware de autorización | Restringe endpoints según sesión y rol. |
| `PasswordChangeRequiredMiddleware` | Obliga a sustituir la contraseña temporal antes de usar el sistema. |
| Google OAuth | Permite entrar o registrarse con identidad verificada por Google. |
| reCAPTCHA | Reduce registros e inicios de sesión automatizados. |
| Rate limiting | Limita intentos de autenticación y facial por IP y período. |
| CORS | Limita qué orígenes pueden usar la API desde un navegador. |
| Auditoría | Registra actividad relevante del servidor. |

Flujo de contraseña temporal:

```text
Registro tradicional
  → backend genera contraseña temporal segura
  → BCrypt guarda su hash
  → MailKit envía la contraseña por correo
  → usuario inicia sesión
  → debeCambiarPassword = true
  → middleware limita el acceso
  → usuario define contraseña personal
  → acceso normal
```

---

## Fase 7. Servicios complementarios

| Función | Tecnología o servicio |
|---|---|
| Correo y contraseña temporal | MailKit / SMTP mediante `EmailService`. |
| Credencial PDF | QuestPDF mediante `CredentialService`. |
| Código QR | QRCoder, integrado en la credencial. |
| WhatsApp | Servicio `WhatsAppService` que usa una API configurable. |
| Fotos | Archivos en `backend/uploads`; Docker los conserva con el volumen `backend_uploads`. |
| Reconocimiento facial | `face-api.js` calcula descriptor en el cliente; backend lo persiste y compara mediante `FacialService`. |

La personalización visual de una foto se hace con Canvas en el navegador. El servidor recibe la foto final, por lo que no gasta CPU del VPS aplicando filtros.

---

## Fase 8. Infraestructura y despliegue

El archivo `docker-compose.yml` define tres contenedores:

| Contenedor | Rol |
|---|---|
| `frontend` | Compila y ejecuta Nuxt. |
| `backend` | Ejecuta la API de .NET en el puerto interno 8080. |
| `nginx` | Es el único expuesto al exterior; entrega frontend y reenvía `/api` al backend. |

Nginx funciona como **proxy inverso**: el usuario no entra directamente al backend. Esto centraliza rutas, cabeceras de seguridad y la publicación del sitio.

Variables sensibles se proporcionan en el VPS mediante `.env.production`, por ejemplo:

- cadena de conexión a PostgreSQL;
- secreto JWT;
- claves de reCAPTCHA;
- credenciales de Google OAuth;
- credenciales SMTP;
- configuración de WhatsApp.

Nunca debes subir ese archivo ni secretos a GitHub.

Flujo de actualización en el VPS:

```bash
cd /opt/proyecto_automatas
git pull origin main
sudo docker compose --env-file .env.production up --build -d
```

---

## Fase 9. Guion breve para exponer el proyecto

1. **Problema:** se necesita analizar texto en varios idiomas y clasificar elementos léxicos en un entorno académico.
2. **Solución:** aplicación web con frontend Nuxt, API .NET y AFD para reconocer un léxico controlado.
3. **Flujo:** usuario se autentica, carga texto, el backend tokeniza y ejecuta el AFD, guarda resultado y muestra historial.
4. **AFD:** cada palabra recorre estados definidos por sus caracteres; si finaliza en un estado de aceptación, se clasifica.
5. **Seguridad:** JWT, BCrypt, reCAPTCHA, límite de intentos, roles y cambio obligatorio de contraseña temporal.
6. **Despliegue:** Docker Compose y Nginx en VPS, con PostgreSQL y variables de entorno privadas.
7. **Valor:** combina teoría de lenguajes formales con una solución usable, segura y responsiva.

---

## Plan de estudio sugerido

| Sesión | Objetivo | Tiempo recomendado |
|---|---|---|
| 1 | Recorrer carpetas y explicar la arquitectura general. | 30 min |
| 2 | Comprender Nuxt, Vue, rutas y el dashboard. | 45 min |
| 3 | Comprender API, controladores, servicios y DTOs. | 45 min |
| 4 | Revisar PostgreSQL, entidades y EF Core. | 30 min |
| 5 | Estudiar `AutomataFinitoDeterminista.cs` paso a paso. | 60 min |
| 6 | Repasar JWT, BCrypt, OAuth, CAPTCHA y middleware. | 45 min |
| 7 | Explicar Docker, Nginx y el despliegue en el VPS. | 30 min |
| 8 | Ensayar la exposición usando la Fase 9. | 30 min |

## Ejercicios prácticos

1. Sigue una petición desde `frontend/pages/dashboard/analisis.vue` hasta `AnalisisController`.
2. Agrega mentalmente una palabra al léxico y explica qué estados nuevos produciría el trie.
3. Describe qué ocurriría si se elimina el estado pozo del AFD.
4. Explica por qué una contraseña no se guarda con texto plano.
5. Dibuja el flujo desde el navegador hasta PostgreSQL y de regreso.
6. Explica qué problema resuelve Nginx aunque ya exista una API funcional.
