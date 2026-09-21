<script setup lang="ts">
// Compatibilidad con enlaces antiguos: el registro forma parte de /login.
await navigateTo({ path: '/login', query: { modo: 'registro' } }, { replace: true })
</script>

<template>
  <span />
</template>
