import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { execFileSync } from "node:child_process";
import { readdirSync } from "node:fs";
import { resolve } from "node:path";
import { fileURLToPath } from "node:url";

const repositoryRoot = fileURLToPath(new URL("..", import.meta.url));

function gitOutput(...arguments_: string[]) {
  try {
    return execFileSync("git", arguments_, { cwd: repositoryRoot, encoding: "utf8" }).trim();
  } catch {
    return "";
  }
}

const branches = gitOutput("branch", "--format=%(refname:short)").split("\n").filter(Boolean);
const currentBranch = gitOutput("branch", "--show-current");
const projectNames = readdirSync(resolve(repositoryRoot, "projects"), { withFileTypes: true })
  .filter((entry) => entry.isDirectory() && entry.name !== "_template" && !entry.name.startsWith("."))
  .map((entry) => entry.name);
const activeProject = projectNames.includes(currentBranch) ? currentBranch : null;

export default defineConfig({
  plugins: [react()],
  define: {
    __GIT_BRANCHES__: JSON.stringify(branches),
    __CURRENT_BRANCH__: JSON.stringify(currentBranch),
    __ACTIVE_PROJECT__: JSON.stringify(activeProject),
    __PROJECT_URL__: JSON.stringify(process.env.PROJECT_URL ?? "http://localhost:5173"),
  },
  server: {
    port: Number(process.env.LANDING_PORT ?? 5173),
    strictPort: true,
    proxy: {
      "/api": process.env.VITE_API_PROXY_TARGET ?? "http://localhost:5080",
    },
  },
});
