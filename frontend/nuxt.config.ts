import vuetify from 'vite-plugin-vuetify'

export default defineNuxtConfig({
  devtools: { enabled: false },

  devServer: {
    port: Number(process.env.NUXT_PORT || 5000),
    host: process.env.NUXT_HOST || 'localhost'
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
      apiBase:            process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:8080/api',
      recaptchaSiteKey:   process.env.NUXT_PUBLIC_RECAPTCHA_SITE_KEY || ''
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
