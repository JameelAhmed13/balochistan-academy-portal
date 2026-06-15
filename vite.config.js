import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  // Local Ollama server (llama3) for AI test generation. Proxied so the
  // browser calls same-origin "/ollama/*" and avoids CORS issues.
  const ollamaTarget = env.VITE_OLLAMA_URL || 'http://localhost:11434'
  // .NET 10 API (server-dotnet) — override with VITE_API_TARGET env var if needed
  const apiTarget = env.VITE_API_TARGET || 'http://localhost:5000'

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
          target: apiTarget,
          changeOrigin: true,
        },
        '/hubs': {
          target: apiTarget,
          changeOrigin: true,
          ws: true,   // WebSocket passthrough for SignalR
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
