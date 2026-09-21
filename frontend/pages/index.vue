<template>
  <main class="landing-page">
    <header class="landing-nav">
      <NuxtLink to="/" class="landing-brand" aria-label="Inicio">
        <img src="/images/logo-umg-oficial.png" alt="Universidad Mariano Gálvez de Guatemala">
        <span><strong>Universidad Mariano Gálvez</strong><small>Analizador léxico</small></span>
      </NuxtLink>

      <nav class="landing-links" aria-label="Navegación principal">
        <a href="#proceso">Proceso</a><a href="#capacidades">Capacidades</a><a href="#acceso">Acceso</a>
      </nav>
      <div class="landing-nav__actions">
        <v-btn to="/registro" class="landing-register" variant="text">Crear cuenta</v-btn>
        <v-btn to="/login" class="landing-login" variant="outlined" prepend-icon="mdi-login">Ingresar</v-btn>
      </div>
    </header>

<<<<<<< HEAD
      <v-form @submit.prevent="loginPassword" class="login-form">
        <label class="login-field-label" for="identificador">Correo o nickname</label>
        <v-text-field
          id="identificador"
          v-model="form.identificador"
          placeholder="tu@correo.com o nickname"
          prepend-inner-icon="mdi-account"
          :error="!!error"
          hide-details
          base-color="primary"
          color="primary"
          class="login-password-field mb-4"
          @update:model-value="error = ''"
        />

        <label class="login-field-label" for="clave-acceso">Clave de acceso</label>
        <v-text-field
          id="clave-acceso"
          v-model="form.password"
          placeholder="Ingresa tu clave"
          prepend-inner-icon="mdi-lock"
          :type="mostrarPass ? 'text' : 'password'"
          :append-inner-icon="mostrarPass ? 'mdi-eye-off' : 'mdi-eye'"
          @click:append-inner="mostrarPass = !mostrarPass"
          :error="!!error"
          hide-details
          base-color="primary"
          color="primary"
          class="login-password-field"
          @update:model-value="error = ''"
        />

        <p v-if="error" class="login-field-error">
          <v-icon size="15" start>mdi-alert-circle-outline</v-icon>{{ error }}
        </p>

        <v-btn
          type="submit"
          color="accent"
          class="umg-gold-button login-access-button mt-10"
          block
          size="large"
          :loading="cargando"
          prepend-icon="mdi-login"
        >
          Ingresar
        </v-btn>
      </v-form>

      <div class="text-center mt-4">
        <v-divider class="my-4" />
        <v-btn
          variant="outlined"
          color="primary"
          block
          prepend-icon="mdi-face-recognition"
          @click="abrirLoginFacial"
        >
          Ingresar con reconocimiento facial
        </v-btn>
      </div>

      <div class="text-center mt-5">
        <v-btn variant="text" color="primary" size="small" @click="mostrarReset = true">
          ¿Olvidaste tu contraseña?
        </v-btn>
      </div>

      <div class="text-center mt-2">
        <span class="text-body-2 text-medium-emphasis">¿No tienes cuenta? </span>
        <NuxtLink to="/registro" class="font-weight-bold" style="color:#B48B21">Crear cuenta</NuxtLink>
      </div>
    </v-card>

    <v-dialog v-model="mostrarReset" max-width="420">
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">Restablecer contraseña</v-card-title>
        <v-card-text class="px-0">
          La recuperación de contraseña estará disponible al conectar la base de datos.
          Por el momento utiliza la clave temporal asignada.
        </v-card-text>
        <v-card-actions class="px-0 justify-end">
          <v-btn class="umg-gold-button" color="accent" @click="mostrarReset = false">Entendido</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="mostrarFacial" max-width="440" @update:model-value="onCerrarFacial">
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">Reconocimiento facial</v-card-title>
        <v-card-text class="px-0 text-center">
          <div class="webcam-container mb-3">
            <video ref="videoFacialEl" autoplay playsinline
                   style="width:100%;border-radius:8px;object-fit:cover" />
          </div>
          <canvas ref="canvasFacialEl" style="display:none" width="640" height="480" />

          <p class="text-body-2 text-medium-emphasis mb-2">
            Coloca tu rostro frente a la cámara y presiona "Verificar".
          </p>

          <v-alert v-if="errorFacial" type="error" variant="tonal" density="compact" class="mb-2">
            {{ errorFacial }}
          </v-alert>
        </v-card-text>
        <v-card-actions class="px-0 justify-end">
          <v-btn variant="outlined" @click="mostrarFacial = false">Cancelar</v-btn>
          <v-btn class="umg-gold-button" color="accent" :loading="verificandoFacial"
                 prepend-icon="mdi-camera-iris" @click="verificarRostro">
            Verificar
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
=======
    <section class="landing-hero" aria-labelledby="landing-title">
      <div class="landing-hero__media" aria-hidden="true">
        <span class="landing-hero__image landing-hero__image--one" />
        <span class="landing-hero__image landing-hero__image--two" />
        <span class="landing-hero__image landing-hero__image--three" />
      </div>
      <div class="landing-hero__overlay" />
      <div class="landing-hero__content landing-reveal">
        <p class="landing-eyebrow"><span /> Plataforma académica</p>
        <h1 id="landing-title">Convierte texto en<br><span>conocimiento útil.</span></h1>
        <p class="landing-lead">Analiza documentos <strong>.txt</strong>, identifica componentes léxicos y conserva resultados trazables para tu proceso académico.</p>
        <div class="landing-actions">
          <v-btn to="/login" class="umg-gold-button landing-primary" size="large" prepend-icon="mdi-arrow-right">Iniciar análisis</v-btn>
          <v-btn to="/registro" class="landing-register-cta" variant="text" append-icon="mdi-arrow-right">Crear una cuenta</v-btn>
        </div>
        <div class="landing-stats" aria-label="Características principales">
          <div><strong>5</strong><span>idiomas disponibles</span></div>
          <div><strong>.txt</strong><span>formato de análisis</span></div>
          <div><strong>100%</strong><span>historial consultable</span></div>
        </div>
      </div>
    </section>

    <section id="proceso" class="landing-section landing-process">
      <div class="landing-section__intro landing-reveal"><p class="landing-section__label">01 · Proceso</p><h2>Una ruta clara, desde el archivo hasta el resultado.</h2></div>
      <div class="landing-steps">
        <article class="landing-reveal"><v-icon>mdi-file-upload-outline</v-icon><span>01</span><h3>Carga</h3><p>Selecciona tu documento de texto y el idioma que deseas procesar.</p></article>
        <article class="landing-reveal"><v-icon>mdi-text-search</v-icon><span>02</span><h3>Analiza</h3><p>El sistema clasifica palabras, pronombres, verbos y patrones del contenido.</p></article>
        <article class="landing-reveal"><v-icon>mdi-chart-box-outline</v-icon><span>03</span><h3>Consulta</h3><p>Revisa y conserva tus hallazgos desde tu historial personal.</p></article>
      </div>
    </section>

    <section id="capacidades" class="landing-section landing-capabilities">
      <div class="landing-capabilities__panel landing-reveal">
        <p class="landing-section__label">02 · Capacidades</p><h2>Lecturas que dejan evidencia.</h2>
        <p>Una vista ordenada para identificar la estructura del texto, no solo contar palabras.</p>
        <ul><li><v-icon>mdi-check-circle-outline</v-icon> Frecuencias y palabras poco comunes</li><li><v-icon>mdi-check-circle-outline</v-icon> Sustantivos, verbos y pronombres</li><li><v-icon>mdi-check-circle-outline</v-icon> Patrones léxicos por idioma</li></ul>
      </div>
      <div class="landing-results landing-reveal" aria-label="Resumen de resultados">
        <div class="landing-result-card landing-result-card--large"><span>Resultado de análisis</span><strong>Palabras clave</strong><div class="landing-bars"><i /><i /><i /><i /></div></div>
        <div class="landing-result-card"><v-icon>mdi-translate</v-icon><strong>Idiomas</strong><small>ES · EN · RU · ZH · AR</small></div>
        <div class="landing-result-card"><v-icon>mdi-history</v-icon><strong>Historial</strong><small>Consulta tus resultados cuando los necesites.</small></div>
      </div>
    </section>

    <section id="acceso" class="landing-cta landing-reveal"><div><p class="landing-section__label">03 · Acceso</p><h2>Tu próxima lectura empieza aquí.</h2><p>Crea tu cuenta para acceder al panel académico o inicia sesión si ya cuentas con credenciales.</p></div><div class="landing-cta__actions"><v-btn to="/registro" class="umg-gold-button" size="large" prepend-icon="mdi-account-plus-outline">Crear cuenta</v-btn><v-btn to="/login" class="landing-login--inverse" variant="outlined" size="large" prepend-icon="mdi-login">Ingresar</v-btn></div></section>
    <footer class="landing-footer"><span>Universidad Mariano Gálvez de Guatemala</span><span>Analizador léxico · {{ new Date().getFullYear() }}</span></footer>
  </main>
>>>>>>> origin/main
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
useHead({ title: 'Analizador Léxico | Universidad Mariano Gálvez' })

<<<<<<< HEAD
const auth     = useAuthStore()
const { api }  = useApi()
const { cargarModelos, obtenerDescriptor } = useFacialRecognition()

const mostrarPass  = ref(false)
const cargando     = ref(false)
const error        = ref('')
const mostrarReset = ref(false)
const accesoExitoso = ref(false)

// Login facial
const mostrarFacial    = ref(false)
const verificandoFacial = ref(false)
const errorFacial      = ref('')
const videoFacialEl    = ref<HTMLVideoElement | null>(null)
const canvasFacialEl   = ref<HTMLCanvasElement | null>(null)
let   streamFacial: MediaStream | null = null

const form = reactive({ identificador: '', password: '' })
=======
let revealObserver: IntersectionObserver | undefined
>>>>>>> origin/main

onMounted(() => {
  const sections = document.querySelectorAll<HTMLElement>('.landing-reveal')
  revealObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add('is-visible')
        revealObserver?.unobserve(entry.target)
      }
    })
  }, { threshold: 0.14 })
  sections.forEach(section => revealObserver?.observe(section))
})

<<<<<<< HEAD
async function loginPassword() {
  error.value = ''
  if (!form.identificador.trim() || !form.password.trim()) {
    error.value = 'Ingresa tu correo/nickname y tu clave para continuar.'
    return
  }

  cargando.value = true
  try {
    const { data } = await api.post('/auth/login', {
      identificador:  form.identificador.trim(),
      password:       form.password,
      recaptchaToken: import.meta.dev ? 'dev-bypass' : ''
    })

    accesoExitoso.value = true
    auth.login(data)
    await new Promise(resolve => setTimeout(resolve, 650))
    await navigateTo('/dashboard')
  } catch (e: any) {
    error.value = e.response?.data?.mensaje || 'La clave de acceso no es correcta.'
  } finally {
    cargando.value = false
  }
}

async function abrirLoginFacial() {
  errorFacial.value = ''
  mostrarFacial.value = true
  cargarModelos()
  await nextTick()
  try {
    streamFacial = await navigator.mediaDevices.getUserMedia({ video: true })
    if (videoFacialEl.value) videoFacialEl.value.srcObject = streamFacial
  } catch {
    errorFacial.value = 'No se pudo acceder a la cámara.'
  }
}

function detenerCamaraFacial() {
  streamFacial?.getTracks().forEach(t => t.stop())
  streamFacial = null
}

function onCerrarFacial(abierto: boolean) {
  if (!abierto) detenerCamaraFacial()
}

async function verificarRostro() {
  if (!videoFacialEl.value || !canvasFacialEl.value) return
  errorFacial.value = ''
  verificandoFacial.value = true

  try {
    const ctx = canvasFacialEl.value.getContext('2d')!
    ctx.drawImage(videoFacialEl.value, 0, 0, 640, 480)

    const descriptor = await obtenerDescriptor(canvasFacialEl.value)
    if (!descriptor) {
      errorFacial.value = 'No se detectó un rostro. Acércate e inténtalo de nuevo.'
      return
    }

    const { data } = await api.post('/auth/login-facial', {
      descriptor,
      recaptchaToken: import.meta.dev ? 'dev-bypass' : ''
    })

    auth.login(data)
    mostrarFacial.value = false
    detenerCamaraFacial()
    await navigateTo('/dashboard')
  } catch (e: any) {
    errorFacial.value = e.response?.data?.mensaje || 'Rostro no reconocido.'
  } finally {
    verificandoFacial.value = false
  }
}
=======
onBeforeUnmount(() => revealObserver?.disconnect())
>>>>>>> origin/main
</script>
