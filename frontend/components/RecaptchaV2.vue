<template>
  <div class="recaptcha-control">
    <div ref="contenedor" />
    <p v-if="mensaje" class="recaptcha-control__message">{{ mensaje }}</p>
  </div>
</template>

<script setup lang="ts">
interface GrecaptchaApi {
  render: (element: HTMLElement, options: Record<string, unknown>) => number
  reset: (widgetId?: number) => void
}

declare global {
  interface Window {
    grecaptcha?: GrecaptchaApi
    /** Promesa compartida para no insertar el script de Google más de una vez. */
    __umgRecaptchaLoader?: Promise<void>
  }
}

const props = defineProps<{ siteKey: string }>()
const emit = defineEmits<{ 'update:token': [token: string] }>()

const contenedor = ref<HTMLElement | null>(null)
const mensaje = ref('')
let widgetId: number | null = null
let activo = true

const esperar = (milisegundos: number) => new Promise<void>((resolve) => {
  window.setTimeout(resolve, milisegundos)
})

function reiniciarCarga() {
  document.querySelector('script[data-umg-recaptcha="true"]')?.remove()
  window.__umgRecaptchaLoader = undefined
}

/** El token de reCAPTCHA se usa una sola vez; permite al formulario pedir uno nuevo. */
function reiniciar() {
  mensaje.value = ''
  emit('update:token', '')
  if (widgetId !== null && window.grecaptcha) window.grecaptcha.reset(widgetId)
}

function cargarApi(): Promise<void> {
  if (window.grecaptcha?.render) return Promise.resolve()
  if (window.__umgRecaptchaLoader) return window.__umgRecaptchaLoader

  window.__umgRecaptchaLoader = new Promise((resolve, reject) => {
    const scriptExistente = document.querySelector<HTMLScriptElement>(
      'script[data-umg-recaptcha="true"]'
    )

    const resolver = () => resolve()
    const rechazar = () => reject(new Error('No se pudo cargar reCAPTCHA.'))

    if (scriptExistente) {
      scriptExistente.addEventListener('load', resolver, { once: true })
      scriptExistente.addEventListener('error', rechazar, { once: true })
      return
    }

    const script = document.createElement('script')
    script.src = 'https://www.google.com/recaptcha/api.js?render=explicit&hl=es'
    script.async = true
    script.defer = true
    script.dataset.umgRecaptcha = 'true'
    script.addEventListener('load', resolver, { once: true })
    script.addEventListener('error', rechazar, { once: true })
    document.head.appendChild(script)
  })

  // Permite reintentar si la red bloqueó la primera carga.
  window.__umgRecaptchaLoader.catch(() => {
    reiniciarCarga()
  })

  return window.__umgRecaptchaLoader
}

/**
 * Google puede responder antes de exponer `grecaptcha` en la primera carga.
 * Esperamos la API y reintentamos dos veces para no exigir al usuario cambiar
 * de formulario, especialmente en redes móviles lentas.
 */
async function cargarApiConReintentos() {
  let ultimoError: unknown

  for (let intento = 1; intento <= 3; intento += 1) {
    try {
      await cargarApi()

      for (let espera = 0; espera < 10; espera += 1) {
        if (window.grecaptcha?.render) return
        await esperar(100)
      }

      throw new Error('La API de reCAPTCHA no quedó disponible.')
    } catch (error) {
      ultimoError = error
      reiniciarCarga()
      if (intento < 3) await esperar(500 * intento)
    }
  }

  throw ultimoError
}

onMounted(async () => {
  try {
    await cargarApiConReintentos()
    if (!activo || !contenedor.value || !window.grecaptcha) return

    widgetId = window.grecaptcha.render(contenedor.value, {
      sitekey: props.siteKey,
      callback: (token: string) => emit('update:token', token),
      'expired-callback': () => emit('update:token', ''),
      'error-callback': () => {
        emit('update:token', '')
        mensaje.value = 'No fue posible validar reCAPTCHA. Intenta nuevamente.'
      }
    })
  } catch {
    if (activo) mensaje.value = 'No fue posible cargar reCAPTCHA. Revisa tu conexión e intenta nuevamente.'
  }
})

onBeforeUnmount(() => {
  activo = false
  if (widgetId !== null && window.grecaptcha) window.grecaptcha.reset(widgetId)
})

defineExpose({ reiniciar })
</script>

<style scoped>
.recaptcha-control { display: flex; justify-content: center; min-height: 78px; }
.recaptcha-control__message { margin: 4px 0 0; color: #b42318; font-size: 12px; text-align: center; }
</style>
