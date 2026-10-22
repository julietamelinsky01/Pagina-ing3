import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import * as Sentry from '@sentry/react'
import './index.css'
import App from './App.jsx'

// Sentry: qué falla, qué tarda y qué se usa del lado del navegador.
// El DSN del front NO es un secreto: sólo sirve para ENVIAR datos, no para leerlos, y viaja igual
// en el JavaScript que baja cualquier navegador. Por eso va escrito acá (el del back va por entorno).
Sentry.init({
  dsn: 'https://3c248e09c339a1b7a341b4337e774125@o4512301597589504.ingest.us.sentry.io/4512301627670528',
  environment: window.location.hostname, // de qué sitio vino: QA, producción o localhost
  integrations: [Sentry.browserTracingIntegration()],
  tracesSampleRate: 1.0,
})

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
