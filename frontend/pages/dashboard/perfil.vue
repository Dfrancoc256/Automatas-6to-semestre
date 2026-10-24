<template>
  <div>
    <div class="d-flex align-center mb-6">
      <v-icon color="accent" class="mr-3">mdi-account-edit</v-icon>
      <div>
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Mi Perfil</h1>
        <p class="text-body-2 text-medium-emphasis">Gestiona tu información personal</p>
      </div>
    </div>

    <v-row>
      <!-- Avatar y datos rápidos -->
      <v-col cols="12" md="4">
        <v-card class="pa-6 text-center">
          <v-avatar size="96" class="mb-4" color="accent">
            <v-img v-if="fotoPerfilUrl" :src="fotoPerfilUrl" eager />
            <span v-else class="text-white text-h4 font-weight-bold">
              {{ perfil?.nickname?.charAt(0)?.toUpperCase() }}
            </span>
          </v-avatar>
          <h2 class="text-h6 font-weight-bold mb-1">{{ perfil?.nickname }}</h2>
          <v-chip color="accent" size="small" variant="tonal" class="mb-3">{{ perfil?.rol }}</v-chip>
          <p class="text-caption text-medium-emphasis">
            Miembro desde {{ perfil ? formatFecha(perfil.fechaRegistro) : '—' }}
          </p>
        </v-card>
      </v-col>

      <!-- Formulario de edición -->
      <v-col cols="12" md="8">
        <v-card class="pa-6 mb-6">
          <div class="d-flex align-center justify-space-between mb-4">
            <h2 class="text-subtitle-1 font-weight-bold mb-0">Reconocimiento facial</h2>
            <v-chip v-if="perfil?.tieneRostroEnrolado" color="success" size="small" variant="tonal">
              <v-icon start size="16">mdi-check-decagram</v-icon>Enrolado
            </v-chip>
            <v-chip v-else color="warning" size="small" variant="tonal">
              <v-icon start size="16">mdi-alert</v-icon>Sin enrolar
            </v-chip>
          </div>

          <div v-if="!camaraFacialActiva" class="text-center">
            <v-img v-if="fotoPerfilUrl" :src="fotoPerfilUrl" eager
                   width="120" height="120" cover
                   class="mx-auto mb-3 rounded-circle facial-enrol-foto" />
            <p class="text-body-2 text-medium-emphasis mb-3">
              Enrola tu rostro para poder iniciar sesión con reconocimiento facial.
            </p>
            <v-btn color="primary" variant="outlined" prepend-icon="mdi-face-recognition"
                   @click="iniciarEnrolamiento">
              {{ perfil?.tieneRostroEnrolado ? 'Actualizar rostro' : 'Enrolar rostro' }}
            </v-btn>
            <v-alert v-if="msgFacial" :type="tipoMsgFacial" variant="tonal" density="compact" class="mt-3">
              {{ msgFacial }}
            </v-alert>
          </div>

          <div v-else-if="!capturaPendiente" class="text-center">
            <FaceScanner
              con-foto
              :ocupado="enrolando"
              class="mb-3"
              @capturado="prepararFoto"
              @error="alFallarCamara"
            />
            <p class="text-body-2 text-medium-emphasis mb-3">
              Coloca tu rostro dentro del marco. Se capturará automáticamente.
            </p>

            <v-alert v-if="msgFacial" :type="tipoMsgFacial" variant="tonal" density="compact" class="mb-3">
              {{ msgFacial }}
            </v-alert>

            <div class="d-flex gap-2 justify-center">
              <v-btn variant="outlined" @click="cancelarEnrolamiento">Cancelar</v-btn>
            </div>
          </div>

          <div v-else class="text-center">
            <p class="text-body-2 text-medium-emphasis mb-3">Personaliza la foto que aparecerá en tu perfil y credencial.</p>
            <div ref="vistaPreviaFoto" class="foto-preview mb-4">
              <img :src="capturaPendiente.fotoRecortada" :style="{ filter: filtroActual.css }"
                   class="foto-preview__imagen" alt="Vista previa de la credencial" />
              <img v-if="stickerActual !== 'ninguno'" :src="imagenesStickers[stickerActual]"
                   class="foto-preview__sticker"
                   :style="{ left: `${posicionSticker.x}px`, top: `${posicionSticker.y}px`, width: `${tamanoSticker}px` }"
                   alt="Adorno seleccionado" draggable="false" @pointerdown.prevent="iniciarArrastreSticker" />
            </div>
            <p class="text-caption font-weight-bold mb-2">Filtro de la fotografía</p>
            <div class="d-flex flex-wrap justify-center gap-2 mb-4">
              <v-btn v-for="filtro in filtros" :key="filtro.id" size="small"
                     :color="filtroActual.id === filtro.id ? 'accent' : 'primary'"
                     :variant="filtroActual.id === filtro.id ? 'flat' : 'outlined'"
                     :prepend-icon="filtro.icono" @click="filtroActual = filtro">
                {{ filtro.nombre }}
              </v-btn>
            </div>
            <div class="efectos-foto mb-4">
              <p class="text-caption font-weight-bold mb-2">Adorno opcional</p>
              <PhotoStickerSelector v-model="stickerActual" />
            </div>
            <p v-if="stickerActual !== 'ninguno'" class="text-caption text-medium-emphasis mb-4">
              Arrastra el icono sobre la foto para colocarlo donde prefieras.
            </p>
            <v-alert v-if="msgFacial" :type="tipoMsgFacial" variant="tonal" density="compact" class="mb-3">
              {{ msgFacial }}
            </v-alert>
            <div class="d-flex flex-wrap gap-2 justify-center">
              <v-btn variant="outlined" :disabled="enrolando" @click="repetirFoto">Repetir foto</v-btn>
              <v-btn variant="outlined" :disabled="enrolando" @click="cancelarEnrolamiento">Cancelar</v-btn>
              <v-btn color="accent" :loading="enrolando" @click="enrolar">Guardar rostro y foto</v-btn>
            </div>
          </div>
        </v-card>

        <v-card class="pa-6">
          <h2 class="text-subtitle-1 font-weight-bold mb-4">Actualizar información</h2>

          <v-form @submit.prevent="guardar" ref="formRef">
            <v-text-field :model-value="perfil?.correo" label="Correo"
                          prepend-inner-icon="mdi-email" readonly
                          variant="filled" class="mb-3" />
            <v-text-field v-model="form.telefono" label="Teléfono"
                          prepend-inner-icon="mdi-phone" class="mb-3" />
            <v-select v-model="form.metodoNotificacion"
                      label="Método de notificación"
                      prepend-inner-icon="mdi-bell"
                      :items="['email','whatsapp','ambos']"
                      class="mb-4" />

            <v-divider class="mb-4" />
            <h3 class="text-subtitle-2 font-weight-bold mb-3">Cambiar contraseña</h3>

            <v-text-field v-model="form.passwordActual" label="Contraseña actual"
                          prepend-inner-icon="mdi-lock-outline" type="password" class="mb-3" />
            <v-text-field v-model="form.nuevoPassword"
                          label="Nueva contraseña (mín. 8 caracteres)"
                          prepend-inner-icon="mdi-lock" type="password" class="mb-4"
                          :rules="form.nuevoPassword ? [v => v.length >= 8 || 'Mínimo 8'] : []" />

            <v-alert v-if="msgExito" type="success" variant="tonal" density="compact" class="mb-4">
              {{ msgExito }}
            </v-alert>
            <v-alert v-if="msgError" type="error" variant="tonal" density="compact" class="mb-4">
              {{ msgError }}
            </v-alert>

            <v-btn type="submit" color="accent" :loading="guardando"
                   prepend-icon="mdi-content-save" block>
              Guardar cambios
            </v-btn>
          </v-form>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: ['auth', 'role'], roles: ['ADMIN', 'SUPERVISOR', 'ANALISTA'] })
useHead({ title: 'Mi Perfil' })

const auth     = useAuthStore()
const { api }  = useApi()
const { calentar } = useFacialRecognition()
const { fotoUrl } = useMediaUrl()
const perfil   = ref<any>(null)
const fotoPerfilUrl = computed(() => fotoUrl(perfil.value?.fotoModificada))
const guardando = ref(false)
const msgExito = ref(''); const msgError = ref('')
const formRef  = ref<any>(null)
const form = reactive({
  telefono: '', metodoNotificacion: 'email',
  passwordActual: '', nuevoPassword: ''
})

// Enrolamiento facial
const camaraFacialActiva = ref(false)
const enrolando     = ref(false)
const msgFacial      = ref('')
const tipoMsgFacial  = ref<'success' | 'error'>('success')
const capturaPendiente = ref<{ descriptor: number[], fotoRecortada: string } | null>(null)
const vistaPreviaFoto = ref<HTMLElement | null>(null)
const stickerActual = ref('ninguno')
const posicionSticker = reactive({ x: 72, y: 12 })
const tamanoSticker = 42
const imagenesStickers: Record<string, string> = {
  graduacion: '/stickers/graduacion.png',
  lentes: '/stickers/lentes.png',
  corona: '/stickers/corona.png',
  estrella: '/stickers/estrella.png',
  corazon: '/stickers/corazon.png'
}
const filtros = [
  { id: 'normal', nombre: 'Natural', icono: 'mdi-image-filter-none', css: 'none', canvas: 'none' },
  { id: 'calido', nombre: 'Cálido', icono: 'mdi-white-balance-sunny', css: 'sepia(.28) saturate(1.15)', canvas: 'sepia(28%) saturate(115%)' },
  { id: 'azul', nombre: 'Azul UMG', icono: 'mdi-palette-outline', css: 'hue-rotate(175deg) saturate(1.1)', canvas: 'hue-rotate(175deg) saturate(110%)' },
  { id: 'gris', nombre: 'Clásico', icono: 'mdi-image-filter-black-white', css: 'grayscale(1) contrast(1.08)', canvas: 'grayscale(100%) contrast(108%)' }
]
const filtroActual = ref(filtros[0]!)
let arrastrandoSticker = false
let desplazamientoSticker = { x: 0, y: 0 }

onMounted(async () => {
  calentar()
  try {
    const { data } = await api.get('/auth/perfil')
    perfil.value = data
    form.telefono = data.telefono
    form.metodoNotificacion = data.metodoNotificacion
  } catch { /* ignore */ }
})

async function guardar() {
  msgExito.value = ''; msgError.value = ''
  guardando.value = true
  try {
    const payload: any = {
      telefono: form.telefono,
      metodoNotificacion: form.metodoNotificacion
    }
    if (form.passwordActual && form.nuevoPassword) {
      payload.passwordActual = form.passwordActual
      payload.nuevoPassword  = form.nuevoPassword
    }
    await api.put('/auth/perfil', payload)
    msgExito.value = 'Perfil actualizado correctamente.'
    form.passwordActual = ''; form.nuevoPassword = ''
  } catch (e: any) {
    msgError.value = e.response?.data?.mensaje || 'Error al guardar.'
  } finally { guardando.value = false }
}

function formatFecha(iso: string) {
  return new Date(iso).toLocaleDateString('es-GT', { day: '2-digit', month: 'long', year: 'numeric' })
}

function iniciarEnrolamiento() {
  msgFacial.value = ''
  capturaPendiente.value = null
  camaraFacialActiva.value = true
}

function cancelarEnrolamiento() {
  camaraFacialActiva.value = false
  capturaPendiente.value = null
  msgFacial.value = ''
  terminarArrastreSticker()
}

function repetirFoto() {
  capturaPendiente.value = null
  msgFacial.value = ''
  terminarArrastreSticker()
}

function prepararFoto(captura: { descriptor: number[] | null, fotoRecortada: string | null }) {
  if (!captura.descriptor || !captura.fotoRecortada) return
  capturaPendiente.value = { descriptor: captura.descriptor, fotoRecortada: captura.fotoRecortada }
  filtroActual.value = filtros[0]!
  stickerActual.value = 'ninguno'
  posicionSticker.x = 72
  posicionSticker.y = 12
  msgFacial.value = ''
}

function iniciarArrastreSticker(evento: PointerEvent) {
  const vista = vistaPreviaFoto.value
  if (!vista) return
  const limites = vista.getBoundingClientRect()
  arrastrandoSticker = true
  desplazamientoSticker = {
    x: evento.clientX - limites.left - posicionSticker.x,
    y: evento.clientY - limites.top - posicionSticker.y
  }
  window.addEventListener('pointermove', moverSticker)
  window.addEventListener('pointerup', terminarArrastreSticker, { once: true })
}

function moverSticker(evento: PointerEvent) {
  const vista = vistaPreviaFoto.value
  if (!arrastrandoSticker || !vista) return
  const limites = vista.getBoundingClientRect()
  posicionSticker.x = Math.max(0, Math.min(limites.width - tamanoSticker, evento.clientX - limites.left - desplazamientoSticker.x))
  posicionSticker.y = Math.max(0, Math.min(limites.height - tamanoSticker, evento.clientY - limites.top - desplazamientoSticker.y))
}

function terminarArrastreSticker() {
  arrastrandoSticker = false
  window.removeEventListener('pointermove', moverSticker)
}

onBeforeUnmount(terminarArrastreSticker)

function cargarImagen(origen: string): Promise<HTMLImageElement> {
  return new Promise((resolver, rechazar) => {
    const imagen = new Image()
    imagen.onload = () => resolver(imagen)
    imagen.onerror = () => rechazar(new Error('No se pudo cargar la imagen.'))
    imagen.src = origen
  })
}

async function crearFotoPersonalizada(fuente: string): Promise<string> {
  const imagen = await cargarImagen(fuente)
  const lienzo = document.createElement('canvas')
  lienzo.width = 360
  lienzo.height = 480
  const contexto = lienzo.getContext('2d')
  if (!contexto) throw new Error('No se pudo preparar la fotografía.')
  contexto.filter = filtroActual.value.canvas
  contexto.drawImage(imagen, 0, 0, lienzo.width, lienzo.height)
  contexto.filter = 'none'

  const rutaSticker = imagenesStickers[stickerActual.value]
  if (rutaSticker) {
    const sticker = await cargarImagen(rutaSticker)
    const anchoVista = vistaPreviaFoto.value?.clientWidth || 120
    const altoVista = vistaPreviaFoto.value?.clientHeight || 160
    contexto.drawImage(
      sticker,
      posicionSticker.x * lienzo.width / anchoVista,
      posicionSticker.y * lienzo.height / altoVista,
      tamanoSticker * lienzo.width / anchoVista,
      tamanoSticker * lienzo.height / altoVista
    )
  }
  return lienzo.toDataURL('image/jpeg', 0.85)
}

function alFallarCamara(mensaje: string) {
  msgFacial.value = mensaje
  tipoMsgFacial.value = 'error'
}

function actualizarFotoSesion(fotoModificada: string | null) {
  if (!auth.user) return
  auth.establecerSesionVerificada({ ...auth.user, fotoModificada })
}

async function enrolar() {
  const captura = capturaPendiente.value
  if (!captura || enrolando.value) return
  msgFacial.value = ''
  enrolando.value = true

  try {
    const fotoPersonalizada = await crearFotoPersonalizada(captura.fotoRecortada)
    await api.post('/facial/enrolar', { descriptor: captura.descriptor })
    await api.put('/auth/perfil', {
      fotoBase64: captura.fotoRecortada,
      fotoModificadaBase64: fotoPersonalizada
    })

    const { data } = await api.get('/auth/perfil')
    perfil.value = data
    actualizarFotoSesion(data.fotoModificada)

    msgFacial.value = 'Rostro enrolado correctamente.'
    tipoMsgFacial.value = 'success'
    camaraFacialActiva.value = false
    capturaPendiente.value = null
  } catch (e: any) {
    msgFacial.value = e.response?.data?.mensaje || 'No se pudo actualizar el rostro o la foto. Intenta nuevamente.'
    tipoMsgFacial.value = 'error'
  } finally {
    enrolando.value = false
  }
}
</script>

<style scoped>
.facial-enrol-foto {
  border: 3px solid #B48B21;
}
.gap-2 { gap: 8px; }
.foto-preview {
  position: relative;
  width: 120px;
  height: 160px;
  margin: 0 auto;
  overflow: hidden;
  border: 3px solid #B48B21;
  border-radius: 12px;
  background: #edf1f4;
}
.foto-preview__imagen { display: block; width: 100%; height: 100%; object-fit: cover; }
.foto-preview__sticker {
  position: absolute;
  z-index: 1;
  cursor: grab;
  touch-action: none;
  user-select: none;
  -webkit-user-drag: none;
}
.foto-preview__sticker:active { cursor: grabbing; }
.efectos-foto {
  max-width: 420px;
  margin: 0 auto;
  padding-top: 4px;
  border-top: 1px solid rgba(5, 27, 46, .12);
}
</style>
