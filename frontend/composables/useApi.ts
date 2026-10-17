import axios from 'axios'

export function useApi() {
  const config = useRuntimeConfig()
  const authStore = useAuthStore()

  const client = axios.create({
    baseURL: config.public.apiBase,
    timeout: 30000,
    // Envía la cookie de sesión.
    withCredentials: true
  })

  // Compatibilidad con clientes que aún envían token.
  client.interceptors.request.use((req) => {
    if (authStore.user?.token) {
      req.headers.Authorization = `Bearer ${authStore.user.token}`
    }
    return req
  })

  // Manejo global de 401
  client.interceptors.response.use(
    (res) => res,
    (err) => {
      if (err.response?.status === 401) {
        authStore.logout()
        // El guard de navegación controla esta consulta.
        if (err.config?.url !== '/auth/sesion') {
          navigateTo('/login')
        }
      }
      return Promise.reject(err)
    }
  )

  return { api: client }
}
