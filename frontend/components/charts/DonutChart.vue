<template>
  <div class="donut">
    <div class="donut-svg">
      <svg viewBox="0 0 160 160" width="150" height="150" role="img" :aria-label="ariaLabel">
        <circle cx="80" cy="80" :r="r" fill="none" stroke="#EEF0F2" :stroke-width="grosor" />
        <path v-for="(a, i) in arcos" :key="a.label" :d="a.d" :fill="a.color"
              stroke="#fff" stroke-width="2" stroke-linejoin="round"
              :opacity="hover < 0 || hover === i ? 1 : 0.35"
              @mouseenter="hover = i" @mouseleave="hover = -1" style="cursor:default" />
        <text x="80" y="78" text-anchor="middle" class="centro-num">
          {{ formatNum(hover >= 0 ? items[hover]?.value ?? 0 : total) }}
        </text>
        <text x="80" y="96" text-anchor="middle" class="centro-lbl">
          {{ hover >= 0 ? items[hover]?.label : centerLabel }}
        </text>
      </svg>
    </div>
    <ul class="leyenda">
      <li v-for="(it, i) in items" :key="it.label" @mouseenter="hover = i" @mouseleave="hover = -1"
          :class="{ dim: hover >= 0 && hover !== i }">
        <span class="sw" :style="{ background: it.color }" />
        <span class="ley-lbl">{{ it.label }}</span>
        <span class="ley-val">{{ formatNum(it.value) }}</span>
        <span class="ley-pct">{{ total ? Math.round((it.value / total) * 100) : 0 }}%</span>
      </li>
      <li v-if="!items.length" class="text-medium-emphasis">Sin datos en el periodo</li>
    </ul>
  </div>
</template>

<script setup lang="ts">
interface Item { label: string; value: number; color: string }

const props = withDefaults(defineProps<{
  items: Item[]
  centerLabel?: string
  ariaLabel?: string
}>(), { centerLabel: 'Total', ariaLabel: 'Gráfico de anillo' })

const hover = ref(-1)
const r = 62, grosor = 20
const total = computed(() => props.items.reduce((a, b) => a + b.value, 0))

function punto(radio: number, ang: number) {
  return [80 + radio * Math.cos(ang), 80 + radio * Math.sin(ang)]
}

// Segmentos del anillo como áreas (permite separar con borde blanco de 2px)
const arcos = computed(() => {
  let ang = -Math.PI / 2
  const ro = r + grosor / 2, ri = r - grosor / 2
  return props.items.filter(i => i.value > 0).map(it => {
    const barrido = Math.min((it.value / (total.value || 1)) * Math.PI * 2, Math.PI * 2 - 0.0001)
    const a0 = ang, a1 = ang + barrido
    ang = a1
    const grande = barrido > Math.PI ? 1 : 0
    const [x0, y0] = punto(ro, a0), [x1, y1] = punto(ro, a1)
    const [x2, y2] = punto(ri, a1), [x3, y3] = punto(ri, a0)
    return {
      label: it.label, color: it.color,
      d: `M${x0},${y0}A${ro},${ro} 0 ${grande} 1 ${x1},${y1}L${x2},${y2}A${ri},${ri} 0 ${grande} 0 ${x3},${y3}Z`
    }
  })
})
</script>

<style scoped>
.donut { display: flex; align-items: center; gap: 20px; flex-wrap: wrap; }
.donut-svg { flex: none; }
.centro-num { font-size: 24px; font-weight: 800; fill: #051B2E; }
.centro-lbl { font-size: 11px; fill: #6B7280; text-transform: capitalize; }
.leyenda { list-style: none; padding: 0; margin: 0; flex: 1; min-width: 150px; }
.leyenda li { display: flex; align-items: center; gap: 8px; padding: 5px 0; font-size: 13px; border-bottom: 1px solid #F1F3F5; transition: opacity .15s; }
.leyenda li:last-child { border-bottom: 0; }
.leyenda li.dim { opacity: .45; }
.sw { width: 10px; height: 10px; border-radius: 3px; flex: none; }
.ley-lbl { flex: 1; text-transform: capitalize; color: #374151; }
.ley-val { font-weight: 700; color: #051B2E; font-variant-numeric: tabular-nums; }
.ley-pct { width: 38px; text-align: right; color: #6B7280; font-variant-numeric: tabular-nums; }
</style>
