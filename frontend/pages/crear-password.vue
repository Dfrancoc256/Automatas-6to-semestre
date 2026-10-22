<template>
  <main class="password-page">
    <v-card class="password-card pa-7" elevation="0">
      <img class="password-logo mb-5" src="/images/logo-umg-oficial.png" alt="Universidad Mariano Gálvez de Guatemala" />
      <h1>Crear contraseña personal</h1>

      <div v-if="verificando" class="mt-6">
        <v-progress-circular indeterminate color="primary" />
        <p class="mt-3">Verificando enlace…</p>
      </div>

      <template v-else-if="enlaceValido">
        <p class="password-lead">Elige una contraseña para ingresar a tu cuenta.</p>
        <form @submit.prevent="guardar">
          <v-text-field
            v-model="nuevoPassword"
            label="Nueva contraseña"
            type="password"
            autocomplete="new-password"
            hint="De 8 a 128 caracteres, con mayúscula, minúscula, número y símbolo"
            persistent-hint
            class="mt-3"
          />
          <v-text-field
            v-model="confirmacion"
            label="Confirmar nueva contraseña"
            type="password"
            autocomplete="new-password"
            class="mt-4"
          />
          <v-alert v-if="error" type="error" variant="tonal" density="compact" class="mt-4">{{ error }}</v-alert>
          <v-btn type="submit" block size="large" class="umg-gold-button mt-5" :loading="guardando">
            Guardar contraseña
          </v-btn>
        </form>
      </template>

      <template v-else>
        <v-alert type="error" variant="tonal" class="mt-6">El enlace expiró o ya fue utilizado.</v-alert>
        <p class="password-lead">Puedes solicitar otro desde «¿Olvidaste tu contraseña?» en la página de acceso.</p>
        <v-btn to="/login" variant="text" color="primary" class="mt-4">Ir al acceso</v-btn>
      </template>
    </v-card>
  </main>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
useHead({ title: 'Crear contraseña | Analizador Léxico', meta: [{ name: 'referrer', content: 'no-referrer' }] })

const route = useRoute()
const { api } = useApi()
const auth = useAuthStore()
const token = typeof route.query.token === 'string' ? route.query.token : ''
const verificando = ref(true)
const enlaceValido = ref(false)
const nuevoPassword = ref('')
const confirmacion = ref('')
const guardando = ref(false)
const error = ref('')

onMounted(async () => {
  if (token) {
    try {
      await api.get('/auth/validar-enlace-inicial', { params: { token } })
      enlaceValido.value = true
    } catch {
      enlaceValido.value = false
    }
  }
  verificando.value = false
})

async function guardar() {
  error.value = ''
  if (!/^(?=.{8,128}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).*$/.test(nuevoPassword.value)) {
    error.value = 'Usa entre 8 y 128 caracteres, con mayúscula, minúscula, número y símbolo.'
    return
  }
  if (nuevoPassword.value !== confirmacion.value) {
    error.value = 'La confirmación no coincide con la nueva contraseña.'
    return
  }

  guardando.value = true
  try {
    const { data } = await api.post('/auth/cambiar-password-inicial', {
      token,
      nuevoPassword: nuevoPassword.value
    })
    auth.login(data)
    await navigateTo('/')
  } catch (e: any) {
    const mensaje = e.response?.data?.mensaje
    if (mensaje === 'El enlace expiró o ya fue utilizado.') {
      enlaceValido.value = false
    } else {
      error.value = mensaje || 'No fue posible actualizar la contraseña. Intenta nuevamente.'
    }
  } finally {
    guardando.value = false
  }
}
</script>

<style scoped>
.password-page { display: grid; min-height: 100dvh; place-items: center; padding: 24px; background: #F5F7F9; }
.password-card { width: min(100%, 460px); text-align: center; border: 1px solid #DCE5EC; border-radius: 16px; background: #fff; }
.password-logo { width: 94px; height: 94px; object-fit: contain; }
.password-card h1 { margin: 0; color: #051B2E; font-size: 24px; }
.password-lead { margin: 12px 0 24px; color: #667085; font-size: 14px; line-height: 1.5; }
@media (max-width: 520px) { .password-page { align-items: start; padding: 16px; } .password-card { padding: 24px !important; } }
</style>
