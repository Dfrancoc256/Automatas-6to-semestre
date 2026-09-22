import vuetify from 'vite-plugin-vuetify'

// Mantiene el archivo tipado aun cuando el proyecto no instala los tipos de
// Node de forma explícita; Nuxt evalúa esta configuración en el servidor.
const environment = (globalThis as typeof globalThis & {
  process?: { env?: Record<string, string | undefined> }
}).process?.env ?? {}

export default defineNuxtConfig({
  devtools: { enabled: false },

  devServer: {
    port: Number(environment.NUXT_PORT || 5000),
    host: environment.NUXT_HOST || 'localhost'
  },

  build: {
    transpile: ['vuetify']
  },

  modules: [
    '@pinia/nuxt',
    (_options, nuxt) => {
      nuxt.hooks.hook('vite:extendConfig', (config) => {
        config.plugins?.push(vuetify({ autoImport: false }))
      })
    }
  ],

  vite: {
    define: {
      'process.env.DEBUG': 'false'
    }
  },

  runtimeConfig: {
    public: {
      apiBase:            environment.NUXT_PUBLIC_API_BASE || 'http://localhost:8080/api',
      recaptchaSiteKey:   environment.NUXT_PUBLIC_RECAPTCHA_SITE_KEY || ''
    }
  },

  css: [
    'vuetify/styles',
    '@mdi/font/css/materialdesignicons.min.css',
    '~/assets/main.css'
  ],

  ssr: false,

  compatibilityDate: '2024-11-01'
})
