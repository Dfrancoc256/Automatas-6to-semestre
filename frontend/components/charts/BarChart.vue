<template>
  <div ref="wrap" class="chart-wrap" :style="{ height: `${height}px` }" @mouseleave="hover = -1">
    <svg v-if="width > 0" :width="width" :height="height" role="img" :aria-label="ariaLabel">
      <!-- Grid y eje Y -->
      <g v-for="t in ticks" :key="t">
        <line :x1="padL" :x2="width - padR" :y1="y(t)" :y2="y(t)" class="grid" />
        <text :x="padL - 6" :y="y(t) + 4" text-anchor="end" class="axis">{{ formatNum(t) }}</text>
      </g>

      <!-- Barras -->
      <g v-for="(lbl, i) in labels" :key="lbl + i">
        <rect :x="xBand(i)" :y="padT" :width="band" :height="plotH"
              class="hit" :class="{ on: hover === i }" @mouseenter="hover = i" />
        <template v-for="seg in segmentos(i)" :key="seg.key">
          <path :d="seg.d" :fill="seg.color" pointer-events="none" />
        </template>
        <text v-if="mostrarEtiqueta(i)" :x="xBand(i) + band / 2" :y="height - 6"
              text-anchor="middle" class="axis">{{ formatX(lbl) }}</text>
      </g>
      <line :x1="padL" :x2="width - padR" :y1="padT + plotH" :y2="padT + plotH" class="baseline" />
    </svg>

    <div v-if="hover >= 0" class="tip" :style="tipStyle">
      <div class="tip-title">{{ formatTip(labels[hover] ?? "") }}</div>
      <div v-for="s in series" :key="s.name" class="tip-row">
        <span class="sw" :style="{ background: s.color }" />
        <span class="tip-name">{{ s.name }}</span>
        <span class="tip-val">{{ formatNum(s.values[hover] || 0) }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Serie { name: string; color: string; values: number[] }

const props = withDefaults(defineProps<{
  labels: string[]
  series: Serie[]
  height?: number
  ariaLabel?: string
  formatX?: (v: string) => string
  formatTip?: (v: string) => string
}>(), {
  height: 240,
  ariaLabel: 'Gráfico de barras',
  formatX: (v: string) => v,
  formatTip: (v: string) => v
})

const { wrap, width } = useChartWidth()
const hover = ref(-1)

const padL = 36, padR = 8, padT = 10, padB = 24
const plotH = computed(() => props.height - padT - padB)
const plotW = computed(() => Math.max(0, width.value - padL - padR))

const totales = computed(() => props.labels.map((_, i) =>
  props.series.reduce((acc, s) => acc + (s.values[i] || 0), 0)))
const escala = computed(() => niceTicks(Math.max(0, ...totales.value)))
const max = computed(() => escala.value.max)
const ticks = computed(() => escala.value.ticks)

const paso = computed(() => plotW.value / Math.max(1, props.labels.length))
const band = computed(() => Math.max(2, Math.min(28, paso.value * 0.7)))
const xBand = (i: number) => padL + paso.value * i + (paso.value - band.value) / 2
const y = (v: number) => padT + plotH.value - (v / max.value) * plotH.value

// Separa etiquetas del eje X para que no se encimen
const cadaN = computed(() => Math.max(1, Math.ceil(props.labels.length / Math.max(1, Math.floor(plotW.value / 46)))))
const mostrarEtiqueta = (i: number) => i % cadaN.value === 0 || i === props.labels.length - 1 && props.labels.length <= 12

function segmentos(i: number) {
  const gap = 2
  const vivos = props.series.filter(s => (s.values[i] || 0) > 0)
  let base = 0
  return vivos.map((s, k) => {
    const v = s.values[i] || 0
    const y0 = y(base)
    const y1 = y(base + v)
    base += v
    const top = k === vivos.length - 1
    const h = Math.max(1, y0 - y1 - (k > 0 ? gap : 0))
    const yTop = y0 - (k > 0 ? gap : 0) - h
    return { key: s.name, color: s.color, d: rectPath(xBand(i), yTop, band.value, h, top ? Math.min(4, band.value / 2, h) : 0) }
  })
}

const tipStyle = computed(() => {
  const x = xBand(hover.value) + band.value / 2
  const izquierda = x > width.value - 160
  return { left: `${izquierda ? x - 12 : x + 12}px`, top: '8px', transform: izquierda ? 'translateX(-100%)' : 'none' }
})
</script>

<style scoped src="./chart.css"></style>
