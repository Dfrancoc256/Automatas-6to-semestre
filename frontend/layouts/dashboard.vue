<template>
  <div class="main-layout">
    <div v-if="sidebarOpen" class="sidebar-overlay" @click="sidebarOpen = false" />

    <aside :class="['sidebar', { open: sidebarOpen, collapsed: sidebarCollapsed }]" aria-label="Navegación principal">
      <div class="sidebar-logo">
        <div class="sidebar-logo-icon">
          <img src="/images/logo-umg-oficial.png" alt="Logo UMG" />
        </div>
        <div>
          <div class="sidebar-title">UMG · Académico</div>
          <div class="sidebar-subtitle">Lenguajes y autómatas</div>
        </div>
      </div>

      <nav class="sidebar-nav" @click="sidebarOpen = false">
        <div class="nav-root">
          <NuxtLink to="/dashboard" class="nav-item nav-item--root" title="Inicio" :class="{ active: route.path === '/dashboard' }">
            <span class="nav-item__icon"><v-icon size="18">mdi-home-variant-outline</v-icon></span>
            <span class="nav-item__label">Inicio</span>
          </NuxtLink>
        </div>

        <section class="nav-group">
          <button type="button" class="nav-section nav-section--toggle" :aria-expanded="gruposAbiertos.analisis" aria-controls="grupo-analisis" @click.stop="alternarGrupo('analisis')">
            <span>Análisis</span><v-icon size="15">{{ gruposAbiertos.analisis ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
          </button>
          <div id="grupo-analisis" v-show="gruposAbiertos.analisis" class="nav-tree">
            <NuxtLink to="/dashboard/analisis" class="nav-item" title="Análisis léxico"
                      :class="{ active: route.path === '/dashboard/analisis' }">
              <span class="nav-item__icon"><v-icon size="18">mdi-text-search</v-icon></span>
              <span class="nav-item__label">Análisis léxico</span>
            </NuxtLink>
            <NuxtLink to="/dashboard/historial" class="nav-item" title="Historial de análisis"
                      :class="{ active: route.path === '/dashboard/historial' }">
              <span class="nav-item__icon"><v-icon size="18">mdi-history</v-icon></span>
              <span class="nav-item__label">Historial de análisis</span>
            </NuxtLink>
          </div>
        </section>

        <template v-if="auth.isSupervisor">
          <section class="nav-group">
            <button type="button" class="nav-section nav-section--toggle" :aria-expanded="gruposAbiertos.supervision" aria-controls="grupo-supervision" @click.stop="alternarGrupo('supervision')">
              <span>Supervisión</span><v-icon size="15">{{ gruposAbiertos.supervision ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
            </button>
            <div id="grupo-supervision" v-show="gruposAbiertos.supervision" class="nav-tree">
              <NuxtLink to="/dashboard/reportes" class="nav-item" title="Estadísticas y reportes"
                        :class="{ active: route.path === '/dashboard/reportes' }">
                <span class="nav-item__icon"><v-icon size="18">mdi-chart-box-outline</v-icon></span>
                <span class="nav-item__label">Estadísticas</span>
              </NuxtLink>
            </div>
          </section>
        </template>

        <template v-if="auth.isAdmin">
          <section class="nav-group">
            <button type="button" class="nav-section nav-section--toggle" :aria-expanded="gruposAbiertos.administracion" aria-controls="grupo-administracion" @click.stop="alternarGrupo('administracion')">
              <span>Administración</span><v-icon size="15">{{ gruposAbiertos.administracion ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
            </button>
            <div id="grupo-administracion" v-show="gruposAbiertos.administracion" class="nav-tree">
              <NuxtLink to="/dashboard/usuarios" class="nav-item" title="Usuarios y roles"
                        :class="{ active: route.path === '/dashboard/usuarios' }">
                <span class="nav-item__icon"><v-icon size="18">mdi-account-group-outline</v-icon></span>
                <span class="nav-item__label">Usuarios</span>
              </NuxtLink>
              <NuxtLink to="/dashboard/bitacora" class="nav-item" title="Historial de accesos"
                        :class="{ active: route.path === '/dashboard/bitacora' }">
                <span class="nav-item__icon"><v-icon size="18">mdi-shield-check-outline</v-icon></span>
                <span class="nav-item__label">Historial de accesos</span>
              </NuxtLink>
            </div>
          </section>
        </template>

        <section class="nav-group">
          <button type="button" class="nav-section nav-section--toggle" :aria-expanded="gruposAbiertos.cuenta" aria-controls="grupo-cuenta" @click.stop="alternarGrupo('cuenta')">
            <span>Cuenta</span><v-icon size="15">{{ gruposAbiertos.cuenta ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
          </button>
          <div id="grupo-cuenta" v-show="gruposAbiertos.cuenta" class="nav-tree">
            <NuxtLink to="/dashboard/perfil" class="nav-item" title="Mi perfil"
                      :class="{ active: route.path === '/dashboard/perfil' }">
              <span class="nav-item__icon"><v-icon size="18">mdi-account-cog-outline</v-icon></span>
              <span class="nav-item__label">Mi perfil</span>
            </NuxtLink>
          </div>
        </section>
      </nav>

      <div class="sidebar-account">
        <NuxtLink to="/dashboard/perfil" class="sidebar-user" title="Abrir mi perfil" @click="sidebarOpen = false">
          <v-avatar size="38" color="accent">
            <span v-if="!fotoSidebarCargada" class="text-white text-sm font-bold">
              {{ auth.user?.nickname?.charAt(0).toUpperCase() }}
            </span>
            <img
              v-if="fotoSidebarUrl && !fotoSidebarError"
              :src="fotoSidebarUrl"
              class="sidebar-user-photo"
              alt=""
              @load="fotoSidebarCargada = true"
              @error="fotoSidebarError = true"
            >
          </v-avatar>
          <span class="sidebar-user__copy">
            <span class="sidebar-user-name">{{ auth.user?.nombre || auth.user?.nickname || 'Usuario UMG' }}</span>
            <span class="sidebar-user-role">{{ auth.user?.rol || 'USUARIO' }}</span>
          </span>
          <v-icon class="sidebar-user__arrow" size="17">mdi-chevron-right</v-icon>
        </NuxtLink>
        <v-btn class="sidebar-logout" variant="text" color="white" size="small" title="Cerrar sesión" aria-label="Cerrar sesión"
               prepend-icon="mdi-logout" @click="cerrarSesion">
          <span>Cerrar sesión</span>
        </v-btn>
      </div>
    </aside>

    <main :class="['page-content', { 'sidebar-collapsed': sidebarCollapsed, 'page-content--overview': route.path === '/dashboard' }]">
      <header class="dashboard-topbar">
        <button
          type="button"
          class="mobile-hamburger"
          :aria-expanded="sidebarOpen"
          :aria-label="sidebarOpen ? 'Cerrar menú de navegación' : 'Abrir menú de navegación'"
          @click="alternarSidebar"
        >
          <span />
          <span />
          <span />
        </button>
        <div class="dashboard-topbar__page">
          <span>Espacio académico</span>
          <strong>{{ paginaActual }}</strong>
        </div>
        <NuxtLink to="/dashboard/perfil" class="dashboard-topbar__profile" title="Abrir mi perfil">
          <v-avatar size="30" color="accent">
            <img v-if="fotoSidebarUrl && !fotoSidebarError" :src="fotoSidebarUrl" class="sidebar-user-photo" alt="" @error="fotoSidebarError = true">
            <span v-else>{{ auth.user?.nickname?.charAt(0).toUpperCase() }}</span>
          </v-avatar>
          <span>{{ auth.user?.nombre || auth.user?.nickname || 'Mi perfil' }}</span>
        </NuxtLink>
      </header>

      <slot />
    </main>
  </div>
</template>

<script setup lang="ts">
const auth  = useAuthStore()
const route = useRoute()
const { fotoUrl } = useMediaUrl()
const fotoSidebarUrl = computed(() => fotoUrl(auth.user?.fotoModificada))
const fotoSidebarCargada = ref(false)
const fotoSidebarError = ref(false)

const sidebarOpen = ref(false)
const sidebarCollapsed = ref(false)
const gruposAbiertos = reactive({ analisis: true, supervision: true, administracion: true, cuenta: true })
const paginaActual = computed(() => ({
  '/dashboard': 'Resumen',
  '/dashboard/analisis': 'Análisis léxico',
  '/dashboard/historial': 'Historial',
  '/dashboard/reportes': 'Estadísticas',
  '/dashboard/usuarios': 'Usuarios',
  '/dashboard/bitacora': 'Historial de accesos',
  '/dashboard/perfil': 'Mi perfil'
}[route.path] || 'Panel académico'))

watch(fotoSidebarUrl, () => {
  fotoSidebarCargada.value = false
  fotoSidebarError.value = false
}, { immediate: true })

onMounted(() => auth.restore())

function alternarSidebar() {
  if (window.innerWidth <= 768) {
    sidebarOpen.value = !sidebarOpen.value
    return
  }

  sidebarCollapsed.value = !sidebarCollapsed.value
}

function alternarGrupo(grupo: keyof typeof gruposAbiertos) {
  gruposAbiertos[grupo] = !gruposAbiertos[grupo]
}

async function cerrarSesion() {
  const { api } = useApi()
  try {
    await api.post('/auth/logout')
  } catch {
    // Limpia el estado local.
  }
  auth.logout()
  await navigateTo('/')
}
</script>

<style scoped>
.sidebar-overlay {
  position: fixed; inset: 0;
  background: rgba(0,0,0,.5);
  z-index: 99;
}

.mobile-hamburger {
  width: 48px;
  height: 48px;
  padding: 0;
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 5px;
  cursor: pointer;
  color: #051B2E;
  background: #FFFFFF;
  border: 1px solid #E2E4E8;
  border-radius: 10px;
}

.mobile-hamburger span {
  display: block;
  width: 20px;
  height: 2px;
  background: currentColor;
  border-radius: 2px;
  transition: transform .2s ease, opacity .2s ease;
}

.dashboard-topbar { min-height: 48px; }

@media (max-width: 768px) {
  .mobile-hamburger[aria-expanded='true'] span:nth-child(1) { transform: translateY(7px) rotate(45deg); }
  .mobile-hamburger[aria-expanded='true'] span:nth-child(2) { opacity: 0; }
  .mobile-hamburger[aria-expanded='true'] span:nth-child(3) { transform: translateY(-7px) rotate(-45deg); }
  .dashboard-topbar__profile > span { display: none; }
}

.items-center { align-items: center; }
.gap-3        { gap: 12px; }
.px-4         { padding-left: 16px; padding-right: 16px; }
.py-3         { padding-top: 12px; padding-bottom: 12px; }
.flex         { display: flex; }
.text-white   { color: white; }
.text-sm      { font-size: 13px; }
.font-bold    { font-weight: 700; }
.sidebar-user-photo { width: 100%; height: 100%; object-fit: cover; }
</style>
