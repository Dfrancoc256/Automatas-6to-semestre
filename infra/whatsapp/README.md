# WhatsApp con Evolution API en Render

El backend ya está listo (`WhatsAppService`, `POST /api/whatsapp/send`, aviso de registro).
Solo falta **desplegar Evolution API** y **pegar 3 valores** en el backend.

Evolution API usa el protocolo de WhatsApp Web (Baileys): es **no oficial**. Úsalo con un
número dedicado/secundario, no para spam, y escribe solo a quienes se registran en la plataforma.

## 1. Desplegar en Render (≈ 10 min)

1. Entra a https://dashboard.render.com con tu cuenta de GitHub y agrega un método de pago.
2. **New → Blueprint** → elige el repo `Automatas-6to-semestre` → rama `cristian` (o la que uses)
   → **Blueprint Path:** `infra/whatsapp/render.yaml`.
3. Render te pedirá `SERVER_URL`: escribe `https://evolution-api-umg.onrender.com`
   (si Render te asigna otro nombre, corrígelo después en *Environment*).
4. **Apply**. Se crean 3 recursos: `evolution-api-umg` (web), `evolution-redis` y `evolution-db`.
   La primera vez tarda varios minutos (migra la base de datos al arrancar).
5. Cuando el servicio diga **Live**, abre `https://<tu-url>.onrender.com` → debe responder un JSON de bienvenida.
6. En *evolution-api-umg → Environment* copia el valor de **AUTHENTICATION_API_KEY** (será tu `ApiKey`).

## 2. Crear la instancia y vincular el número

Reemplaza `URL` y `CLAVE`:

```bash
curl -X POST URL/instance/create \
  -H "apikey: CLAVE" -H "Content-Type: application/json" \
  -d '{"instanceName":"umg-lenguajes","qrcode":true,"integration":"WHATSAPP-BAILEYS"}'
```

Para ver el QR: abre `URL/manager` (inicia sesión con la misma `CLAVE`), entra a la instancia
`umg-lenguajes` y pulsa *Connect*. En el celular: **WhatsApp → Dispositivos vinculados → Vincular un dispositivo**
y escanea el QR (dura ~30 s; si caduca, vuelve a pulsar *Connect*).

Comprobar estado: `GET URL/instance/connectionState/umg-lenguajes` (header `apikey`) → `"state":"open"`.

## 3. Pegar las claves en el backend

Desde la carpeta `backend/` (se guardan fuera del repo, en *user-secrets*):

```bash
dotnet user-secrets set "WhatsApp:BaseUrl" "https://TU-URL.onrender.com"
dotnet user-secrets set "WhatsApp:ApiKey" "LA_CLAVE_DE_RENDER"
```

`WhatsApp:InstanceName` ya viene como `umg-lenguajes` en `appsettings.json`
(cámbialo solo si usaste otro nombre en el paso 2).
Si despliegas el backend, define las mismas variables de entorno:
`WhatsApp__BaseUrl`, `WhatsApp__ApiKey`, `WhatsApp__InstanceName`.

## 4. Probar

1. Reinicia el backend, entra como ADMIN y llama (Swagger o curl con tu sesión):
   `POST /api/whatsapp/send` con `{"to":"5555-1234","message":"Prueba"}`.
2. Regístrate con método de notificación **whatsapp** o **ambos**: llega el mensaje de bienvenida.

## Notas

- Los teléfonos de 8 dígitos se envían como Guatemala (`502`); cambia `WhatsApp:DefaultCountryCode` si hace falta.
- Si falta configuración o Evolution está caída, **el registro no falla**: solo se registra un aviso en el log.
- Si la sesión se cae (celular sin internet muchos días, número cerrado), repite el QR del paso 2.
- Ahorro: en vez de `evolution-db` puedes usar un Postgres propio (crea una base `evolution`, quita el bloque
  `databases` del Blueprint y define `DATABASE_CONNECTION_URI` a mano).
- Revisa precios actuales en https://render.com/pricing (web *starter* + Postgres *basic* son de pago).
