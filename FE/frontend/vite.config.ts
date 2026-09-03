import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';

export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@':           path.resolve(__dirname, './src'),
      '@api':        path.resolve(__dirname, './src/api'),
      '@components': path.resolve(__dirname, './src/components'),
      '@pages':      path.resolve(__dirname, './src/pages'),
      '@hooks':      path.resolve(__dirname, './src/hooks'),
      '@types':      path.resolve(__dirname, './src/types'),
      '@utils':      path.resolve(__dirname, './src/utils'),
      '@schemas':    path.resolve(__dirname, './src/schemas'),
    },
  },
  server: {
    port:       5173,
    strictPort: true,
    proxy: {
      '/api': {
        target:       'http://localhost:5001',
        changeOrigin: true,
        secure:       false,
      },
    },
  },
});
