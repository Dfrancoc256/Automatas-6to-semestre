<template>
  <div class="sticker-selector" :class="{ 'sticker-selector--compact': compact }">
    <p v-if="!compact" class="text-caption font-weight-bold mb-2">
      Agrega un sticker
    </p>

    <div class="sticker-list">
      <button
        v-for="sticker in stickers"
        :key="sticker.id"
        type="button"
        class="sticker-button"
        :class="{ seleccionado: modelValue === sticker.id }"
        :aria-pressed="modelValue === sticker.id"
        @click="seleccionar(sticker.id)"
      >
        <img v-if="compact && sticker.id !== 'ninguno'" class="sticker-imagen"
             :src="`/stickers/${sticker.id}.png`" alt="" />
        <span v-else class="sticker-emoji">{{ sticker.emoji }}</span>

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
  compact?: boolean
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
  border-color: #b48b21;
  background: #fffaf0;
  box-shadow: inset 0 0 0 1px #b48b21;
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
.sticker-selector--compact .sticker-list {
  flex-wrap: nowrap;
  justify-content: flex-start;
  overflow-x: auto;
  padding: 3px 2px 7px;
  scrollbar-width: thin;
  scrollbar-color: #b48b21 transparent;
  scroll-snap-type: x proximity;
}
.sticker-selector--compact .sticker-button {
  flex: 0 0 65px;
  min-width: 65px;
  height: 65px;
  padding: 7px 3px;
  scroll-snap-align: start;
}
.sticker-imagen { width: 29px; height: 29px; object-fit: contain; }
.sticker-selector--compact .sticker-emoji { display: grid; height: 29px; place-items: center; font-size: 22px; }
.sticker-selector--compact .sticker-nombre { font-size: 9px; }
</style>
