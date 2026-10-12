export default defineNuxtRouteMiddleware(async (to) => {
  const { validarSesion } = useSessionGuard()

  if (!await validarSesion()) {
    return navigateTo({ path: '/login', query: { redirect: to.fullPath } })
  }
})
