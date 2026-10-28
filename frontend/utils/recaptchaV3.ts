type RecaptchaV3 = {
  ready: (callback: () => void) => void
  execute: (siteKey: string, options: { action: string }) => Promise<string>
}

let carga: Promise<RecaptchaV3> | null = null

function cargarRecaptcha(siteKey: string): Promise<RecaptchaV3> {
  if (carga) return carga

  carga = new Promise((resolve, reject) => {
    const script = document.createElement('script')
    script.src = `https://www.google.com/recaptcha/api.js?render=${encodeURIComponent(siteKey)}`
    script.async = true
    script.onerror = () => {
      script.remove()
      carga = null
      reject(new Error('No se pudo cargar la verificación de seguridad. Intenta nuevamente.'))
    }
    script.onload = () => {
      const recaptcha = (window as Window & { grecaptcha?: RecaptchaV3 }).grecaptcha
      if (!recaptcha) {
        carga = null
        reject(new Error('No se pudo iniciar la verificación de seguridad.'))
        return
      }
      recaptcha.ready(() => resolve(recaptcha))
    }
    document.head.appendChild(script)
  })

  return carga
}

export async function obtenerTokenRecaptcha(siteKey: string, accion: string): Promise<string> {
  if (!siteKey) throw new Error('Falta configurar la clave pública de reCAPTCHA v3.')
  const recaptcha = await cargarRecaptcha(siteKey)
  const token = await recaptcha.execute(siteKey, { action: accion })
  if (!token) throw new Error('No se pudo completar la verificación de seguridad. Intenta nuevamente.')
  return token
}

export function prepararRecaptcha(siteKey: string): void {
  if (siteKey) void cargarRecaptcha(siteKey).catch(() => {
    // El siguiente envío volverá a intentar la carga y mostrará el error si falla.
  })
}
