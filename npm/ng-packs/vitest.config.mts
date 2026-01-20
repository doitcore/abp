import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    projects: [
      {
        root: './packages/core',
        test: {
          name: 'core',
          globals: true,
          environment: 'jsdom',
          setupFiles: ['src/test-setup.ts'],
          include: ['{src,tests}/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}'],
        },
      },
      {
        root: './packages/theme-basic',
        test: {
          name: 'theme-basic',
          globals: true,
          environment: 'jsdom',
          setupFiles: ['src/test-setup.ts'],
          include: ['{src,tests}/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}'],
        },
      },
      {
        root: './packages/theme-shared',
        test: {
          name: 'theme-shared',
          globals: true,
          environment: 'jsdom',
          setupFiles: ['src/test-setup.ts'],
          include: ['{src,tests}/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}'],
        },
      },
      {
        root: './packages/oauth',
        test: {
          name: 'oauth',
          globals: true,
          environment: 'jsdom',
          setupFiles: ['src/test-setup.ts'],
        },
      },
      {
        root: './packages/generators',
        test: {
          name: 'generators',
          globals: true,
          environment: 'jsdom',
          setupFiles: ['src/test-setup.ts'],
        },
      },
      {
        root: './packages/schematics',
        test: {
          name: 'schematics',
          globals: true,
          environment: 'jsdom',
          setupFiles: ['src/test-setup.ts'],
        },
      },
    ],
  },
});