<template>
  <main class="reset-page">
    <v-card class="reset-card pa-7" elevation="0">
      <img class="reset-logo mb-5" src="/images/logo-umg-oficial.png" alt="Universidad Mariano Gálvez de Guatemala" />
      <h1>Crear nueva contraseña</h1>
      <p class="reset-card__lead">Define una contraseña segura para volver a ingresar a tu cuenta.</p>

      <template v-if="!completado">
        <v-text-field
          v-model="password"
          label="Nueva contraseña"
          :type="verPassword ? 'text' : 'password'"
          :append-inner-icon="verPassword ? 'mdi-eye-off' : 'mdi-eye'"
          autocomplete="new-password"
          hint="Mínimo 8 caracteres, mayúscula, minúscula, número y símbolo"
          persistent-hint
          @click:append-inner="verPassword = !verPassword"
        />
        <v-text-field
          v-model="confirmacion"
          class="mt-3"
          label="Confirmar nueva contraseña"
          :type="verConfirmacion ? 'text' : 'password'"
          :append-inner-icon="verConfirmacion ? 'mdi-eye-off' : 'mdi-eye'"
          autocomplete="new-password"
          @click:append-inner="verConfirmacion = !verConfirmacion"
        />
        <v-alert v-if="error" type="error" variant="tonal" density="compact" class="mt-4">{{ error }}</v-alert>
        <v-btn block size="large" class="umg-gold-button mt-5" :loading="guardando" @click="restablecer">
          Guardar nueva contraseña
        </v-btn>
      </template>

      <v-alert v-else type="success" variant="tonal" class="mt-4">
        Contraseña actualizada. Ya puedes iniciar sesión.
      </v-alert>
      <v-btn v-if="completado" block variant="outlined" color="primary" class="mt-5" @click="navigateTo('/login')">
        Ir a iniciar sesión
      </v-btn>
      <v-btn variant="text" color="primary" class="mt-3" @click="navigateTo('/login')">Volver al acceso</v-btn>
    </v-card>
  </main>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
useHead({ title: 'Restablecer contraseña | Analizador Léxico' })

const route = useRoute()
const { api } = useApi()
const token = computed(() => typeof route.query.token === 'string' ? route.query.token : '')
const password = ref('')
const confirmacion = ref('')
const verPassword = ref(false)
const verConfirmacion = ref(false)
const guardando = ref(false)
const completado = ref(false)
const error = ref(token.value ? '' : 'El enlace no es válido. Solicita uno nuevo desde la pantalla de acceso.')

async function restablecer() {
  error.value = ''
  const cumpleRegla = /^(?=.{8,128}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).*$/.test(password.value)
  if (!token.value) {
    error.value = 'El enlace no es válido. Solicita uno nuevo.'
    return
  }
  if (!cumpleRegla) {
    error.value = 'Usa entre 8 y 128 caracteres, con mayúscula, minúscula, número y símbolo.'
    return
  }
  if (password.value !== confirmacion.value) {
    error.value = 'La confirmación no coincide con la nueva contraseña.'
    return
  }

  guardando.value = true
  try {
    await api.post('/auth/restablecer-password', { token: token.value, nuevoPassword: password.value })
    completado.value = true
  } catch (e: any) {
    error.value = e.response?.data?.mensaje || 'No fue posible actualizar la contraseña.'
  } finally {
    guardando.value = false
  }
}
</script>

<style scoped>
.reset-page { display: grid; min-height: 100dvh; place-items: center; padding: 24px; background: #F5F7F9; }
.reset-card { width: min(100%, 440px); text-align: center; border: 1px solid #DCE5EC; border-radius: 16px; background: #fff; }
.reset-logo { width: 94px; height: 94px; object-fit: contain; filter: drop-shadow(0 9px 12px rgba(5, 27, 46, .16)); }
.reset-card h1 { margin: 0; color: #051B2E; font-size: 24px; }
.reset-card__lead { margin: 10px 0 24px; color: #667085; font-size: 14px; line-height: 1.5; }
@media (max-width: 520px) { .reset-page { align-items: start; padding: 16px; } .reset-card { padding: 24px !important; } }
</style>
