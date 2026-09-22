<template>
  <main class="landing-page">
    <header class="landing-nav">
      <NuxtLink to="/" class="landing-brand" aria-label="Inicio">
        <img src="/images/logo-umg-oficial.png" alt="Universidad Mariano Gálvez de Guatemala">
        <span><strong>Universidad Mariano Gálvez</strong><small>Analizador léxico</small></span>
      </NuxtLink>

      <nav class="landing-links" aria-label="Navegación principal">
        <a href="#proceso">Proceso</a><a href="#capacidades">Capacidades</a><a href="#acceso">Acceso</a>
      </nav>
      <div class="landing-nav__actions">
        <v-btn to="/registro" class="landing-register" variant="text">Crear cuenta</v-btn>
        <v-btn to="/login" class="landing-login" variant="outlined" prepend-icon="mdi-login">Ingresar</v-btn>
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
          <div><strong>5</strong><span>idiomas disponibles</span></div>
          <div><strong>.txt</strong><span>formato de análisis</span></div>
          <div><strong>100%</strong><span>historial consultable</span></div>
        </div>
      </div>
    </section>

    <section id="proceso" class="landing-section landing-process">
      <div class="landing-section__intro landing-reveal"><p class="landing-section__label">01 · Proceso</p><h2>Una ruta clara, desde el archivo hasta el resultado.</h2></div>
      <div class="landing-steps">
        <article class="landing-reveal"><v-icon>mdi-file-upload-outline</v-icon><span>01</span><h3>Carga</h3><p>Selecciona tu documento de texto y el idioma que deseas procesar.</p></article>
        <article class="landing-reveal"><v-icon>mdi-text-search</v-icon><span>02</span><h3>Analiza</h3><p>El sistema clasifica palabras, pronombres, verbos y patrones del contenido.</p></article>
        <article class="landing-reveal"><v-icon>mdi-chart-box-outline</v-icon><span>03</span><h3>Consulta</h3><p>Revisa y conserva tus hallazgos desde tu historial personal.</p></article>
      </div>
    </section>

    <section id="capacidades" class="landing-section landing-capabilities">
      <div class="landing-capabilities__panel landing-reveal">
        <p class="landing-section__label">02 · Capacidades</p><h2>Lecturas que dejan evidencia.</h2>
        <p>Una vista ordenada para identificar la estructura del texto, no solo contar palabras.</p>
        <ul><li><v-icon>mdi-check-circle-outline</v-icon> Frecuencias y palabras poco comunes</li><li><v-icon>mdi-check-circle-outline</v-icon> Sustantivos, verbos y pronombres</li><li><v-icon>mdi-check-circle-outline</v-icon> Patrones léxicos por idioma</li></ul>
      </div>
      <div class="landing-results landing-reveal" aria-label="Resumen de resultados">
        <div class="landing-result-card landing-result-card--large"><span>Resultado de análisis</span><strong>Palabras clave</strong><div class="landing-bars"><i /><i /><i /><i /></div></div>
        <div class="landing-result-card"><v-icon>mdi-translate</v-icon><strong>Idiomas</strong><small>ES · EN · RU · ZH · AR</small></div>
        <div class="landing-result-card"><v-icon>mdi-history</v-icon><strong>Historial</strong><small>Consulta tus resultados cuando los necesites.</small></div>
      </div>
    </section>

    <section id="acceso" class="landing-cta landing-reveal"><div><p class="landing-section__label">03 · Acceso</p><h2>Tu próxima lectura empieza aquí.</h2><p>Crea tu cuenta para acceder al panel académico o inicia sesión si ya cuentas con credenciales.</p></div><div class="landing-cta__actions"><v-btn to="/registro" class="umg-gold-button" size="large" prepend-icon="mdi-account-plus-outline">Crear cuenta</v-btn><v-btn to="/login" class="landing-login--inverse" variant="outlined" size="large" prepend-icon="mdi-login">Ingresar</v-btn></div></section>
    <footer class="landing-footer"><span>Universidad Mariano Gálvez de Guatemala</span><span>Analizador léxico · {{ new Date().getFullYear() }}</span></footer>
  </main>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
useHead({ title: 'Analizador Léxico | Universidad Mariano Gálvez' })

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
