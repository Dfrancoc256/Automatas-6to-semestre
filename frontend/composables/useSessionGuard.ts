// Valida la sesión en el servidor al navegar.
let validacionPendiente: Promise<boolean> | null = null
let sesionValidadaHasta = 0

export function useSessionGuard() {
  async function validarSesion() {
    const auth = useAuthStore()
    if (auth.isAuthenticated && Date.now() < sesionValidadaHasta) return true

    if (!validacionPendiente) {
      validacionPendiente = (async () => {
        try {
          const { api } = useApi()
          const { data } = await api.get('/auth/sesion')
          auth.establecerSesionVerificada(data)
          sesionValidadaHasta = Date.now() + 30_000
          return true
        } catch {
          sesionValidadaHasta = 0
          auth.logout()
          return false
        } finally {
          validacionPendiente = null
        }
      })()
    }

    return validacionPendiente
  }

  return { validarSesion }
}
