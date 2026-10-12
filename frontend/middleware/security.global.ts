// Lista explícita de rutas anónimas. Cualquier ruta nueva queda protegida por
// defecto hasta que se decida hacerla pública de forma consciente.
const rutasPublicas = new Set(['/', '/login', '/registro', '/register/photo'])

export default defineNuxtRouteMiddleware(async (to) => {
  if (rutasPublicas.has(to.path)) return

  const { validarSesion } = useSessionGuard()
  if (!await validarSesion()) {
    return navigateTo({ path: '/login', query: { redirect: to.fullPath } })
  }
})
