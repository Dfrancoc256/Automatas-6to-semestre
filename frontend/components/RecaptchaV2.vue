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
    document.querySelector('script[data-umg-recaptcha="true"]')?.remove()
    window.__umgRecaptchaLoader = undefined
  })

  return window.__umgRecaptchaLoader
}

onMounted(async () => {
  try {
    await cargarApi()
    if (!contenedor.value || !window.grecaptcha) return

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
    mensaje.value = 'No fue posible cargar reCAPTCHA. Revisa tu conexión e intenta nuevamente.'
  }
})

onBeforeUnmount(() => {
  if (widgetId !== null && window.grecaptcha) window.grecaptcha.reset(widgetId)
})
</script>

<style scoped>
.recaptcha-control { display: flex; justify-content: center; min-height: 78px; }
.recaptcha-control__message { margin: 4px 0 0; color: #b42318; font-size: 12px; text-align: center; }
</style>
