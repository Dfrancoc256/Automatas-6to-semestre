<template>
  <main class="credential-check">
    <v-card class="credential-check__card" elevation="0">
      <img src="/images/logo-umg-oficial.png" alt="Universidad Mariano Gálvez" class="credential-check__logo">
      <p class="credential-check__eyebrow">Universidad Mariano Gálvez</p>
      <h1>Verificación de credencial</h1>

      <div v-if="cargando" class="credential-check__state"><v-progress-circular indeterminate color="accent" size="36" /><p>Validando credencial…</p></div>
      <div v-else-if="credencial?.valido" class="credential-check__valid">
        <v-icon color="success" size="46">mdi-check-decagram</v-icon>
        <h2>Credencial válida</h2>
        <p>La credencial corresponde a la siguiente cuenta activa.</p>
        <dl>
          <div><dt>Nombre</dt><dd>{{ credencial.nombre }}</dd></div>
          <div><dt>Usuario</dt><dd>{{ credencial.nickname }}</dd></div>
          <div><dt>Rol</dt><dd>{{ credencial.rol }}</dd></div>
          <div><dt>Registro</dt><dd>{{ fecha(credencial.fechaRegistro) }}</dd></div>
        </dl>
      </div>
      <div v-else class="credential-check__invalid"><v-icon color="error" size="46">mdi-alert-circle-outline</v-icon><h2>No se pudo validar</h2><p>{{ error }}</p></div>
    </v-card>
  </main>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
useHead({ title: 'Verificación de credencial' })

const route = useRoute()
const { api } = useApi()
const cargando = ref(true)
const error = ref('El código QR no contiene una credencial válida.')
const credencial = ref<any>(null)

onMounted(async () => {
  const token = typeof route.query.token === 'string' ? route.query.token : ''
  if (!token) { cargando.value = false; return }
  try {
    const { data } = await api.get('/auth/verificar-credencial', { params: { token } })
    credencial.value = data
  } catch (e: any) {
    error.value = e.response?.data?.mensaje || 'No fue posible verificar la credencial.'
  } finally { cargando.value = false }
})

function fecha(valor: string) { return new Date(valor).toLocaleDateString('es-GT') }
</script>

<style scoped>
.credential-check { display: grid; min-height: 100dvh; padding: 24px; place-items: center; background: #F4F7FA; }
.credential-check__card { width: min(100%, 470px); padding: 38px; border: 1px solid #DFE6EC; border-top: 4px solid #B48B21; text-align: center; }
.credential-check__logo { width: 70px; height: 70px; object-fit: contain; }
.credential-check__eyebrow { margin: 14px 0 5px; color: #64748B; font-size: 12px; }
h1 { margin: 0; color: #051B2E; font-size: 24px; }
.credential-check__state, .credential-check__valid, .credential-check__invalid { margin-top: 30px; }
.credential-check__state p, .credential-check__valid p, .credential-check__invalid p { color: #64748B; line-height: 1.55; }
h2 { margin: 12px 0 6px; color: #051B2E; font-size: 20px; }
dl { margin: 24px 0 0; text-align: left; }
dl div { display: flex; justify-content: space-between; gap: 16px; padding: 11px 0; border-bottom: 1px solid #E5EAF0; }
dt { color: #64748B; font-size: 13px; } dd { margin: 0; color: #172B40; font-size: 13px; font-weight: 700; text-align: right; }
@media (max-width: 480px) { .credential-check { padding: 14px; } .credential-check__card { padding: 30px 22px; } }
</style>
