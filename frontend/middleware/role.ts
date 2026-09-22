/**
 * Restringe rutas administrativas sin depender únicamente del menú lateral.
 * El rol también se valida en el backend mediante JWT; esta capa evita que la
 * interfaz exponga módulos que no pertenecen a la sesión actual.
 */
export default defineNuxtRouteMiddleware((to) => {
  const auth = useAuthStore()
  auth.restore()

  if (!auth.isAuthenticated) {
    return navigateTo('/')
  }

  const rolesPermitidos = to.meta.roles as string[] | undefined
  if (rolesPermitidos?.length && !rolesPermitidos.includes(auth.user!.rol)) {
    return navigateTo('/dashboard')
  }
})
