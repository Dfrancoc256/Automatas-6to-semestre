<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
  <CookieConsent />
</template>

<script setup lang="ts">
useHead({
  titleTemplate: (t) => t ? `${t} — UMG` : 'Universidad Mariano Gálvez',
  meta: [
    { name: 'description', content: 'Sistema de Análisis Léxico — Universidad Mariano Gálvez 2026' },
    { name: 'viewport', content: 'width=device-width, initial-scale=1' }
  ],
  link: [
    // Fuerza el emblema institucional como ícono de la pestaña y evita
    // que el navegador reutilice el favicon heredado del proyecto.
    { rel: 'icon', type: 'image/png', href: '/images/logo-umg-oficial.png' }
  ]
})
</script>
