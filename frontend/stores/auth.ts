import { defineStore } from 'pinia'

export interface AuthUser {
  token: string
  rol: string
  nickname: string
  fotoModificada: string | null
  expiracion: string
  codigoQr?: string
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as AuthUser | null
  }),

  getters: {
    isAuthenticated: (s) => !!s.user?.token,
    isAdmin:        (s) => s.user?.rol === 'ADMIN',
    isSupervisor:   (s) => s.user?.rol === 'SUPERVISOR' || s.user?.rol === 'ADMIN',
    isAnalista:     (s) => !!s.user
  },

  actions: {
    login(data: AuthUser) {
      this.user = data
      if (import.meta.client) {
        localStorage.setItem('auth', JSON.stringify(data))
      }
    },
    logout() {
      this.user = null
      if (import.meta.client) {
        localStorage.removeItem('auth')
      }
    },
    restore() {
      if (import.meta.client) {
        const raw = localStorage.getItem('auth')
        if (raw) {
          try {
            const data: AuthUser = JSON.parse(raw)
            const exp = new Date(data.expiracion)
            const esSesionValida = typeof data.token === 'string'
              && data.token.length > 0
              && typeof data.rol === 'string'
              && typeof data.nickname === 'string'
              && !Number.isNaN(exp.getTime())

            if (esSesionValida && exp > new Date()) {
              this.user = data
            } else {
              localStorage.removeItem('auth')
            }
          } catch {
            localStorage.removeItem('auth')
          }
        }
      }
    }
  }
})
