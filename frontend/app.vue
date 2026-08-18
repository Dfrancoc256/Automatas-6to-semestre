<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>

<script setup lang="ts">
useHead({
  titleTemplate: (t) => t ? `${t} — UMG` : 'Universidad Mariano Gálvez',
  meta: [
    { name: 'description', content: 'Sistema de Análisis Léxico — Universidad Mariano Gálvez 2026' },
    { name: 'viewport', content: 'width=device-width, initial-scale=1' }
  ]
})
</script>
