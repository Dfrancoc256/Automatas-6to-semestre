<script setup lang="ts">
// Ruta heredada: la fotografía se completa dentro del registro unificado.
await navigateTo({ path: '/login', query: { modo: 'registro' } }, { replace: true })
</script>

<template>
  <span />
</template>
