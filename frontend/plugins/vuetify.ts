import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

// Paleta 60-30-10 inspirada en la identidad visual UMG
// 60% — marfil y gris azulado claro
// 30% — azul institucional profundo
// 10% — dorado institucional para acciones y énfasis

export default defineNuxtPlugin((nuxtApp) => {
  const vuetify = createVuetify({
    components,
    directives,
    icons: {
      defaultSet: 'mdi'
    },
    theme: {
      defaultTheme: 'light',
      themes: {
        light: {
          dark: false,
          colors: {
            background:     '#F5F6F7',
            surface:        '#FFFFFF',
            primary:        '#051B2E',
            secondary:      '#CB3332',
            accent:         '#B48B21',
            error:          '#CB3332',
            warning:        '#B48B21',
            info:           '#1A5080',
            success:        '#2E7D5B',
            'on-primary':   '#FFFFFF',
            'on-secondary': '#FFFFFF',
            'on-accent':    '#FFFFFF'
          }
        }
      }
    },
    defaults: {
      VBtn:       { color: 'accent', rounded: 'md', elevation: 0 },
      VCard:      { rounded: 'lg', elevation: 0 },
      VTextField: { variant: 'outlined', density: 'comfortable' },
      VSelect:    { variant: 'outlined', density: 'comfortable' }
    }
  })

  nuxtApp.vueApp.use(vuetify)
})
