// URL pública de fotografías guardadas por el backend en /uploads/fotos.
export function useMediaUrl() {
  function fotoUrl(ruta?: string | null): string | undefined {
    if (!ruta) return undefined

    // Compatibilidad con datos de usuarios anteriores y con URLs completas.
    if (/^data:image\//i.test(ruta) || /^https?:\/\//i.test(ruta)) return ruta

    const limpia = ruta.replace(/\\/g, '/').replace(/^\/+/, '')
    const coincidencia = limpia.match(/(?:^|\/)(?:uploads\/)?(fotos\/.+)$/i)
    if (!coincidencia) return undefined

    // La aplicación y el backend comparten el mismo dominio detrás de Nginx.
    // Una ruta relativa evita depender de una API_BASE configurada con otro host.
    return `/uploads/${coincidencia[1]}`
  }

  return { fotoUrl }
}
