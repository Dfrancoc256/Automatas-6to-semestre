<template>
  <div class="qr-scanner">
    <div class="qr-scanner__viewport">
      <video ref="video" autoplay muted playsinline aria-label="Cámara para escanear el código QR del carnet" />
      <span class="qr-scanner__frame" aria-hidden="true" />
    </div>
    <p v-if="estado" class="text-body-2 text-medium-emphasis mt-3 mb-0">{{ estado }}</p>
  </div>
</template>

<script setup lang="ts">
import { BrowserQRCodeReader } from '@zxing/browser'

const emit = defineEmits<{
  detectado: [codigo: string]
  error: [mensaje: string]
}>()

const video = ref<HTMLVideoElement | null>(null)
const estado = ref('Preparando cámara…')
let controles: { stop: () => void } | undefined
let lector: BrowserQRCodeReader | undefined
let detectado = false

onMounted(async () => {
  if (!navigator.mediaDevices?.getUserMedia) {
    emit('error', 'Tu navegador no permite acceder a la cámara para escanear el QR.')
    return
  }

  try {
    lector = new BrowserQRCodeReader()
    controles = await lector.decodeFromConstraints(
      { audio: false, video: { facingMode: { ideal: 'environment' } } },
      video.value ?? undefined,
      (resultado) => {
        if (!resultado || detectado) return
        detectado = true
        controles?.stop()
        estado.value = 'Código detectado. Verificando carnet…'
        emit('detectado', resultado.getText())
      }
    )
    estado.value = 'Enfoca el código QR de tu carnet dentro del recuadro.'
  } catch {
    emit('error', 'No fue posible abrir la cámara. Revisa los permisos e inténtalo nuevamente.')
  }
})

onBeforeUnmount(() => controles?.stop())
</script>

<style scoped>
.qr-scanner { text-align: center; }
.qr-scanner__viewport {
  position: relative;
  width: min(100%, 360px);
  aspect-ratio: 1;
  margin: 0 auto;
  overflow: hidden;
  border-radius: 14px;
  background: #071d2e;
}
.qr-scanner video { width: 100%; height: 100%; object-fit: cover; }
.qr-scanner__frame {
  position: absolute;
  inset: 14%;
  border: 2px solid #e2b635;
  border-radius: 12px;
  box-shadow: 0 0 0 999px rgba(5, 27, 46, .22), 0 0 18px rgba(226, 182, 53, .5);
  pointer-events: none;
}
</style>
