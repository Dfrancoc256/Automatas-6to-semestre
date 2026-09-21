<template>
  <div class="face-scanner" :class="[`face-scanner--${estado}`]" role="group" aria-label="Escáner facial">
    <video ref="videoEl" class="face-scanner__video" autoplay playsinline muted />

    <!-- Marco guía: el óvalo recorta el fondo oscurecido para indicar dónde colocar el rostro -->
    <div class="face-scanner__marco" aria-hidden="true">
      <div class="face-scanner__ovalo">
        <div class="face-scanner__barrido"><span /></div>
      </div>
      <span class="face-scanner__esquina face-scanner__esquina--tl" />
      <span class="face-scanner__esquina face-scanner__esquina--tr" />
      <span class="face-scanner__esquina face-scanner__esquina--bl" />
      <span class="face-scanner__esquina face-scanner__esquina--br" />
    </div>

    <div v-if="estado === 'ok'" class="face-scanner__exito" aria-hidden="true">
      <v-icon size="54" color="white">mdi-check-circle</v-icon>
    </div>

    <p class="face-scanner__mensaje" role="status" aria-live="polite">
      <v-icon size="16" class="mr-1">{{ icono }}</v-icon>{{ mensaje }}
    </p>
  </div>
</template>

<script setup lang="ts">
/**
 * Cámara con guía de encuadre y animación de escaneo.
 *
 * - Modo automático (`auto`): al detectar un rostro bien encuadrado y estable calcula el
 *   descriptor y emite `capturado` sin que el usuario tenga que pulsar nada.
 * - Modo manual (`auto = false`): solo guía el encuadre; el padre llama a `capturar()`.
 *
 * La cámara se abre al montar el componente y se apaga al desmontarlo (usar v-if).
 */
const props = withDefaults(defineProps<{
  auto?: boolean
  conFoto?: boolean   // incluir la foto (JPEG base64) junto al descriptor
  ocupado?: boolean   // el padre está procesando el resultado (p. ej. llamando a la API)
}>(), { auto: true, conFoto: false, ocupado: false })

interface CapturaFacial { descriptor: number[] | null, foto: string | null }

const emit = defineEmits<{
  capturado: [captura: CapturaFacial]
  error: [mensaje: string]
}>()

const { calentar, detectarRostro, obtenerDescriptor } = useFacialRecognition()

type Estado = 'iniciando' | 'buscando' | 'ajustando' | 'listo' | 'escaneando' | 'ok'

const videoEl = ref<HTMLVideoElement | null>(null)
const estado = ref<Estado>('iniciando')
const pista = ref('')
let stream: MediaStream | null = null
let activo = false
let pausado = false
let buenosSeguidos = 0

// Cuántos cuadros seguidos bien encuadrados se piden antes de capturar (evita capturas borrosas).
const CUADROS_ESTABLES = 2
const INTERVALO_MS = 40

const mensaje = computed(() => {
  if (props.ocupado) return 'Verificando identidad…'
  switch (estado.value) {
    case 'iniciando': return 'Iniciando cámara…'
    case 'buscando': return 'Coloca tu rostro dentro del marco'
    case 'ajustando': return pista.value || 'Ajusta tu posición'
    case 'listo': return props.auto ? 'No te muevas…' : 'Rostro listo: puedes capturar'
    case 'escaneando': return 'Escaneando rostro…'
    case 'ok': return 'Rostro capturado'
  }
})

const icono = computed(() => {
  if (props.ocupado || estado.value === 'escaneando') return 'mdi-face-recognition'
  if (estado.value === 'ok') return 'mdi-check'
  if (estado.value === 'listo') return 'mdi-check-circle-outline'
  if (estado.value === 'ajustando') return 'mdi-arrow-expand-all'
  return 'mdi-face-man-shimmer-outline'
})

// Proporción del video que ocupa el óvalo guía (ver CSS): el rostro debe caer dentro.
function evaluarEncuadre(caja: { x: number, y: number, ancho: number, alto: number }, video: HTMLVideoElement) {
  const anchoV = video.videoWidth
  const altoV = video.videoHeight
  const relAncho = caja.ancho / anchoV
  const dx = (caja.x + caja.ancho / 2) / anchoV - 0.5
  const dy = (caja.y + caja.alto / 2) / altoV - 0.5

  if (relAncho < 0.28) return 'Acércate un poco'
  if (relAncho > 0.56) return 'Aléjate un poco'
  if (Math.abs(dx) > 0.14 || Math.abs(dy) > 0.16) return 'Centra tu rostro en el marco'
  return null
}

function tomarFoto(video: HTMLVideoElement): string | null {
  if (!props.conFoto) return null
  const lienzo = document.createElement('canvas')
  lienzo.width = 640
  lienzo.height = 480
  lienzo.getContext('2d')!.drawImage(video, 0, 0, lienzo.width, lienzo.height)
  return lienzo.toDataURL('image/jpeg', 0.8)
}

async function calcularCaptura(): Promise<CapturaFacial | null> {
  const video = videoEl.value
  if (!video || !video.videoWidth) return null
  const descriptor = await obtenerDescriptor(video)
  // La foto se toma justo después del cálculo para que coincida con el rostro evaluado.
  return { descriptor, foto: tomarFoto(video) }
}

async function ciclo() {
  while (activo) {
    const video = videoEl.value
    if (pausado || !video || !video.videoWidth || video.paused) {
      await new Promise(r => setTimeout(r, 100))
      continue
    }

    let caja = null
    try {
      caja = await detectarRostro(video)
    } catch {
      await new Promise(r => setTimeout(r, 250))
      continue
    }
    if (!activo) return

    if (!caja) {
      buenosSeguidos = 0
      estado.value = 'buscando'
    } else {
      const problema = evaluarEncuadre(caja, video)
      if (problema) {
        buenosSeguidos = 0
        pista.value = problema
        estado.value = 'ajustando'
      } else {
        buenosSeguidos++
        estado.value = 'listo'
        if (props.auto && buenosSeguidos >= CUADROS_ESTABLES) {
          pausado = true
          estado.value = 'escaneando'
          try {
            const captura = await calcularCaptura()
            if (captura?.descriptor) {
              estado.value = 'ok'
              emit('capturado', captura)
            } else {
              buenosSeguidos = 0
              pausado = false
            }
          } catch {
            buenosSeguidos = 0
            pausado = false
          }
          continue
        }
      }
    }
    await new Promise(r => setTimeout(r, INTERVALO_MS))
  }
}

/** Modo manual: calcula el descriptor del cuadro actual. La foto se devuelve aunque no haya rostro. */
async function capturar(): Promise<CapturaFacial | null> {
  const video = videoEl.value
  if (!video || !video.videoWidth) return null
  pausado = true
  estado.value = 'escaneando'
  try {
    const captura = await calcularCaptura()
    estado.value = captura?.descriptor ? 'ok' : 'buscando'
    return captura
  } finally {
    pausado = false
  }
}

/** Vuelve a buscar un rostro (p. ej. tras un intento fallido en el servidor). */
function reiniciar() {
  buenosSeguidos = 0
  estado.value = 'buscando'
  pausado = false
}

async function iniciarCamara() {
  try {
    stream = await navigator.mediaDevices.getUserMedia({
      video: { facingMode: 'user', width: { ideal: 640 }, height: { ideal: 480 } }
    })
    if (!activo) { stream.getTracks().forEach(t => t.stop()); return }
    if (videoEl.value) videoEl.value.srcObject = stream
  } catch {
    emit('error', 'No se pudo acceder a la cámara. Revisa los permisos del navegador.')
    return
  }
  // Los modelos se cargan en paralelo con el permiso de cámara; aquí ya deberían estar listos.
  await calentar()
  if (!activo) return
  estado.value = 'buscando'
  ciclo()
}

onMounted(() => {
  activo = true
  iniciarCamara()
})

onBeforeUnmount(() => {
  activo = false
  stream?.getTracks().forEach(t => t.stop())
  stream = null
})

defineExpose({ capturar, reiniciar })
</script>

<style scoped>
.face-scanner {
  --fs-color: #B48B21;
  position: relative;
  width: 100%;
  max-width: 380px;
  margin: 0 auto;
  aspect-ratio: 4 / 3;
  overflow: hidden;
  border-radius: 14px;
  background: #050d16;
  box-shadow: 0 10px 30px rgba(5, 27, 46, .28);
}
.face-scanner--ajustando { --fs-color: #F5A524; }
.face-scanner--listo,
.face-scanner--escaneando,
.face-scanner--ok { --fs-color: #2FBF71; }

.face-scanner__video {
  position: absolute; inset: 0;
  width: 100%; height: 100%;
  object-fit: cover;
  transform: scaleX(-1); /* efecto espejo, más natural al encuadrarse */
}

.face-scanner__marco {
  position: absolute; inset: 0;
  display: flex; align-items: center; justify-content: center;
  padding-bottom: 8%; /* deja libre la franja del mensaje inferior */
  pointer-events: none;
}

/* Óvalo guía: su sombra gigante oscurece todo lo que queda fuera */
.face-scanner__ovalo {
  position: relative;
  height: 74%;
  aspect-ratio: 4 / 5;
  border: 3px solid var(--fs-color);
  border-radius: 50%;
  overflow: hidden;
  box-shadow: 0 0 0 9999px rgba(5, 13, 22, .58), 0 0 18px 2px color-mix(in srgb, var(--fs-color) 55%, transparent), inset 0 0 22px color-mix(in srgb, var(--fs-color) 30%, transparent);
  transition: border-color .25s ease, box-shadow .25s ease;
}

/* Línea de escaneo que recorre el óvalo, con estela luminosa */
.face-scanner__barrido { position: absolute; inset: 0; }
.face-scanner__barrido span {
  position: absolute; left: 0; right: 0; top: 0;
  height: 42%;
  background: linear-gradient(to bottom, transparent, color-mix(in srgb, var(--fs-color) 42%, transparent));
  border-bottom: 2px solid var(--fs-color);
  box-shadow: 0 6px 14px color-mix(in srgb, var(--fs-color) 70%, transparent);
  transform: translateY(-100%);
  animation: fs-barrido 2.2s cubic-bezier(.45, 0, .55, 1) infinite;
  opacity: .9;
}
.face-scanner--escaneando .face-scanner__barrido span,
.face-scanner--listo .face-scanner__barrido span { animation-duration: 1.1s; }
.face-scanner--iniciando .face-scanner__barrido span,
.face-scanner--ok .face-scanner__barrido span { animation: none; opacity: 0; }

@keyframes fs-barrido {
  0%   { transform: translateY(-100%); }
  100% { transform: translateY(240%); }
}

/* Esquinas tipo visor alrededor del óvalo */
.face-scanner__esquina {
  position: absolute;
  width: 26px; height: 26px;
  border: 0 solid var(--fs-color);
  transition: border-color .25s ease;
}
.face-scanner__esquina--tl { top: calc(9% - 7px);    left: calc(50% - 22.2% - 7px); border-top-width: 3px;    border-left-width: 3px;  border-top-left-radius: 8px; }
.face-scanner__esquina--tr { top: calc(9% - 7px);    right: calc(50% - 22.2% - 7px); border-top-width: 3px;    border-right-width: 3px; border-top-right-radius: 8px; }
.face-scanner__esquina--bl { bottom: calc(17% - 7px); left: calc(50% - 22.2% - 7px); border-bottom-width: 3px; border-left-width: 3px;  border-bottom-left-radius: 8px; }
.face-scanner__esquina--br { bottom: calc(17% - 7px); right: calc(50% - 22.2% - 7px); border-bottom-width: 3px; border-right-width: 3px; border-bottom-right-radius: 8px; }
.face-scanner--escaneando .face-scanner__esquina { animation: fs-pulso .55s ease-in-out infinite alternate; }
@keyframes fs-pulso { from { transform: scale(1); } to { transform: scale(1.12); } }

.face-scanner__exito {
  position: absolute; inset: 0;
  display: flex; align-items: center; justify-content: center;
  background: rgba(47, 191, 113, .32);
  animation: fs-exito .35s ease-out;
}
@keyframes fs-exito { from { opacity: 0; transform: scale(.94); } to { opacity: 1; transform: scale(1); } }

.face-scanner__mensaje {
  position: absolute; left: 50%; bottom: 10px;
  transform: translateX(-50%);
  display: flex; align-items: center;
  margin: 0; padding: 5px 13px;
  max-width: 92%;
  white-space: nowrap;
  color: #fff;
  font-size: 12.5px; font-weight: 600;
  background: rgba(5, 13, 22, .72);
  border: 1px solid color-mix(in srgb, var(--fs-color) 70%, transparent);
  border-radius: 999px;
  backdrop-filter: blur(3px);
}

@media (prefers-reduced-motion: reduce) {
  .face-scanner__barrido span,
  .face-scanner__esquina { animation: none !important; }
}
</style>
