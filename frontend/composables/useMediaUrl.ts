// URL pública de archivos guardados en /uploads.
export function useMediaUrl() {
  const config = useRuntimeConfig()

  function fotoUrl(ruta?: string | null): string | undefined {
    if (!ruta) return undefined
    const base = (config.public.apiBase as string).replace(/\/api\/?$/, '')
    const limpia = ruta.replace(/\\/g, '/').replace(/^\/?uploads\//, '')
    return `${base}/uploads/${limpia}`
  }

  return { fotoUrl }
}
