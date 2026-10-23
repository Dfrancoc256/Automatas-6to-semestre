<template>
  <div :class="['registro-content', {
    'registro-content--captura': paso === 2,
    'registro-content--edicion': paso === 1 && formularioEnEdicion,
  }]">
    <div v-if="mostrarOpcionesRegistro" class="registro-choice text-center">
      <img class="auth-logo login-logo mx-auto mb-4" src="/images/logo-umg-oficial.png"
           alt="Universidad Mariano Gálvez de Guatemala" />
      <h1 class="text-h5 font-weight-bold mb-2">Crear cuenta</h1>
      <p class="text-body-2 text-medium-emphasis mb-7">Elige cómo deseas registrarte.</p>

      <v-btn variant="outlined" color="primary" class="registro-choice__action google-access-button mb-3"
             @click="registrarConGoogle">
        <span class="google-mark" aria-hidden="true">G</span>
        <span>Continuar con Google</span>
      </v-btn>
      <p class="registro-choice__hint">Si tu cuenta es nueva, verificaremos tu identidad y después solicitaremos únicamente los datos necesarios para tu credencial.</p>
      <button type="button" class="registro-choice__login" @click="emit('cambiarModo', 'login')">¿Ya tienes una cuenta? Ingresa con Google</button>
      <div class="registro-choice__divider"><span>o</span></div>
      <v-btn color="accent" class="registro-choice__action umg-gold-button" @click="crearConCorreo">
        Crear cuenta con correo
      </v-btn>
      <p class="text-body-2 text-medium-emphasis mt-6 mb-0">
        ¿Ya tienes cuenta?
        <button type="button" class="auth-inline-link" @click="emit('cambiarModo', 'login')">Inicia sesión</button>
      </p>
    </div>

    <template v-else>
      <div class="text-center mb-6">
        <img class="auth-logo login-logo mx-auto mb-3" src="/images/logo-umg-oficial.png"
             alt="Universidad Mariano Gálvez de Guatemala" />
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Crear cuenta</h1>
        <p class="text-body-2 text-medium-emphasis mt-1">{{ esRegistroGoogle ? 'Completa los datos para generar tu credencial' : 'Completa el formulario para registrarte' }}</p>
        <v-btn v-if="!esRegistroGoogle" variant="text" size="small" color="primary" class="mt-1" @click="mostrarOpcionesRegistro = true">
          Elegir otro método
        </v-btn>
      </div>

      <v-stepper v-model="paso" alt-labels flat non-linear>
        <v-stepper-header>
          <v-stepper-item title="Datos" :value="1" :complete="paso > 1" :editable="paso === 2" />
          <v-divider />
          <v-stepper-item title="Foto" :value="2" :complete="paso > 2" :editable="paso === 3" />
          <v-divider />
          <v-stepper-item title="Listo" :value="3" />
        </v-stepper-header>

        <v-stepper-window>
          <!-- Paso 1: datos básicos -->
          <v-stepper-window-item :value="1">
            <v-form ref="formPaso1" @focusin="activarEdicion" @submit.prevent="siguientePaso">
              <div v-if="esRegistroGoogle" class="google-identity mb-5">
                <span class="google-identity__icon"><span class="google-mark" aria-hidden="true">G</span></span>
                <div>
                  <strong>Cuenta de Google verificada</strong>
                  <span>{{ form.nombre }} · {{ form.correo }}</span>
                </div>
                <v-icon color="success" size="20">mdi-check-decagram</v-icon>
              </div>
              <p v-if="esRegistroGoogle && datosGoogleFaltantes.length" class="google-identity__notice mb-4">
                Google no proporcionó {{ datosGoogleFaltantes.join(', ') }}. Completa únicamente ese dato para continuar.
              </p>

              <v-row v-if="!esRegistroGoogle" dense>
                <v-col cols="12" sm="6">
                  <v-text-field v-model.trim="form.nombre" label="Nombres"
                                prepend-inner-icon="mdi-account"
                                autocomplete="given-name"
                                :readonly="esRegistroGoogle"
                                :rules="[validarNombre]" class="mb-1" />
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field v-model.trim="form.apellido" label="Apellidos"
                                prepend-inner-icon="mdi-account-outline"
                                autocomplete="family-name"
                                :rules="[validarApellido]" class="mb-1" />
                </v-col>
              </v-row>

              <v-text-field v-if="!esRegistroGoogle" v-model="form.correo" label="Correo electrónico"
                            prepend-inner-icon="mdi-email" type="email"
                            :rules="[v => !!v || 'Requerido', v => /.+@.+\..+/.test(v) || 'Correo inválido']"
                            class="mb-3" />
              <v-text-field v-if="esRegistroGoogle && !form.apellido" v-model.trim="form.apellido" label="Apellidos"
                            prepend-inner-icon="mdi-account-outline" autocomplete="family-name"
                            :rules="[validarApellido]" class="mb-3" />
              <v-text-field v-if="!esRegistroGoogle || !telefonoGoogleValido" v-model="form.telefono" label="Teléfono"
                            prepend-inner-icon="mdi-phone"
                            :rules="[v => !!v || 'Requerido']" class="mb-3" />
              <v-text-field v-if="!esRegistroGoogle || !fechaGoogleValida" v-model="form.fechaNacimiento" label="Fecha de nacimiento"
                            prepend-inner-icon="mdi-calendar" type="date"
                            :rules="[v => !!v || 'Requerido']" class="mb-3" />
              <v-select v-if="!esRegistroGoogle || !telefonoGoogleValido" v-model="form.metodoNotificacion"
                        label="Método de notificación"
                        prepend-inner-icon="mdi-bell"
                        :items="['email','whatsapp','ambos']"
                        class="mb-4" />

              <v-alert v-if="error" type="error" variant="tonal" density="compact" class="mb-4">
                {{ error }}
              </v-alert>

              <v-btn color="accent" class="umg-gold-button" block size="large" type="submit">
                Siguiente <v-icon end>mdi-arrow-right</v-icon>
              </v-btn>
            </v-form>
          </v-stepper-window-item>

          <!-- Paso 2: foto -->
          <v-stepper-window-item :value="2">
            <div class="text-center mb-4">
              <p class="text-body-2 mb-4">Toma una foto: la credencial usará un recorte centrado en tu rostro</p>

              <!-- Cámara con marco guía y animación de escaneo -->
              <FaceScanner
                v-if="!fotoCapturada"
                ref="escaner"
                :auto="false"
                con-foto
                class="mb-4"
                @error="error = $event"
              />

              <input
                ref="selectorFoto"
                class="selector-foto-oculto"
                type="file"
                accept="image/jpeg,image/png,image/webp"
                capture="user"
                @change="seleccionarFoto"
              />

              <!-- Foto capturada -->
              <div v-if="fotoCapturada" class="mb-4">
                <div ref="vistaPreviaFoto" class="foto-preview">
                  <img :src="fotoParaCredencial || fotoCapturada" :style="{ filter: filtroActual.css }"
                       class="foto-preview__imagen" alt="Vista previa de la credencial" />
                  <img v-if="stickerActual !== 'ninguno'" :src="imagenesStickers[stickerActual]"
                       class="foto-preview__sticker"
                       :style="{ left: `${posicionSticker.x}px`, top: `${posicionSticker.y}px`, width: `${tamanoSticker}px` }"
                       alt="Sticker decorativo" draggable="false" @pointerdown.prevent="iniciarArrastreSticker" />
                </div>
                <p v-if="procesandoRostro" class="text-caption mt-1">Analizando rostro...</p>
                <p v-else-if="rostroDetectado" class="text-caption mt-1 text-success">✓ Foto capturada · rostro detectado y recortado</p>
                <p v-else class="text-caption mt-1 text-warning">
                  ✓ Foto capturada · no se detectó un rostro claro (podrás enrolarlo después desde tu perfil)
                </p>
              </div>

              <div v-if="fotoCapturada" class="mb-4">
                <p class="text-caption font-weight-bold mb-2">Personaliza tu foto para la credencial</p>
                <div class="d-flex flex-wrap justify-center gap-2">
                  <v-btn v-for="filtro in filtros" :key="filtro.id" size="small"
                         :color="filtroActual.id === filtro.id ? 'accent' : 'primary'"
                         :variant="filtroActual.id === filtro.id ? 'flat' : 'outlined'"
                         :prepend-icon="filtro.icono"
                         @click="filtroActual = filtro">
                    {{ filtro.nombre }}
                  </v-btn>
                </div>
              </div>

              <div v-if="fotoCapturada" class="efectos-foto mb-4">
                <p class="text-caption font-weight-bold mb-2">Agrega iconos y adornos</p>
                <PhotoStickerSelector v-model="stickerActual" />
              </div>
              <p v-if="fotoCapturada && stickerActual !== 'ninguno'" class="text-caption text-medium-emphasis mb-4">
                Arrastra el icono sobre la foto para colocarlo donde prefieras.
              </p>

              <div class="d-flex flex-wrap gap-2 justify-center mb-4">
                <v-btn v-if="!fotoCapturada" color="accent" class="umg-gold-button"
                       prepend-icon="mdi-camera-iris" :loading="procesandoRostro" @click="capturarFoto">
                  Capturar foto
                </v-btn>
                <v-btn v-if="!fotoCapturada" color="primary" variant="outlined"
                       prepend-icon="mdi-image-plus" @click="selectorFoto?.click()">
                  Tomar o subir foto
                </v-btn>
              </div>
            </div>

            <v-alert v-if="error" type="error" variant="tonal" class="registro-error mb-3" density="compact" closable @click:close="error = ''">
              <span class="registro-error__message">{{ error }}</span>
            </v-alert>
            <template v-if="fotoCapturada">
              <p class="text-body-2 text-medium-emphasis text-center mb-2">Confirma la verificación de seguridad antes de crear la cuenta.</p>
              <RecaptchaV2
                v-if="requiereRecaptcha"
                ref="recaptcha"
                v-model:token="recaptchaToken"
                :site-key="recaptchaSiteKey"
                class="mb-3"
              />
              <v-alert v-else-if="!bypassDesarrollo" type="warning" variant="tonal" density="compact" class="mb-3">
                Falta configurar la clave pública de reCAPTCHA.
              </v-alert>
            </template>
            <v-btn v-if="error && fotoCapturada" class="registro-retry mb-4" size="small" variant="text" prepend-icon="mdi-refresh" @click="registrar">
              Reintentar registro
            </v-btn>

            <div class="registro-actions">
              <v-btn color="secondary" variant="outlined" prepend-icon="mdi-refresh"
                     :disabled="cargando" @click="retomar">
                Retomar
              </v-btn>
              <v-btn color="accent" class="umg-gold-button" :loading="cargando"
                     :disabled="!puedeRegistrar" @click="registrar">
                Registrarme
              </v-btn>
            </div>
          </v-stepper-window-item>

          <!-- Paso 3: éxito -->
          <v-stepper-window-item :value="3">
            <div class="text-center py-6">
              <v-icon size="80" color="success" class="mb-4">mdi-check-circle</v-icon>
              <h2 class="text-h5 font-weight-bold mb-2">¡Registro exitoso!</h2>
              <p class="text-body-2 text-medium-emphasis mb-6">
                {{ esRegistroGoogle
                  ? 'Tu cuenta ha sido creada. A partir de ahora podrás ingresar con la cuenta de Google seleccionada.'
                  : 'Tu cuenta ha sido creada. Revisa tu correo para conocer la contraseña temporal. En tu próximo inicio de sesión deberás cambiarla por una contraseña personal.' }}
              </p>
              <v-btn color="accent" class="umg-gold-button" prepend-icon="mdi-login"
                     @click="navigateTo('/dashboard')">
                Ingresar
              </v-btn>
              <v-btn class="mt-3" variant="outlined" color="secondary"
                     prepend-icon="mdi-file-pdf-box" :loading="descargando"
                     @click="descargarCredencial">
                Descargar credencial PDF
              </v-btn>
            </div>
          </v-stepper-window-item>
        </v-stepper-window>
      </v-stepper>

      <div class="text-center mt-4">
        <span class="text-body-2 text-medium-emphasis">¿Ya tienes cuenta? </span>
        <button type="button" class="auth-inline-link" @click="emit('cambiarModo', 'login')">Inicia sesión</button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
const emit = defineEmits<{ cambiarModo: [modo: 'login' | 'registro'] }>()

const auth    = useAuthStore()
const { api } = useApi()
const config = useRuntimeConfig()
const route = useRoute()
const { calentar, detectarRostro } = useFacialRecognition()

const paso        = ref(1)
const cargando    = ref(false)
const error       = ref('')
const descargando = ref(false)
const formPaso1   = ref<any>(null)
const formularioEnEdicion = ref(false)
const googleRegistroToken = computed(() => typeof route.query.googleRegistro === 'string' ? route.query.googleRegistro : '')
const esRegistroGoogle = computed(() => googleRegistroToken.value.length > 0)
const mostrarOpcionesRegistro = ref(!googleRegistroToken.value)

const form = reactive({
  nombre: '', apellido: '', correo: '', telefono: '', fechaNacimiento: '',
  metodoNotificacion: 'email'
})
const telefonoGoogleValido = computed(() => /^\+?[0-9\s-]{8,20}$/.test(form.telefono.trim()))
const fechaGoogleValida = computed(() => /^\d{4}-\d{2}-\d{2}$/.test(form.fechaNacimiento))
const datosGoogleFaltantes = computed(() => [
  !form.apellido ? 'apellidos' : '',
  !telefonoGoogleValido.value ? 'teléfono' : '',
  !fechaGoogleValida.value ? 'fecha de nacimiento' : ''
].filter(Boolean))

const patronNombre = /^[\p{L}][\p{L}\s'-]{1,79}$/u
const validarNombre = (valor: string) =>
  patronNombre.test(valor.trim()) || 'Ingresa nombres válidos (solo letras, espacios, apóstrofes o guiones).'
const validarApellido = (valor: string) =>
  patronNombre.test(valor.trim()) || 'Ingresa apellidos válidos (solo letras, espacios, apóstrofes o guiones).'

function activarEdicion() {
  formularioEnEdicion.value = true
}

onMounted(async () => {
  if (!googleRegistroToken.value) return
  mostrarOpcionesRegistro.value = false
  try {
    const { data } = await api.post('/auth/google-registro', { token: googleRegistroToken.value })
    form.correo = data.correo
    form.nombre = data.nombre
    form.apellido = data.apellido || ''
    form.telefono = data.telefono || ''
    form.fechaNacimiento = data.fechaNacimiento || ''
    form.metodoNotificacion = telefonoGoogleValido.value ? 'ambos' : 'email'
  } catch (e: any) {
    error.value = e.response?.data?.mensaje || 'No se pudo validar la cuenta de Google.'
  }
})

function crearConCorreo() {
  mostrarOpcionesRegistro.value = false
}

function registrarConGoogle() {
  window.location.assign(`${config.public.apiBase}/auth/google/registrar`)
}
const recaptchaToken = ref('')
const recaptcha = ref<{ reiniciar: () => void } | null>(null)
const recaptchaSiteKey = computed(() => config.public.recaptchaSiteKey.trim())
const requiereRecaptcha = computed(() => recaptchaSiteKey.value.length > 0)
const bypassDesarrollo = computed(() => import.meta.dev && !requiereRecaptcha.value)
const puedeEnviar = computed(() => bypassDesarrollo.value || recaptchaToken.value.length > 0)
const puedeRegistrar = computed(() => !!fotoCapturada.value && puedeEnviar.value)

// La cámara se administra en FaceScanner.
const escaner       = ref<{ capturar: () => Promise<{ descriptor: number[] | null, foto: string | null, fotoRecortada: string | null } | null> } | null>(null)
const selectorFoto  = ref<HTMLInputElement | null>(null)
const fotoCapturada = ref<string | null>(null)
const fotoParaCredencial = ref<string | null>(null)
const descriptorFacial  = ref<number[] | null>(null)
const procesandoRostro  = ref(false)
const rostroDetectado = ref(false)
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
let arrastrandoSticker = false
let desplazamientoSticker = { x: 0, y: 0 }

const filtros = [
  { id: 'normal', nombre: 'Natural', icono: 'mdi-image-filter-none', css: 'none', canvas: 'none' },
  { id: 'calido', nombre: 'Cálido', icono: 'mdi-white-balance-sunny', css: 'sepia(.28) saturate(1.15)', canvas: 'sepia(28%) saturate(115%)' },
  { id: 'azul', nombre: 'Azul UMG', icono: 'mdi-palette-outline', css: 'hue-rotate(175deg) saturate(1.1)', canvas: 'hue-rotate(175deg) saturate(110%)' },
  { id: 'gris', nombre: 'Clásico', icono: 'mdi-image-filter-black-white', css: 'grayscale(1) contrast(1.08)', canvas: 'grayscale(100%) contrast(108%)' }
]
const filtroActual = ref(filtros[0]!)

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

async function siguientePaso() {
  error.value = ''
  const { valid } = await formPaso1.value?.validate()
  if (!valid) return
  paso.value = 2
  calentar()
}

watch(paso, (pasoActual) => {
  // Al volver a Datos se descarta un token viejo; se solicitará uno nuevo al final.
  if (pasoActual !== 2) {
    recaptchaToken.value = ''
    recaptcha.value?.reiniciar()
  }
})

async function capturarFoto() {
  error.value = ''
  procesandoRostro.value = true
  try {
    const captura = await escaner.value?.capturar()
    if (!captura?.foto) {
      error.value = 'No se pudo tomar la foto. Verifica que la cámara esté activa.'
      return
    }
    if (!captura.fotoRecortada) {
      error.value = 'No se detectó un rostro. Centra tu rostro, mejora la iluminación y vuelve a tomar la foto.'
      return
    }
    descriptorFacial.value = captura.descriptor
    fotoCapturada.value = captura.foto
    // La credencial usa el retrato recortado.
    fotoParaCredencial.value = captura.fotoRecortada
    rostroDetectado.value = true
  } catch {
    error.value = 'No se pudo tomar la foto.'
  } finally {
    procesandoRostro.value = false
  }
}

/** Abre la cámara o galería nativa en móvil. */
async function seleccionarFoto(evento: Event) {
  error.value = ''
  const input = evento.target as HTMLInputElement
  const archivo = input.files?.[0]
  input.value = ''

  if (!archivo) return
  if (!['image/jpeg', 'image/png', 'image/webp'].includes(archivo.type)) {
    error.value = 'Selecciona una fotografía JPG, PNG o WEBP.'
    return
  }
  if (archivo.size > 5 * 1024 * 1024) {
    error.value = 'La fotografía no puede superar 5 MB.'
    return
  }

  try {
    const fotoLeida = await new Promise<string>((resolver, rechazar) => {
      const lector = new FileReader()
      lector.onload = () => typeof lector.result === 'string' ? resolver(lector.result) : rechazar()
      lector.onerror = () => rechazar(lector.error)
      lector.readAsDataURL(archivo)
    })

    const recorte = await crearRecorteDesdeImagen(fotoLeida)
    if (!recorte) {
      error.value = 'No se detectó un rostro en la fotografía. Toma otra foto de frente, con buena iluminación.'
      return
    }
    fotoCapturada.value = fotoLeida
    fotoParaCredencial.value = recorte
    // El retrato se recorta antes de enrolar el rostro.
    descriptorFacial.value = null
    rostroDetectado.value = true
  } catch {
    error.value = 'No se pudo leer la fotografía seleccionada.'
  }
}

function cargarImagen(origen: string): Promise<HTMLImageElement> {
  return new Promise((resolver, rechazar) => {
    const imagen = new Image()
    imagen.onload = () => resolver(imagen)
    imagen.onerror = () => rechazar(new Error('No se pudo cargar la imagen.'))
    imagen.src = origen
  })
}

async function crearRecorteDesdeImagen(foto: string): Promise<string | null> {
  const imagen = await cargarImagen(foto)
  let caja: Awaited<ReturnType<typeof detectarRostro>> = null
  try {
    caja = await detectarRostro(imagen)
  } catch {
    return null
  }
  if (!caja) return null

  const relacion = 3 / 4
  const altoRecorte = Math.min(imagen.naturalHeight, Math.max(caja.alto * 1.22, caja.ancho / relacion * 1.18))
  const anchoRecorte = Math.min(imagen.naturalWidth, altoRecorte * relacion)
  const centroX = caja.x + caja.ancho / 2
  const centroY = caja.y + caja.alto * 0.48
  const origenX = Math.max(0, Math.min(imagen.naturalWidth - anchoRecorte, centroX - anchoRecorte / 2))
  const origenY = Math.max(0, Math.min(imagen.naturalHeight - altoRecorte, centroY - altoRecorte / 2))
  const lienzo = document.createElement('canvas')
  lienzo.width = 360
  lienzo.height = 480
  lienzo.getContext('2d')!.drawImage(imagen, origenX, origenY, anchoRecorte, altoRecorte, 0, 0, lienzo.width, lienzo.height)
  return lienzo.toDataURL('image/jpeg', 0.88)
}

async function crearFotoPersonalizada() {
  const fuente = fotoParaCredencial.value || fotoCapturada.value
  if (!fuente) return null
  try {
    const imagen = await cargarImagen(fuente)
    const lienzo = document.createElement('canvas')
    // La credencial usa formato vertical 3:4.
    lienzo.width = 360
    lienzo.height = 480
    const contexto = lienzo.getContext('2d')!
    contexto.filter = filtroActual.value.canvas
    contexto.drawImage(imagen, 0, 0, lienzo.width, lienzo.height)
    contexto.filter = 'none'

    const rutaSticker = imagenesStickers[stickerActual.value]
    if (rutaSticker) {
      try {
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
      } catch {
        // El registro continúa aunque falle un recurso visual.
      }
    }
    return lienzo.toDataURL('image/jpeg', 0.85)
  } catch {
    // No bloquea el registro si Canvas no está disponible.
    return fuente
  }
}

function retomar() {
  fotoCapturada.value = null
  fotoParaCredencial.value = null
  descriptorFacial.value = null
  rostroDetectado.value = false
  stickerActual.value = 'ninguno'
  posicionSticker.x = 72
  posicionSticker.y = 12
}

onBeforeUnmount(terminarArrastreSticker)

async function registrar() {
  error.value = ''
  cargando.value = true
  try {
    const payload = {
      correo:             form.correo,
      nombre:             form.nombre,
      apellido:           form.apellido,
      telefono:           form.telefono,
      fechaNacimiento:    form.fechaNacimiento,
      metodoNotificacion: form.metodoNotificacion,
      // Guarda solo el recorte facial.
      fotoBase64:         fotoParaCredencial.value || fotoCapturada.value,
      fotoModificadaBase64: await crearFotoPersonalizada(),
      descriptor:         descriptorFacial.value,
      googleRegistroToken: googleRegistroToken.value || undefined,
      recaptchaToken:     bypassDesarrollo.value ? 'dev-bypass' : recaptchaToken.value
    }
    const { data } = await api.post('/auth/registro', payload)
    auth.login(data)
    paso.value = 3
  } catch (e: any) {
    error.value = obtenerMensajeErrorRegistro(e)
    // El token debe renovarse después de cada envío.
    if (requiereRecaptcha.value && !bypassDesarrollo.value) {
      recaptchaToken.value = ''
      recaptcha.value?.reiniciar()
      paso.value = 1
      error.value += ' Completa nuevamente reCAPTCHA antes de reintentar.'
    }
  } finally {
    cargando.value = false
  }
}

/** Convierte errores HTTP y de red en mensajes legibles. */
function obtenerMensajeErrorRegistro(errorRespuesta: any) {
  const estado = errorRespuesta?.response?.status
  const respuesta = errorRespuesta?.response?.data

  if (Array.isArray(respuesta?.errores)) return respuesta.errores.join(' ')
  if (typeof respuesta?.mensaje === 'string' && respuesta.mensaje.trim()) return respuesta.mensaje
  if (typeof respuesta?.detail === 'string' && respuesta.detail.trim()) return respuesta.detail
  if (typeof respuesta === 'string' && respuesta.trim()) return respuesta

  if (!estado) return 'No se pudo conectar con el servidor. Verifica tu conexión e inténtalo nuevamente.'
  if (estado === 413) return 'La fotografía es demasiado grande. Selecciona una imagen menor de 5 MB.'
  if (estado === 429) return 'Se alcanzó el límite de intentos. Espera un minuto antes de volver a intentarlo.'
  if (estado >= 500) return 'El servidor no pudo completar el registro. Intenta nuevamente en unos minutos.'
  return 'No fue posible completar el registro. Revisa los datos e inténtalo nuevamente.'
}

async function descargarCredencial() {
  descargando.value = true
  try {
    const { data } = await api.get('/auth/credencial', { responseType: 'blob' })
    const url = URL.createObjectURL(new Blob([data], { type: 'application/pdf' }))
    const enlace = document.createElement('a')
    enlace.href = url
    enlace.download = `credencial-${auth.user?.nickname || 'usuario'}.pdf`
    enlace.click()
    URL.revokeObjectURL(url)
  } catch {
    error.value = 'No se pudo generar la credencial.'
  } finally {
    descargando.value = false
  }
}

</script>

<style scoped>
.auth-logo {
  width: 140px; height: 140px;
  object-fit: contain;
  background: transparent;
  border: 0;
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  outline: none;
  box-shadow:
    0 0 18px 8px rgba(180,139,33,.38),
    0 14px 28px rgba(5,27,46,.22);
}
.gap-2 { gap: 8px; }
.registro-content > .text-center.mb-6 { margin-bottom: 12px !important; }.registro-content .text-h5 { font-size: 23px !important; line-height: 1.15; }.registro-content .text-body-2 { font-size: 13px !important; }
.registro-content > .text-center.mb-6 {
  max-height: 240px;
  overflow: hidden;
  opacity: 1;
  transform: translateY(0);
  transition: max-height .24s ease, opacity .16s ease, margin .24s ease, transform .24s ease;
}
/* Reduce el encabezado durante el registro. */
.registro-content--edicion > .text-center.mb-6,
.registro-content--captura > .text-center.mb-6 {
  max-height: 0;
  margin: 0 !important;
  opacity: 0;
  transform: translateY(-12px);
  pointer-events: none;
}
.registro-content :deep(.v-stepper) { display: flex; flex: 1; min-height: 0; flex-direction: column; }
.registro-content :deep(.v-stepper-header) { min-height: 74px; }.registro-content :deep(.v-stepper-item) { padding: 8px 4px; }.registro-content :deep(.v-stepper-item__title) { font-size: 13px; }.registro-content :deep(.v-stepper-item__avatar) { width: 26px; height: 26px; font-size: 12px; }
.registro-content :deep(.v-stepper-window) { flex: 1; min-height: 0; overflow-y: auto; overscroll-behavior: contain; padding-right: 6px; }
.registro-content :deep(.v-stepper-window::-webkit-scrollbar) { width: 5px; }
.registro-content :deep(.v-stepper-window::-webkit-scrollbar-thumb) { background: #B48B21; border-radius: 99px; }
.registro-content--captura :deep(.v-stepper-header) { min-height: 60px; }
.registro-content--captura :deep(.face-scanner) { max-width: 420px; }
.selector-foto-oculto { display: none; }
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
  margin-right: auto;
  margin-left: auto;
  padding-top: 4px;
  border-top: 1px solid rgba(5, 27, 46, .12);
}
.registro-actions {
  display: flex;
  align-items: stretch;
  justify-content: center;
  gap: 12px;
  width: 100%;
}
.registro-actions .v-btn { flex: 1 1 0; max-width: 300px; }
.auth-inline-link { border: 0; background: transparent; color: #7A5E12; font: inherit; font-weight: 700; text-decoration: underline; cursor: pointer; }
.registro-error { min-width: 0; }
.registro-error__message { display: block; min-width: 0; overflow-wrap: anywhere; white-space: normal; }
.registro-retry { color: #7A5E12 !important; font-weight: 700 !important; }
.registro-choice {
  display: flex;
  flex: 1;
  min-height: 0;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: min(100%, 430px);
  margin: 0 auto;
  padding: 16px 4px;
}
.registro-choice h1 { color: #051B2E; }
.registro-choice__hint { margin: 0 0 14px; color: #667085; font-size: 12px; line-height: 1.45; }
.registro-choice__login { margin: -5px 0 14px; padding: 0; border: 0; color: #7A5E12; background: transparent; font: inherit; font-size: 12px; font-weight: 700; text-decoration: underline; cursor: pointer; }
.registro-choice__action {
  width: min(100%, 340px) !important;
  min-height: 46px !important;
  height: 46px !important;
  padding-inline: 18px !important;
  font-size: 14px !important;
}
.registro-choice__divider {
  display: flex;
  align-items: center;
  width: 100%;
  gap: 12px;
  margin: 2px 0 14px;
  color: #6B7280;
  font-size: 12px;
}
.registro-choice__divider::before,
.registro-choice__divider::after { content: ''; flex: 1; height: 1px; background: #E2E4E8; }
.google-access-button :deep(.v-btn__content) { gap: 10px; justify-content: center; }
.google-identity { display: flex; align-items: center; gap: 10px; min-width: 0; padding: 11px 12px; border: 1px solid #D7E5EF; border-radius: 12px; background: #F5F9FC; }
.google-identity__icon { display: grid; width: 30px; height: 30px; flex: 0 0 30px; place-items: center; border-radius: 8px; background: #fff; border: 1px solid #E5ECF1; }
.google-identity > div { display: grid; min-width: 0; flex: 1; }
.google-identity strong { color: #15324A; font-size: 12px; }
.google-identity span:not(.google-mark) { overflow: hidden; margin-top: 2px; color: #65798B; font-size: 11px; text-overflow: ellipsis; white-space: nowrap; }
.google-identity__notice { color: #65798B; font-size: 12px; line-height: 1.45; }
.google-mark {
  display: inline-grid;
  width: 20px;
  height: 20px;
  place-items: center;
  background: conic-gradient(from -42deg, #4285F4 0 25%, #34A853 25% 50%, #FBBC05 50% 75%, #EA4335 75% 100%);
  color: transparent;
  -webkit-background-clip: text;
  background-clip: text;
  font-family: Arial, sans-serif;
  font-size: 20px;
  font-weight: 700;
  line-height: 1;
}
@media (max-width: 700px) {
  .registro-content { min-width: 0; max-width: 100%; overflow-x: hidden; }
  .registro-content :deep(.v-stepper-header) { overflow: hidden; }
  .registro-content :deep(.v-stepper-item) { min-width: 0; padding-inline: 2px; }
  .registro-content :deep(.v-stepper-window) { padding-right: 2px; }
  .registro-content :deep(.v-stepper-window-item),
  .registro-content :deep(.v-row) { min-width: 0; max-width: 100%; }
  .registro-content :deep(.v-stepper-window-item) { padding-inline: 0; }
  .registro-content :deep(.v-row) { margin-inline: 0; }
  .registro-content :deep(.v-col) { min-width: 0; padding-inline: 4px; }
  .registro-content :deep(.v-col-sm-6) { flex: 0 0 100% !important; max-width: 100% !important; }
  .registro-content :deep(.v-input),
  .registro-content :deep(.v-field) { max-width: 100%; }
  .registro-content :deep(.v-stepper-header) { min-height: 64px; }
  .registro-content :deep(.v-stepper-item__title) { overflow: hidden; font-size: 11px; text-overflow: ellipsis; white-space: nowrap; }
  .registro-content--captura :deep(.face-scanner) { max-width: 100%; }
  .registro-choice { width: 100%; padding-inline: 0; }
  .registro-choice__action { width: 100% !important; }
  .registro-error :deep(.v-alert__content) { min-width: 0; width: 100%; }
  .registro-error :deep(.v-alert__close) { align-self: flex-start; }
  .registro-actions { gap: 8px; }
  .registro-actions .v-btn { min-width: 0; }
}

@media (max-width: 420px) {
  .registro-content :deep(.v-stepper-item__title) { font-size: 10px; }
  .registro-actions { flex-direction: column-reverse; }
  .registro-actions .v-btn { width: 100%; max-width: none; }
}
</style>
