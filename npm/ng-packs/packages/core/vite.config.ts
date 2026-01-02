/// <reference types="vitest" />
import { defineConfig } from 'vitest/config';
import { nxViteTsPaths } from '@nx/vite/plugins/nx-tsconfig-paths.plugin';

export default defineConfig({
  plugins: [nxViteTsPaths()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test-setup.ts'],
    include: ['src/**/*.spec.ts'],
    coverage: {
      reportsDirectory: '../../coverage/packages/core',
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
    },
  },
});
