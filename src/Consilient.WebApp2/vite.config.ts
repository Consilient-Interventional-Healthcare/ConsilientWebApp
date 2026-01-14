import path from "path"
import tailwindcss from "@tailwindcss/vite"
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'fs'

// Only use HTTPS config in development and if PEM files exist
let httpsConfig: undefined | { key: Buffer; cert: Buffer } = undefined;
if (
  process.env.NODE_ENV === 'development' &&
  fs.existsSync('.local/localhost-key.pem') &&
  fs.existsSync('.local/localhost.pem')
) {
  httpsConfig = {
    key: fs.readFileSync('.local/localhost-key.pem'),
    cert: fs.readFileSync('.local/localhost.pem'),
  };
}

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
          // Separate UI libraries (small)
          'ui-vendor': [
            '@radix-ui/react-slot',
            '@radix-ui/react-toggle',
            '@radix-ui/react-toggle-group',
            'class-variance-authority',
            'clsx',
            'tailwind-merge',
            'lucide-react'
          ],
          // Separate Axios and HTTP utilities
          'http-vendor': ['axios'],
          // Separate logging libraries
          'logging-vendor': ['loglevel', 'loglevel-plugin-remote'],
          // Large UI component library
          'rsuite-vendor': ['rsuite'],
          // Charting library
          'charts-vendor': ['recharts'],
          // Data processing libraries (Excel, SQL)
          'data-vendor': ['xlsx', 'alasql'],
          // Icon libraries
          'icons-vendor': [
            '@fortawesome/fontawesome-svg-core',
            '@fortawesome/free-solid-svg-icons',
            '@fortawesome/react-fontawesome'
          ],
          // Utility libraries
          'utils-vendor': ['date-fns', 'zod'],
        },
      },
    },
    chunkSizeWarningLimit: 600,
  },
  server: {
    port: 5173,
    host: true,
    https: httpsConfig,
    watch: {
      usePolling: true,
    },
  },
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
})