import { fileURLToPath, URL } from 'node:url'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv, type Plugin } from 'vite'

const apiUrl = process.env.VITE_API_PROXY_TARGET ?? 'http://localhost:5228'

function hideFromSearchEngines(enabled: boolean): Plugin {
  return {
    name: 'hide-from-search-engines',
    transformIndexHtml: () =>
      enabled ? [{ tag: 'meta', attrs: { name: 'robots', content: 'noindex, nofollow' }, injectTo: 'head' }] : [],
  }
}

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), 'VITE_')

  return {
    plugins: [react(), tailwindcss(), hideFromSearchEngines(env.VITE_DEMO_MODE === 'true')],
    resolve: {
      alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
    },
    server: {
      port: 5173,
      proxy: {
        '/api': apiUrl,
        '/uploads': apiUrl,
      },
    },
  }
})
