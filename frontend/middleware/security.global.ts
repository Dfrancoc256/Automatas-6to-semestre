// Las rutas no incluidas aquí requieren sesión.
const rutasPublicas = new Set(['/', '/login', '/registro', '/register/photo'])

export default defineNuxtRouteMiddleware(async (to) => {
  if (rutasPublicas.has(to.path)) return

  const { validarSesion } = useSessionGuard()
  if (!await validarSesion()) {
    return navigateTo({ path: '/login', query: { redirect: to.fullPath } })
  }

  // La cookie temporal sigue siendo válida, pero no concede acceso al sistema.
  // Redirigimos al formulario obligatorio para que el usuario complete el cambio.
  const auth = useAuthStore()
  if (auth.user?.debeCambiarPassword) {
    return navigateTo('/login')
  }
})
