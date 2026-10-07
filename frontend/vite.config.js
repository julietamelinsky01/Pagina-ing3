import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  test: {
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html', 'lcov', 'json-summary'],
      // Qué ENTRA en la cuenta: sólo la lógica pura (src/utils). Quedan afuera las páginas y
      // componentes (UI: se verifica end-to-end en el TP7), el cliente HTTP (src/api: adaptadores
      // finos sobre axios) y el arranque (main.jsx, App.jsx).
      include: ['src/utils/**'],
      exclude: ['src/**/*.test.js'],
      thresholds: { lines: 90, branches: 90 },
    },
  },
})
