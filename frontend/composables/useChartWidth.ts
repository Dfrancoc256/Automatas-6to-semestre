// Utilidades compartidas por los gráficos SVG del dashboard

/** Mide el ancho del contenedor para dibujar el SVG a tamaño real (texto nítido, sin deformarse) */
export function useChartWidth() {
  const wrap = ref<HTMLElement | null>(null)
  const width = ref(0)
  let ro: ResizeObserver | null = null

  onMounted(() => {
    if (!wrap.value) return
    width.value = wrap.value.clientWidth
    ro = new ResizeObserver((entries) => { width.value = Math.floor(entries[0]?.contentRect.width ?? 0) })
    ro.observe(wrap.value)
  })
  onBeforeUnmount(() => ro?.disconnect())

  return { wrap, width }
}

/** Eje Y con pasos "limpios" (1, 2, 5 × 10ⁿ) y unas 4 divisiones */
export function niceTicks(v: number) {
  // Conteos pequeños: escala fija 0–4 para que una barra de 1 no llene el gráfico
  if (v <= 4) return { max: 4, ticks: [0, 1, 2, 3, 4] }
  const bruto = Math.max(1, v) / 4
  const p = Math.pow(10, Math.floor(Math.log10(bruto)))
  const n = bruto / p
  const paso = Math.max(1, (n <= 1 ? 1 : n <= 2 ? 2 : n <= 5 ? 5 : 10) * p)
  const max = Math.max(paso, Math.ceil(Math.max(1, v) / paso) * paso)
  const ticks: number[] = []
  for (let t = 0; t <= max + 1e-9; t += paso) ticks.push(t)
  return { max, ticks }
}

export function formatNum(v: number) {
  return new Intl.NumberFormat('es-GT', { maximumFractionDigits: 1 }).format(v)
}

/** Rectángulo con esquinas superiores redondeadas (base recta sobre el eje) */
export function rectPath(x: number, y: number, w: number, h: number, r: number) {
  if (r <= 0) return `M${x},${y}h${w}v${h}h${-w}Z`
  return `M${x},${y + h}V${y + r}Q${x},${y} ${x + r},${y}H${x + w - r}Q${x + w},${y} ${x + w},${y + r}V${y + h}Z`
}
