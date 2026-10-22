<template>
  <main class="landing-page">
    <header class="landing-nav">
      <NuxtLink to="/" class="landing-brand" aria-label="Inicio">
        <img src="/images/logo-umg-oficial.png" alt="Universidad Mariano Gálvez de Guatemala">
        <span><strong>Universidad Mariano Gálvez</strong><small>Analizador léxico</small></span>
      </NuxtLink>

      <nav class="landing-links" aria-label="Navegación principal">
        <a href="#proceso"><v-icon size="16">mdi-source-branch</v-icon><span>Cómo funciona</span></a>
        <a href="#capacidades"><v-icon size="16">mdi-chart-box-outline</v-icon><span>Herramientas</span></a>
      </nav>
      <div class="landing-nav__actions">
        <v-btn to="/registro" class="landing-register" variant="text" prepend-icon="mdi-account-plus-outline" aria-label="Crear cuenta" title="Crear cuenta">Crear cuenta</v-btn>
        <v-btn to="/login" class="landing-login" variant="outlined" prepend-icon="mdi-login" aria-label="Ingresar" title="Ingresar">Ingresar</v-btn>

        <v-menu location="bottom end" transition="slide-y-transition">
          <template #activator="{ props }">
            <v-btn
              v-bind="props"
              class="landing-menu-trigger"
              icon="mdi-menu"
              variant="text"
              aria-label="Abrir menú de navegación"
            />
          </template>
          <v-list class="landing-mobile-menu" density="comfortable" min-width="230" nav>
            <v-list-item href="#proceso" prepend-icon="mdi-source-branch" title="Cómo funciona" />
            <v-list-item href="#capacidades" prepend-icon="mdi-chart-box-outline" title="Herramientas" />
            <v-divider class="my-1" />
            <v-list-item to="/registro" prepend-icon="mdi-account-plus-outline" title="Crear cuenta" />
            <v-list-item to="/login" prepend-icon="mdi-login" title="Ingresar" />
          </v-list>
        </v-menu>
      </div>
    </header>

    <section class="landing-hero" aria-labelledby="landing-title">
      <div class="landing-hero__media" aria-hidden="true">
        <span class="landing-hero__image landing-hero__image--one" />
        <span class="landing-hero__image landing-hero__image--two" />
        <span class="landing-hero__image landing-hero__image--three" />
      </div>
      <div class="landing-hero__overlay" />
      <div class="landing-hero__content landing-reveal">
        <p class="landing-eyebrow"><span /> Plataforma académica</p>
        <h1 id="landing-title">Convierte texto en<br><span>conocimiento útil.</span></h1>
        <p class="landing-lead">Analiza documentos <strong>.txt</strong>, identifica componentes léxicos y conserva resultados trazables para tu proceso académico.</p>
        <div class="landing-actions">
          <v-btn to="/login" class="umg-gold-button landing-primary" size="large" prepend-icon="mdi-arrow-right">Iniciar análisis</v-btn>
          <v-btn to="/registro" class="landing-register-cta" variant="text" append-icon="mdi-arrow-right">Crear una cuenta</v-btn>
        </div>
        <div class="landing-stats" aria-label="Características principales">
          <div><v-icon>mdi-translate</v-icon><span>Análisis en varios idiomas</span></div>
          <div><v-icon>mdi-file-document-outline</v-icon><span>Documentos de texto</span></div>
          <div><v-icon>mdi-history</v-icon><span>Historial personal</span></div>
        </div>
      </div>
    </section>

    <section id="proceso" class="landing-section landing-process">
      <div class="landing-section__intro landing-reveal"><p class="landing-section__label">Flujo de análisis</p><h2>Una ruta clara, desde el archivo hasta el resultado.</h2></div>
      <div class="landing-steps">
        <article class="landing-reveal"><div class="landing-step__icon"><v-icon>mdi-file-upload-outline</v-icon></div><h3>Carga</h3><p>Selecciona tu documento de texto y el idioma que deseas procesar.</p></article>
        <article class="landing-reveal"><div class="landing-step__icon"><v-icon>mdi-text-search</v-icon></div><h3>Analiza</h3><p>El sistema clasifica palabras, pronombres, verbos y patrones del contenido.</p></article>
        <article class="landing-reveal"><div class="landing-step__icon"><v-icon>mdi-chart-box-outline</v-icon></div><h3>Consulta</h3><p>Revisa y conserva tus hallazgos desde tu historial personal.</p></article>
      </div>
    </section>

    <section id="capacidades" class="landing-section landing-capabilities">
      <div class="landing-capabilities__panel landing-reveal">
        <p class="landing-section__label">Herramientas académicas</p><h2>Lecturas que dejan evidencia.</h2>
        <p>Una vista ordenada para identificar la estructura del texto, no solo contar palabras.</p>
        <ul><li><v-icon>mdi-check-circle-outline</v-icon> Frecuencias y palabras poco comunes</li><li><v-icon>mdi-check-circle-outline</v-icon> Sustantivos, verbos y pronombres</li><li><v-icon>mdi-check-circle-outline</v-icon> Patrones léxicos por idioma</li></ul>
      </div>
      <div class="landing-results landing-reveal" aria-label="Resumen de resultados">
        <div class="landing-result-card landing-result-card--large">
          <span>Resultado de análisis</span>
          <strong>Resumen del documento</strong>
          <div class="landing-result-metrics" aria-label="Métricas del análisis">
            <div><b>703</b><small>Palabras</small></div>
            <div><b>42</b><small>Oraciones</small></div>
            <div><b>38</b><small>Párrafos</small></div>
            <div><b>4.99</b><small>Promedio</small></div>
          </div>
        </div>
        <div class="landing-result-card">
          <v-icon>mdi-format-list-bulleted-type</v-icon>
          <strong>Clasificación léxica</strong>
          <small>Consulta pronombres, verbos, sustantivos, adjetivos y conectores.</small>
        </div>
        <div class="landing-result-card">
          <v-icon>mdi-text-search</v-icon>
          <strong>Datos detectados</strong>
          <small>Identifica enlaces, correos, fechas y números dentro del documento.</small>
        </div>
      </div>
    </section>

    <section id="acceso" class="landing-cta landing-reveal"><div><p class="landing-section__label">Acceso institucional</p><h2>Tu próxima lectura empieza aquí.</h2><p>Crea tu cuenta para acceder al panel académico o inicia sesión si ya cuentas con credenciales.</p></div><div class="landing-cta__actions"><v-btn to="/registro" class="umg-gold-button" size="large" prepend-icon="mdi-account-plus-outline">Crear cuenta</v-btn><v-btn to="/login" class="landing-login--inverse" variant="outlined" size="large" prepend-icon="mdi-login">Ingresar</v-btn></div></section>
    <footer class="landing-footer"><span>Universidad Mariano Gálvez de Guatemala</span><span>Analizador léxico · {{ new Date().getFullYear() }}</span></footer>
  </main>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
useHead({
  title: 'Analizador Léxico | Universidad Mariano Gálvez',
  meta: [
    { name: 'google-site-verification', content: 'BbBOZkQW2lhz7F8f1x20Ac43gSxIjy2AzOpSjgJi4s8' }
  ]
})

let revealObserver: IntersectionObserver | undefined

onMounted(() => {
  const sections = document.querySelectorAll<HTMLElement>('.landing-reveal')
  revealObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add('is-visible')
        revealObserver?.unobserve(entry.target)
      }
    })
  }, { threshold: 0.14 })
  sections.forEach(section => revealObserver?.observe(section))
})

onBeforeUnmount(() => revealObserver?.disconnect())
</script>
