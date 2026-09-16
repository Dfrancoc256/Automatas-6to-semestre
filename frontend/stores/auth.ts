import { defineStore } from 'pinia'

export interface AuthUser {
  token?: string
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
    isAuthenticated: (s) => !!s.user,
    isAdmin:        (s) => s.user?.rol === 'ADMIN',
    isSupervisor:   (s) => s.user?.rol === 'SUPERVISOR' || s.user?.rol === 'ADMIN',
    isAnalista:     (s) => !!s.user
  },

  actions: {
    login(data: AuthUser) {
      // El token se guarda exclusivamente en la cookie HttpOnly del backend.
      // Esta cookie solo mantiene el perfil necesario para la interfaz.
      const { token: _token, codigoQr: _codigoQr, ...perfil } = data
      this.user = perfil
      const sesion = useCookie<Omit<AuthUser, 'token' | 'codigoQr'> | null>('umg_session_meta', {
        maxAge: 60 * 60 * 8,
        sameSite: 'lax',
        secure: !import.meta.dev,
        path: '/'
      })
      sesion.value = perfil
    },
    logout() {
      this.user = null
      const sesion = useCookie('umg_session_meta', { path: '/' })
      sesion.value = null
    },
    restore() {
      const sesion = useCookie<Omit<AuthUser, 'token' | 'codigoQr'> | null>('umg_session_meta', {
        maxAge: 60 * 60 * 8,
        sameSite: 'lax',
        secure: !import.meta.dev,
        path: '/'
      })
      const data = sesion.value
      const exp = data ? new Date(data.expiracion) : null
      if (data && typeof data.rol === 'string' && typeof data.nickname === 'string'
        && exp && !Number.isNaN(exp.getTime()) && exp > new Date()) {
        this.user = data
      } else {
        this.user = null
        sesion.value = null
      }
    }
  }
})
