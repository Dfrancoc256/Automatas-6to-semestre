// Las rutas no incluidas aquí requieren sesión.
const rutasPublicas = new Set(['/', '/login', '/registro', '/register/photo'])

export default defineNuxtRouteMiddleware(async (to) => {
  if (rutasPublicas.has(to.path)) return

  const { validarSesion } = useSessionGuard()
  if (!await validarSesion()) {
    return navigateTo({ path: '/login', query: { redirect: to.fullPath } })
  }
})
