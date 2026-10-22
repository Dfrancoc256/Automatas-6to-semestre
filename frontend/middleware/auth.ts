export default defineNuxtRouteMiddleware(async (to) => {
  const { validarSesion } = useSessionGuard()

  if (!await validarSesion()) {
    return navigateTo({ path: '/login', query: { redirect: to.fullPath } })
  }

  const auth = useAuthStore()
  if (auth.user?.debeCambiarPassword) return navigateTo('/login')
})
