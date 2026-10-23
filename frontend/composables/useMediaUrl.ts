// URL pública de fotografías guardadas por el backend en /uploads/fotos.
export function useMediaUrl() {
  const config = useRuntimeConfig()
  function fotoUrl(ruta?: string | null): string | undefined {
    if (!ruta) return undefined

    // Compatibilidad con datos de usuarios anteriores y con URLs completas.
    if (/^data:image\//i.test(ruta) || /^https?:\/\//i.test(ruta)) return ruta

    const limpia = ruta.replace(/\\/g, '/').replace(/^\/+/, '')
    const coincidencia = limpia.match(/(?:^|\/)(?:uploads\/)?(fotos\/.+)$/i)
    if (!coincidencia) return undefined

    if (import.meta.dev) {
      const base = (config.public.apiBase as string).replace(/\/api\/?$/, '')
      return `${base}/uploads/${coincidencia[1]}`
    }

    // En producción, frontend y backend comparten el dominio detrás de Nginx.
    return `/uploads/${coincidencia[1]}`
  }

  return { fotoUrl }
}
