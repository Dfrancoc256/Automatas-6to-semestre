<template>
  <div ref="wrap" class="chart-wrap" :style="{ height: `${height}px` }" @mouseleave="hover = -1">
    <svg v-if="width > 0" :width="width" :height="height" role="img" :aria-label="ariaLabel"
         @mousemove="onMove">
      <defs>
        <linearGradient :id="gradId" x1="0" x2="0" y1="0" y2="1">
          <stop offset="0%" :stop-color="color" stop-opacity="0.22" />
          <stop offset="100%" :stop-color="color" stop-opacity="0" />
        </linearGradient>
      </defs>

      <g v-for="t in ticks" :key="t">
        <line :x1="padL" :x2="width - padR" :y1="y(t)" :y2="y(t)" class="grid" />
        <text :x="padL - 6" :y="y(t) + 4" text-anchor="end" class="axis">{{ formatNum(t) }}</text>
      </g>

      <path :d="areaPath" :fill="`url(#${gradId})`" />
      <path :d="linePath" fill="none" :stroke="color" stroke-width="2" stroke-linejoin="round" stroke-linecap="round" />

      <template v-for="(lbl, i) in labels" :key="lbl + i">
        <text v-if="i % cadaN === 0" :x="x(i)" :y="height - 6" text-anchor="middle" class="axis">{{ formatX(lbl) }}</text>
      </template>
      <line :x1="padL" :x2="width - padR" :y1="padT + plotH" :y2="padT + plotH" class="baseline" />

      <!-- Punto máximo etiquetado -->
      <g v-if="iMax >= 0 && hover < 0">
        <circle :cx="x(iMax)" :cy="y(values[iMax] ?? 0)" r="4" :fill="color" stroke="#fff" stroke-width="2" />
        <text :x="x(iMax)" :y="y(values[iMax] ?? 0) - 9" text-anchor="middle" class="axis" style="font-weight:700;fill:#051B2E">
          {{ formatNum(values[iMax] ?? 0) }}
        </text>
      </g>

      <g v-if="hover >= 0">
        <line :x1="x(hover)" :x2="x(hover)" :y1="padT" :y2="padT + plotH" class="crosshair" />
        <circle :cx="x(hover)" :cy="y(values[hover] ?? 0)" r="5" :fill="color" stroke="#fff" stroke-width="2" />
      </g>
    </svg>

    <div v-if="hover >= 0" class="tip" :style="tipStyle">
      <div class="tip-title">{{ formatTip(labels[hover] ?? "") }}</div>
      <div class="tip-row">
        <span class="sw" :style="{ background: color }" />
        <span class="tip-name">{{ serieName }}</span>
        <span class="tip-val">{{ formatNum(values[hover] ?? 0) }}</span>
      </div>
      <div v-if="extra" class="tip-row" style="color:#6B7280">{{ extra(hover) }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
  labels: string[]
  values: number[]
  serieName: string
  color?: string
  height?: number
  ariaLabel?: string
  formatX?: (v: string) => string
  formatTip?: (v: string) => string
  extra?: (i: number) => string
}>(), {
  color: '#2a78d6',
  height: 240,
  ariaLabel: 'Gráfico de líneas',
  formatX: (v: string) => v,
  formatTip: (v: string) => v
})

const { wrap, width } = useChartWidth()
const hover = ref(-1)
const gradId = `area-${Math.random().toString(36).slice(2, 9)}`

const padL = 36, padR = 12, padT = 18, padB = 24
const plotH = computed(() => props.height - padT - padB)
const plotW = computed(() => Math.max(0, width.value - padL - padR))
const escala = computed(() => niceTicks(Math.max(0, ...props.values)))
const max = computed(() => escala.value.max)
const ticks = computed(() => escala.value.ticks)

const x = (i: number) => padL + (props.labels.length <= 1 ? plotW.value / 2 : (plotW.value * i) / (props.labels.length - 1))
const y = (v: number) => padT + plotH.value - (v / max.value) * plotH.value
const cadaN = computed(() => Math.max(1, Math.ceil(props.labels.length / Math.max(1, Math.floor(plotW.value / 52)))))

const linePath = computed(() => props.values.map((v, i) => `${i ? 'L' : 'M'}${x(i)},${y(v)}`).join(''))
const areaPath = computed(() => props.values.length
  ? `${linePath.value}L${x(props.values.length - 1)},${padT + plotH.value}L${x(0)},${padT + plotH.value}Z`
  : '')

const iMax = computed(() => {
  const m = Math.max(...props.values, 0)
  return m > 0 ? props.values.lastIndexOf(m) : -1
})

function onMove(e: MouseEvent) {
  const rect = (e.currentTarget as SVGElement).getBoundingClientRect()
  const px = e.clientX - rect.left
  if (px < padL - 10 || px > width.value - padR + 10 || !props.labels.length) { hover.value = -1; return }
  const n = props.labels.length - 1
  hover.value = n <= 0 ? 0 : Math.min(n, Math.max(0, Math.round(((px - padL) / plotW.value) * n)))
}

const tipStyle = computed(() => {
  const px = x(hover.value)
  const izquierda = px > width.value - 170
  return { left: `${izquierda ? px - 12 : px + 12}px`, top: '8px', transform: izquierda ? 'translateX(-100%)' : 'none' }
})
</script>

<style scoped src="./chart.css"></style>
