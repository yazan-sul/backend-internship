import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: Number(process.env.LANDING_PORT ?? 5173),
    strictPort: true,
    open: process.env.OPEN_BROWSER === "true",
    proxy: {
      "/api": process.env.VITE_API_PROXY_TARGET ?? "http://localhost:5080",
    },
  },
});
