<template>
  <div class="sticker-selector">
    <p class="text-caption font-weight-bold mb-2">
      Agrega un sticker
    </p>

    <div class="sticker-list">
      <button
        v-for="sticker in stickers"
        :key="sticker.id"
        type="button"
        class="sticker-button"
        :class="{ seleccionado: modelValue === sticker.id }"
        @click="seleccionar(sticker.id)"
      >
        <span class="sticker-emoji">
          {{ sticker.emoji }}
        </span>

        <span class="sticker-nombre">
          {{ sticker.nombre }}
        </span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  modelValue: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const stickers = [
  {
    id: 'ninguno',
    nombre: 'Ninguno',
    emoji: '🚫'
  },
  {
    id: 'graduacion',
    nombre: 'Graduación',
    emoji: '🎓'
  },
  {
    id: 'lentes',
    nombre: 'Lentes',
    emoji: '🤓'
  },
  {
    id: 'corona',
    nombre: 'Corona',
    emoji: '👑'
  },
  {
    id: 'estrella',
    nombre: 'Estrella',
    emoji: '⭐'
  },
  {
    id: 'corazon',
    nombre: 'Corazón',
    emoji: '❤️'
  }
]

function seleccionar(id: string) {
  emit('update:modelValue', id)
}
</script>

<style scoped>
.sticker-selector {
  width: 100%;
}

.sticker-list {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 8px;
}

.sticker-button {
  min-width: 72px;
  padding: 8px 10px;

  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3px;

  background: white;
  border: 1px solid #d9dde3;
  border-radius: 10px;

  cursor: pointer;
  transition: 0.2s ease;
}

.sticker-button:hover {
  border-color: #b48b21;
  transform: translateY(-1px);
}

.sticker-button.seleccionado {
  border: 2px solid #b48b21;
  background: #fffaf0;
}

.sticker-emoji {
  font-size: 25px;
  line-height: 1;
}

.sticker-nombre {
  font-size: 10px;
  color: #051b2e;
  font-weight: 600;
}
</style>