# Matriz de cumplimiento del proyecto

Fuente: `PROYECTO FINAL LENGUAJES FORMALES 2026.docx`

Estados: **Cumple**, **Parcial**, **Pendiente** o **No verificable**.

| Área | Requisito | Estado actual | Trabajo requerido |
|---|---|---:|---|
| Arquitectura | Capas BD + backend + frontend | Cumple | Mantener separación y agregar documentación de arquitectura. |
| Frontend | Nuxt 3, Vue 3, Vuetify 3, TypeScript | Cumple | Completar pruebas y optimizar bundle. |
| Backend | .NET/Core 8 y C# | Cumple | Instalar runtime 8 en el entorno final. |
| Base de datos | MySQL, Oracle, SQL Server o equivalente | Parcial | PostgreSQL funciona como equivalente; documentar/confirmar aceptación. |
| Registro | Correo, teléfono, nacimiento, nickname y contraseña | Cumple | Añadir pruebas de integración. |
| Registro | Fotografía mediante cámara | Parcial | Captura implementada; falta reconocimiento/recorte facial. |
| Registro | Filtros o stickers | Pendiente | Crear editor no destructivo y guardar original/modificada. |
| Registro | Método email, WhatsApp o ambos | Parcial | Preferencia persistida; envío real pendiente. |
| Registro | Contraseña cifrada | Cumple | BCrypt implementado; agregar política más fuerte y rate limiting. |
| Registro | Credencial PDF con QR | Parcial | PDF, QR firmado y descarga implementados; envío automático pendiente. |
| Registro | Rol ANALISTA automático | Cumple | Cubrir con prueba de integración. |
| Perfil | Cambiar contraseña y fotografía | Parcial | Cambio implementado; mejorar almacenamiento/limpieza de imágenes. |
| Login | Usuario y contraseña | Cumple | Añadir rate limiting y pruebas de integración. |
| Login | Fotografía/reconocimiento facial | Pendiente | Implementar proveedor y endpoint seguro. |
| Login | Lectura de QR | Cumple | Endpoint, validación criptográfica, usuario activo y bitácora implementados. |
| Login | Recuperación de contraseña | Pendiente | Crear tokens de un solo uso, expiración y notificación. |
| reCAPTCHA | Validación en registro/login | Parcial | Backend preparado; falta widget/token real en producción. |
| JWT | Consumo de endpoints | Cumple | Clave mínima validada y tokens de sesión/QR separados por tipo. |
| Roles | ADMIN, SUPERVISOR y ANALISTA | Cumple | Agregar pruebas negativas de autorización. |
| Auditoría | Quién, cuándo, IP, origen y resultado | Parcial | Login se registra; ampliar auditoría persistente a acciones sensibles. |
| Análisis | Cargar archivos `.txt` | Cumple | Validar tamaño también antes de `FileReader` y codificación. |
| Análisis | Elegir 3 de 5 idiomas | Cumple | Actualmente permite los cinco. |
| Análisis | Total y frecuencias | Cumple | Ampliar pruebas por idioma. |
| Análisis | Pronombres, nombres, sustantivos y verbos raíz | Parcial | Usa diccionarios/heurísticas; falta lematización lingüística real. |
| Análisis | Otros patrones | Cumple | Detecta correo, URL, fecha, hora, números y adjetivos. |
| Reporte | PDF por correo/teléfono | Pendiente | Generación y notificación pendientes. |
| Seguridad | Validar/sanitizar entradas | Parcial | DataAnnotations y límites iniciales; faltan pruebas e inspección de archivos/imágenes. |
| Seguridad | No exponer secretos | Parcial | Secretos retirados; deben rotarse y configurarse desde entorno. |
| Seguridad | TLS/HTTPS entre servicios | Pendiente | Configurar proxy/certificados y redirección segura. |
| Errores | Mensajes claros sin información sensible | Cumple | Controladores principales y manejador global producen respuestas seguras. |
| UI/UX | Español, responsiva y regla 60-30-10 | Cumple | Ejecutar pruebas visuales en navegadores y móviles. |
| Navegadores | Chrome, Firefox y Edge | No verificable | Ejecutar matriz de compatibilidad/E2E. |
| Docker | Backend y frontend virtualizados | Pendiente | Crear Dockerfiles, compose y health checks. |
| Pruebas | Unitarias, caja blanca y caja negra con evidencia | Parcial | Hay 4 unitarias; faltan integración, seguridad, frontend y evidencias. |
| Documentación | UML, ER, flujos, casos de uso y arquitectura | Pendiente | Generar artefactos versionados. |
| Entrega | Postman y guías de compilación/API | Pendiente | Crear colección y manuales reproducibles. |
| Entrega | Presentación, costos, manual y video | Pendiente | Preparar después del cierre funcional. |

## Orden de implementación

1. Autenticación segura: QR, recuperación, reCAPTCHA, rate limiting y bootstrap administrativo.
2. Credencial PDF/QR, editor de fotografía y notificaciones.
3. Análisis léxico y reporte multidioma.
4. Docker/TLS/configuración y manejo global de errores.
5. Pruebas integrales y documentación de entrega.
