import axios from 'axios'

export function useApi() {
  const config = useRuntimeConfig()
  const authStore = useAuthStore()

  const client = axios.create({
    baseURL: config.public.apiBase,
    timeout: 30000
  })

  // Adjuntar JWT en cada request
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
        navigateTo('/')
      }
      return Promise.reject(err)
    }
  )

  return { api: client }
}
