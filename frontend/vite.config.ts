import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
  ],
  server: {
    port: 5173,
    proxy: {
      '/auth': {
        target: 'http://localhost:5194',
        changeOrigin: true,
        secure: false,
      },
      '/templates': {
        target: 'http://localhost:5194',
        changeOrigin: true,
        secure: false,
      },
      '/tierlists': {
        target: 'http://localhost:5194',
        changeOrigin: true,
        secure: false,
      },
      '/comments': {
        target: 'http://localhost:5194',
        changeOrigin: true,
        secure: false,
      },
      '/admin': {
        target: 'http://localhost:5194',
        changeOrigin: true,
        secure: false,
      },
      '/images': {
        target: 'http://localhost:5194',
        changeOrigin: true,
        secure: false,
      },
      '/uploads': {
        target: 'http://localhost:5194',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
