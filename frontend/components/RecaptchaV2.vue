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
  interface Window { grecaptcha?: GrecaptchaApi }
}

const props = defineProps<{ siteKey: string }>()
const emit = defineEmits<{ 'update:token': [token: string] }>()

const contenedor = ref<HTMLElement | null>(null)
const mensaje = ref('')
let widgetId: number | null = null
let cargador: Promise<void> | null = null

function cargarApi() {
  if (window.grecaptcha) return Promise.resolve()
  if (cargador) return cargador

  cargador = new Promise((resolve, reject) => {
    const script = document.createElement('script')
    script.src = 'https://www.google.com/recaptcha/api.js?render=explicit&hl=es'
    script.async = true
    script.defer = true
    script.onload = () => resolve()
    script.onerror = () => reject(new Error('No se pudo cargar reCAPTCHA.'))
    document.head.appendChild(script)
  })

  return cargador
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
