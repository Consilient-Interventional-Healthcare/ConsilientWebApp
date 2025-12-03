import path from "path"
import tailwindcss from "@tailwindcss/vite"
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'fs'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  build: {
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: {
          // Separate React and React-related libraries
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          
          
          // Separate TanStack Query
          'query-vendor': ['@tanstack/react-query', '@tanstack/react-query-devtools'],
          
          // Separate UI libraries
          'ui-vendor': [
            '@radix-ui/react-slot',
            'class-variance-authority',
            'clsx',
            'tailwind-merge',
            'lucide-react'
          ],
          
          // Separate Axios and HTTP utilities
          'http-vendor': ['axios'],
          
          // Separate logging libraries
          'logging-vendor': ['loglevel', 'loglevel-plugin-remote'],
        },
      },
    },
    // Increase the warning limit if needed (default is 500 kB)
    chunkSizeWarningLimit: 600,
  },
  server: {
    port: 5173,
    https: {
      key: fs.readFileSync('localhost-key.pem'),
      cert: fs.readFileSync('localhost.pem'),
    },
  },
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
})