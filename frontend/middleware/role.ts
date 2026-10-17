/** Control de acceso por rol. */
export default defineNuxtRouteMiddleware(async (to) => {
  const auth = useAuthStore()
  const { validarSesion } = useSessionGuard()

  if (!await validarSesion()) {
    return navigateTo({ path: '/login', query: { redirect: to.fullPath } })
  }

  const rolesPermitidos = to.meta.roles as string[] | undefined
  if (rolesPermitidos?.length && !rolesPermitidos.includes(auth.user!.rol)) {
    return navigateTo('/dashboard')
  }
})
