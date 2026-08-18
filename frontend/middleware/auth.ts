export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore()
  authStore.restore()

  if (!authStore.isAuthenticated) {
    return navigateTo('/')
  }
})
