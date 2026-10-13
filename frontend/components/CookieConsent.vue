<template>
  <section v-if="mostrar" class="cookie-consent" role="dialog" aria-modal="true" aria-labelledby="cookie-title">
    <div class="cookie-consent__content">
      <v-icon color="accent" size="30" aria-hidden="true">mdi-cookie-outline</v-icon>
      <div>
        <h2 id="cookie-title" class="cookie-consent__title">Uso de cookies</h2>
        <p class="cookie-consent__text">
          Usamos cookies necesarias para mantener tu sesión segura. Con tu autorización podremos usar
          cookies de preferencias para mejorar la experiencia. No usamos esta decisión para identificarte.
        </p>
      </div>
    </div>
    <div class="cookie-consent__actions">
      <v-btn variant="text" color="secondary" @click="rechazar">Rechazar opcionales</v-btn>
      <v-btn color="accent" class="umg-gold-button" @click="aceptar">Aceptar</v-btn>
    </div>
  </section>
</template>

<script setup lang="ts">
type Consentimiento = 'aceptado' | 'rechazado' | null

// Esta cookie registra la decisión; es necesaria para no volver a mostrar el aviso.
const consentimiento = useCookie<Consentimiento>('umg_cookie_consent', {
  default: () => null,
  maxAge: 60 * 60 * 24 * 180,
  sameSite: 'lax',
  secure: !import.meta.dev,
  path: '/'
})

const mostrar = computed(() => consentimiento.value === null)

function aceptar() {
  consentimiento.value = 'aceptado'
}

function rechazar() {
  consentimiento.value = 'rechazado'
}
</script>

<style scoped>
.cookie-consent {
  position: fixed;
  z-index: 3000;
  right: max(16px, env(safe-area-inset-right));
  bottom: max(16px, env(safe-area-inset-bottom));
  left: max(16px, env(safe-area-inset-left));
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  max-width: 860px;
  margin: 0 auto;
  padding: 18px 20px;
  color: #f7f9fb;
  background: #051b2e;
  border: 1px solid rgba(180, 139, 33, .68);
  border-radius: 14px;
  box-shadow: 0 14px 36px rgba(0, 0, 0, .3);
}
.cookie-consent__content { display: flex; align-items: flex-start; gap: 12px; }
.cookie-consent__title { margin: 0 0 4px; font-size: 16px; }
.cookie-consent__text { max-width: 570px; margin: 0; font-size: 13px; line-height: 1.45; }
.cookie-consent__actions { display: flex; flex: 0 0 auto; align-items: center; gap: 8px; }
@media (max-width: 620px) {
  .cookie-consent { flex-direction: column; align-items: stretch; gap: 14px; }
  .cookie-consent__actions { justify-content: flex-end; }
}
</style>
