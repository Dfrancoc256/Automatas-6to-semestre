# QA - Fase 1

Fecha: 4 de agosto de 2026

## Alcance

- Comparación del proyecto con el documento de requisitos.
- Compilación del backend .NET 8 y del frontend Nuxt 3.
- Revisión inicial de configuración y seguridad.
- Pruebas unitarias del servicio de análisis léxico.
- El proyecto Java se utilizó únicamente como referencia durante el diagnóstico inicial y fue retirado al consolidar la reescritura .NET/Nuxt.

## Resultado verificable

- Backend: compila sin errores.
- Frontend: compilación de producción completada.
- Dependencias frontend: 0 vulnerabilidades reportadas por `npm audit` durante `npm ci`.
- Pruebas unitarias backend: 4 aprobadas, 0 fallidas.

## Correcciones realizadas

- Se reemplazaron las URLs internas de Replit en `package-lock.json` por el registro público de npm.
- Se retiraron del archivo versionado la conexión remota y la clave privada JWT.
- Se agregó un ejemplo local de configuración sin secretos reales.
- El bypass de reCAPTCHA quedó desactivado por defecto y limitado explícitamente al ambiente Development.
- Se agregaron límites y formatos de validación para teléfono, contraseña, método de notificación, idioma, contenido y nombre de archivo.
- Se corrigió el espacio final que devolvía la detección de horas.
- Se corrigió el conteo de oraciones para contenido vacío.
- Se agregó el proyecto `backend.Tests` con cobertura inicial de contenido vacío, español, patrones y Unicode.

## Riesgos y brechas pendientes

1. La clave y contraseña retiradas deben rotarse en el servidor porque estuvieron expuestas en archivos del proyecto.
2. iTextSharp 5 genera una advertencia de compatibilidad con .NET 8; debe sustituirse durante depuración/refactorización.
3. El frontend aún envía `dev-bypass`; debe integrarse el widget real de reCAPTCHA antes de producción.
4. El administrador inicial conserva una contraseña conocida dentro del seed/migración; debe migrarse a un proceso seguro de aprovisionamiento.
5. Faltan pruebas de integración de controladores, autenticación, autorización y persistencia.
6. El proyecto Java legado ya no forma parte del producto activo; la validación se concentra en .NET 8 y Nuxt 3.

## Retiro del legado Java

- Se verificó que backend y frontend no dependían de clases, recursos ni plantillas Java.
- Se eliminó la carpeta `biometrico-umg` completa: 237 archivos, aproximadamente 1.6 MB.
- Se retiraron referencias a Maven, Spring Boot y al árbol de archivos obsoleto.
- La implementación oficial queda formada por `backend` (.NET 8/C#), `frontend` (Nuxt 3/Vue 3/Vuetify 3) y `backend.Tests`.
7. PostgreSQL es una tecnología equivalente, pero debe quedar aceptada formalmente si la evaluación exige literalmente MySQL, Oracle o SQL Server.
8. Siguen pendientes Docker, TLS de despliegue, reconocimiento facial, PDF/QR, notificaciones y los artefactos UML/arquitectura/entrega señalados por el documento.

## Comandos de reproducción

```powershell
dotnet build backend/LenguajesFormalesAPI.csproj
$env:DOTNET_ROLL_FORWARD='Major'
dotnet test backend.Tests/LenguajesFormalesAPI.Tests.csproj
cd frontend
npm ci
npm run build
```

El avance mayor de runtime solo fue necesario en esta estación porque dispone de .NET 10, no del runtime .NET 8. El entorno oficial debe instalar .NET 8.

## Fase 2 - Depuración y limpieza

- Se normalizan correo y nickname antes de consultar o registrar usuarios.
- Se limita el tamaño de IP y User-Agent antes de escribir la bitácora.
- Los errores inesperados de login se registran internamente y producen un mensaje público genérico.
- La paginación de bitácora queda acotada a valores válidos y a un máximo de 100 registros.
- Los cambios de rol aceptan mayúsculas/minúsculas de forma consistente y rechazan valores nulos.
- La auditoría se ejecuta incluso cuando un componente posterior lanza una excepción.
- Las fotografías base64 tienen límites de entrada para reducir abuso de memoria.
- La restauración de sesión del frontend elimina datos corruptos o incompletos.
- `dev-bypass` ya no se envía en compilaciones de producción.

Verificación posterior a la fase 2:

- Backend y proyecto de pruebas: compilación correcta.
- Pruebas unitarias: 4 aprobadas, 0 fallidas.
- Frontend Nuxt/Nitro: compilación de producción correcta.
- Advertencias pendientes: compatibilidad de iTextSharp 5 y un paquete/chunk principal del frontend mayor de 500 kB.

## Fase 3 - Primer bloque funcional

- Matriz completa de cumplimiento creada en `MATRIZ_CUMPLIMIENTO.md`.
- Login QR implementado mediante credenciales JWT firmadas y separadas de los tokens de sesión.
- Validación de usuario activo y registro de bitácora para accesos QR.
- Límite de 10 intentos por minuto e IP en registro y login.
- Validación temprana de conexión y clave JWT; la aplicación falla de forma explícita ante configuración insegura.
- Manejo global de excepciones, cabeceras de proxy y Swagger limitado a Development.
- Generación y descarga de credencial PDF con QR desde el registro.
- Pruebas ampliadas a 8 aprobadas, 0 fallidas.
- Compilación de producción Nuxt/Nitro aprobada después de los cambios.

## Retiro de configuración Replit

- Eliminados `.replit`, `replit.md` y `.cache/replit`.
- Retirados el módulo Java de Replit, sus workflows, webview y mapeos de puertos.
- Nuxt usa `NUXT_HOST` y `NUXT_PORT`, con `localhost:5000` como valor local seguro.
- ASP.NET usa `ASPNETCORE_URLS`, con `http://localhost:8080` como valor local.
- Se añadió `.env.example` sin credenciales reales y un `README.md` neutral con instrucciones de compilación.
- Backend: 8 pruebas aprobadas, 0 fallidas.
- Frontend: compilación de producción aprobada sin depender de Replit.
