<template>
  <div>
    <div class="d-flex align-center mb-5">
      <v-icon color="accent" class="mr-3">mdi-account-edit</v-icon>
      <div>
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Mi Perfil</h1>
        <p class="text-body-2 text-medium-emphasis">Gestiona tus datos y el acceso a tu cuenta</p>
      </div>
    </div>

    <v-tabs v-model="pestanaActiva" color="accent" class="profile-tabs mb-6"
            aria-label="Secciones de mi perfil">
      <v-tab value="informacion" prepend-icon="mdi-account-outline" :disabled="enrolando">
        Información personal
      </v-tab>
      <v-tab value="seguridad" prepend-icon="mdi-shield-lock-outline" :disabled="enrolando">
        Seguridad
      </v-tab>
    </v-tabs>

    <v-row>
      <!-- Avatar y datos rápidos -->
      <v-col v-if="pestanaActiva === 'informacion'" cols="12" md="4">
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

      <v-col cols="12" :md="pestanaActiva === 'informacion' ? 8 : 12">
        <v-card v-if="pestanaActiva === 'seguridad'" class="pa-6 mb-6 profile-security-card">
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

        <v-card v-if="pestanaActiva === 'informacion'" class="pa-6 mb-6">
          <h2 class="text-subtitle-1 font-weight-bold mb-4">Actualizar información</h2>

          <v-form @submit.prevent="guardarPerfil">
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

        <v-card v-if="pestanaActiva === 'seguridad'" class="pa-6 profile-security-card">
          <div class="d-flex align-center mb-2">
            <v-icon icon="mdi-shield-key-outline" color="accent" class="mr-3" />
            <h2 class="text-subtitle-1 font-weight-bold">Cambiar contraseña</h2>
          </div>
          <p class="text-body-2 text-medium-emphasis mb-5">Confirma tu contraseña actual antes de guardar una nueva.</p>

          <v-form @submit.prevent="cambiarPassword">
            <v-text-field v-model="password.passwordActual" label="Contraseña actual"
                          prepend-inner-icon="mdi-lock-outline" autocomplete="current-password"
                          :type="mostrarActual ? 'text' : 'password'" variant="outlined" class="mb-3">
              <template #append-inner>
                <v-btn :icon="mostrarActual ? 'mdi-eye-off-outline' : 'mdi-eye-outline'" variant="text" size="small"
                       :aria-label="mostrarActual ? 'Ocultar contraseña actual' : 'Mostrar contraseña actual'"
                       @click="mostrarActual = !mostrarActual" />
              </template>
            </v-text-field>
            <v-text-field v-model="password.nuevoPassword" label="Nueva contraseña"
                          prepend-inner-icon="mdi-lock-plus-outline" autocomplete="new-password"
                          :type="mostrarNueva ? 'text' : 'password'" variant="outlined"
                          maxlength="35" counter="35" class="mb-3">
              <template #append-inner>
                <v-btn :icon="mostrarNueva ? 'mdi-eye-off-outline' : 'mdi-eye-outline'" variant="text" size="small"
                       :aria-label="mostrarNueva ? 'Ocultar nueva contraseña' : 'Mostrar nueva contraseña'"
                       @click="mostrarNueva = !mostrarNueva" />
              </template>
            </v-text-field>

            <div class="password-strength mb-4" aria-live="polite">
              <div class="d-flex justify-space-between text-body-2 mb-2">
                <span>Seguridad estimada</span>
                <strong :style="{ color: nivelPassword.color }">{{ nivelPassword.etiqueta }}</strong>
              </div>
              <v-progress-linear :model-value="nivelPassword.valor" :color="nivelPassword.color"
                                 bg-color="#E7EBEF" rounded height="8" />
              <p class="text-caption text-medium-emphasis mt-2">Estimación basada en la longitud y los tipos de caracteres.</p>
            </div>

            <div class="password-rules mb-5" role="list" aria-label="Requisitos de la nueva contraseña">
              <div v-for="regla in reglasPassword" :key="regla.etiqueta" role="listitem"
                   class="password-rule" :class="{ 'password-rule--met': regla.cumple }">
                <v-icon :icon="regla.cumple ? 'mdi-check-circle' : 'mdi-close-circle-outline'" size="18" />
                <span>{{ regla.etiqueta }}</span>
              </div>
            </div>

            <v-text-field v-model="password.confirmacion" label="Confirmar nueva contraseña"
                          prepend-inner-icon="mdi-lock-check-outline" autocomplete="new-password"
                          :type="mostrarConfirmacion ? 'text' : 'password'" variant="outlined"
                          maxlength="35" class="mb-1">
              <template #append-inner>
                <v-btn :icon="mostrarConfirmacion ? 'mdi-eye-off-outline' : 'mdi-eye-outline'" variant="text" size="small"
                       :aria-label="mostrarConfirmacion ? 'Ocultar confirmación' : 'Mostrar confirmación'"
                       @click="mostrarConfirmacion = !mostrarConfirmacion" />
              </template>
            </v-text-field>
            <p v-if="password.confirmacion" class="password-match mb-4"
               :class="{ 'password-match--met': coincidenPassword }">
              <v-icon :icon="coincidenPassword ? 'mdi-check-circle' : 'mdi-close-circle-outline'" size="18" />
              {{ coincidenPassword ? 'Las contraseñas coinciden' : 'Las contraseñas no coinciden' }}
            </p>

            <v-alert v-if="msgPasswordExito" type="success" variant="tonal" density="compact" class="mb-4">
              {{ msgPasswordExito }}
            </v-alert>
            <v-alert v-if="msgPasswordError" type="error" variant="tonal" density="compact" class="mb-4">
              {{ msgPasswordError }}
            </v-alert>
            <v-btn type="submit" color="accent" prepend-icon="mdi-lock-reset" block
                   :loading="cambiandoPassword" :disabled="!passwordLista || cambiandoPassword">
              Cambiar contraseña
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
const pestanaActiva = ref<'informacion' | 'seguridad'>('informacion')
const perfil   = ref<any>(null)
const fotoPerfilUrl = computed(() => fotoUrl(perfil.value?.fotoModificada))
const guardando = ref(false)
const msgExito = ref(''); const msgError = ref('')
const form = reactive({
  telefono: '', metodoNotificacion: 'email'
})
const password = reactive({ passwordActual: '', nuevoPassword: '', confirmacion: '' })
const cambiandoPassword = ref(false)
const msgPasswordExito = ref('')
const msgPasswordError = ref('')
const mostrarActual = ref(false)
const mostrarNueva = ref(false)
const mostrarConfirmacion = ref(false)
const reglasPassword = computed(() => [
  { etiqueta: 'Entre 8 y 35 caracteres', cumple: password.nuevoPassword.length >= 8 && password.nuevoPassword.length <= 35 },
  { etiqueta: 'Una letra mayúscula', cumple: /[A-Z]/.test(password.nuevoPassword) },
  { etiqueta: 'Una letra minúscula', cumple: /[a-z]/.test(password.nuevoPassword) },
  { etiqueta: 'Un número', cumple: /\d/.test(password.nuevoPassword) },
  { etiqueta: 'Un símbolo', cumple: /[^A-Za-z0-9]/.test(password.nuevoPassword) }
])
const coincidenPassword = computed(() => password.confirmacion.length > 0 && password.nuevoPassword === password.confirmacion)
const passwordLista = computed(() => password.passwordActual.length > 0 && reglasPassword.value.every(regla => regla.cumple) && coincidenPassword.value)
const nivelPassword = computed(() => {
  if (!password.nuevoPassword) return { etiqueta: 'Sin evaluar', valor: 0, color: '#94A3B8' }
  const cumplidas = reglasPassword.value.filter(regla => regla.cumple).length
  if (cumplidas < 3) return { etiqueta: 'Débil', valor: 25, color: '#B42318' }
  if (cumplidas < 5) return { etiqueta: 'En progreso', valor: 50, color: '#C47F00' }
  if (password.nuevoPassword.length < 12) return { etiqueta: 'Adecuada', valor: 75, color: '#2E7D32' }
  return { etiqueta: 'Fuerte', valor: 100, color: '#15803D' }
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

watch(pestanaActiva, nueva => {
  if (nueva !== 'seguridad' && camaraFacialActiva.value) cancelarEnrolamiento()
})

onMounted(async () => {
  calentar()
  try {
    const { data } = await api.get('/auth/perfil')
    perfil.value = data
    form.telefono = data.telefono
    form.metodoNotificacion = data.metodoNotificacion
  } catch { /* ignore */ }
})

async function guardarPerfil() {
  msgExito.value = ''; msgError.value = ''
  guardando.value = true
  try {
    const payload = {
      telefono: form.telefono,
      metodoNotificacion: form.metodoNotificacion
    }
    await api.put('/auth/perfil', payload)
    msgExito.value = 'Perfil actualizado correctamente.'
  } catch (e: any) {
    msgError.value = e.response?.data?.mensaje || 'Error al guardar.'
  } finally { guardando.value = false }
}

async function cambiarPassword() {
  msgPasswordExito.value = ''
  msgPasswordError.value = ''
  if (!passwordLista.value) {
    msgPasswordError.value = 'Completa los tres campos y cumple todos los requisitos de la nueva contraseña.'
    return
  }

  cambiandoPassword.value = true
  try {
    await api.put('/auth/perfil', {
      passwordActual: password.passwordActual,
      nuevoPassword: password.nuevoPassword,
      confirmarNuevoPassword: password.confirmacion
    })
    msgPasswordExito.value = 'Tu contraseña se cambió correctamente.'
    password.passwordActual = ''
    password.nuevoPassword = ''
    password.confirmacion = ''
    mostrarActual.value = false
    mostrarNueva.value = false
    mostrarConfirmacion.value = false
  } catch (e: any) {
    msgPasswordError.value = e.response?.data?.mensaje || 'No se pudo cambiar la contraseña. Intenta nuevamente.'
  } finally {
    cambiandoPassword.value = false
  }
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
.profile-tabs { border-bottom: 1px solid #D8E1EA; }
.profile-tabs :deep(.v-tab) { text-transform: none; font-weight: 700; letter-spacing: 0; }
.profile-security-card { max-width: 960px; margin-left: auto; margin-right: auto; }
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
.password-strength, .password-rules { max-width: 520px; }
.password-rules { display: grid; grid-template-columns: repeat(auto-fit, minmax(205px, 1fr)); gap: 8px 16px; }
.password-rule, .password-match { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #B42318; }
.password-rule--met, .password-match--met { color: #15803D; }
</style>
