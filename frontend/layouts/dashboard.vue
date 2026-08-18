<template>
  <div class="main-layout">
    <!-- Overlay móvil -->
    <div v-if="sidebarOpen" class="sidebar-overlay" @click="sidebarOpen = false" />

    <!-- Sidebar -->
    <aside :class="['sidebar', { open: sidebarOpen, collapsed: sidebarCollapsed }]">
      <div class="sidebar-logo">
        <div class="sidebar-logo-icon">
          <img src="/images/logo-umg-oficial.png" alt="Logo UMG" />
        </div>
        <div>
          <div class="sidebar-title">Universidad Mariano Gálvez</div>
          <div class="sidebar-subtitle">Sistema académico · 2026</div>
        </div>
      </div>

      <!-- Avatar de usuario -->
      <div class="sidebar-user px-4 py-3 flex items-center gap-3">
        <v-avatar size="36" color="accent">
          <v-img v-if="auth.user?.fotoModificada" :src="auth.user.fotoModificada" />
          <span v-else class="text-white text-sm font-bold">
            {{ auth.user?.nickname?.charAt(0).toUpperCase() }}
          </span>
        </v-avatar>
        <div>
          <div class="sidebar-user-name">{{ auth.user?.nickname }}</div>
          <div class="sidebar-user-role">{{ auth.user?.rol }}</div>
        </div>
      </div>

      <nav class="sidebar-nav" @click="sidebarOpen = false">
        <div class="nav-section">Principal</div>

        <NuxtLink to="/dashboard" class="nav-item" :class="{ active: $route.path === '/dashboard' }">
          <v-icon size="18">mdi-view-dashboard</v-icon>
          Dashboard
        </NuxtLink>

        <NuxtLink to="/dashboard/analisis" class="nav-item"
                  :class="{ active: $route.path === '/dashboard/analisis' }">
          <v-icon size="18">mdi-text-search</v-icon>
          Análisis Léxico
        </NuxtLink>

        <NuxtLink to="/dashboard/historial" class="nav-item"
                  :class="{ active: $route.path === '/dashboard/historial' }">
          <v-icon size="18">mdi-history</v-icon>
          Historial
        </NuxtLink>

        <template v-if="auth.isSupervisor">
          <div class="nav-section">Reportes</div>
          <NuxtLink to="/dashboard/reportes" class="nav-item"
                    :class="{ active: $route.path === '/dashboard/reportes' }">
            <v-icon size="18">mdi-chart-bar</v-icon>
            Estadísticas
          </NuxtLink>
        </template>

        <template v-if="auth.isAdmin">
          <div class="nav-section">Administración</div>
          <NuxtLink to="/dashboard/usuarios" class="nav-item"
                    :class="{ active: $route.path === '/dashboard/usuarios' }">
            <v-icon size="18">mdi-account-group</v-icon>
            Usuarios
          </NuxtLink>
          <NuxtLink to="/dashboard/bitacora" class="nav-item"
                    :class="{ active: $route.path === '/dashboard/bitacora' }">
            <v-icon size="18">mdi-shield-check</v-icon>
            Bitácora
          </NuxtLink>
        </template>

        <div class="nav-section">Cuenta</div>
        <NuxtLink to="/dashboard/perfil" class="nav-item"
                  :class="{ active: $route.path === '/dashboard/perfil' }">
          <v-icon size="18">mdi-account-edit</v-icon>
          Mi Perfil
        </NuxtLink>
      </nav>

      <!-- Cerrar sesión -->
      <div style="padding:16px;border-top:1px solid rgba(255,255,255,.12)">
        <v-btn block variant="outlined" color="white" size="small"
               prepend-icon="mdi-logout" @click="cerrarSesion">
          Cerrar sesión
        </v-btn>
      </div>
    </aside>

    <!-- Contenido principal -->
    <main :class="['page-content', { 'sidebar-collapsed': sidebarCollapsed }]">
      <!-- Control de barra lateral -->
      <div class="dashboard-topbar d-flex align-center mb-4">
        <button
          type="button"
          class="mobile-hamburger"
          :aria-expanded="sidebarOpen || !sidebarCollapsed"
          aria-label="Expandir o contraer menú de navegación"
          @click="alternarSidebar"
        >
          <span />
          <span />
          <span />
        </button>
        <span class="ml-2 font-weight-bold">Universidad Mariano Gálvez</span>
      </div>

      <slot />
    </main>
  </div>
</template>

<script setup lang="ts">
const auth  = useAuthStore()
const route = useRoute()

const sidebarOpen = ref(false)
const sidebarCollapsed = ref(false)

onMounted(() => auth.restore())

function alternarSidebar() {
  if (window.innerWidth <= 768) {
    sidebarOpen.value = !sidebarOpen.value
    return
  }

  sidebarCollapsed.value = !sidebarCollapsed.value
}

async function cerrarSesion() {
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

.mobile-hamburger[aria-expanded='true'] span:nth-child(1) { transform: translateY(7px) rotate(45deg); }
.mobile-hamburger[aria-expanded='true'] span:nth-child(2) { opacity: 0; }
.mobile-hamburger[aria-expanded='true'] span:nth-child(3) { transform: translateY(-7px) rotate(-45deg); }

.dashboard-topbar { min-height: 48px; }

.items-center { align-items: center; }
.gap-3        { gap: 12px; }
.px-4         { padding-left: 16px; padding-right: 16px; }
.py-3         { padding-top: 12px; padding-bottom: 12px; }
.flex         { display: flex; }
.text-white   { color: white; }
.text-sm      { font-size: 13px; }
.font-bold    { font-weight: 700; }
</style>
