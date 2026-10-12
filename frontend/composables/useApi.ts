import axios from 'axios'

export function useApi() {
  const config = useRuntimeConfig()
  const authStore = useAuthStore()

  const client = axios.create({
    baseURL: config.public.apiBase,
    timeout: 30000,
    // Permite que el navegador envíe la cookie HttpOnly de sesión al API.
    withCredentials: true
  })

  // Compatibilidad temporal para clientes que aún entregan un token explícito.
  // La aplicación web usa la cookie HttpOnly emitida por el backend.
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
        // La verificación de navegación decide el destino y conserva la ruta
        // solicitada. Evitamos competir con ella cuando consulta /auth/sesion.
        if (err.config?.url !== '/auth/sesion') {
          navigateTo('/login')
        }
      }
      return Promise.reject(err)
    }
  )

  return { api: client }
}
