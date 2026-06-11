import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  // Local Ollama server (llama3) for AI test generation. Proxied so the
  // browser calls same-origin "/ollama/*" and avoids CORS issues.
  const ollamaTarget = env.VITE_OLLAMA_URL || 'http://localhost:11434'

  return {
    plugins: [vue()],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
      },
    },
    server: {
      port: 5173,
      proxy: {
        '/api': {
          target: 'http://localhost:3000',
          changeOrigin: true,
        },
        '/ollama': {
          target: ollamaTarget,
          changeOrigin: true,
          rewrite: (p) => p.replace(/^\/ollama/, ''),
        },
      },
    },
  }
})
