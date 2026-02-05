/// <reference types="vitest" />
import { defineConfig } from 'vite';
import angular from '@analogjs/vite-plugin-angular';
import tsconfigPaths from 'vite-tsconfig-paths';

export default defineConfig(({ mode }) => ({
  plugins: [
    angular({
      tsconfig: './tsconfig.spec.json',
    }),
    tsconfigPaths(),
  ],
  test: {
    globals: true,
    setupFiles: ['vitest-setup.ts'],
    environment: 'jsdom',
    include: ['src/**/*.spec.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'dist/',
        '.angular/',
        '**/*.spec.ts',
        '**/*.config.ts',
        '**/index.ts',
        '**/index.ts.backup',
      ],
    },
    testTimeout: 10000,
    hookTimeout: 10000,
  },
  define: {
    'import.meta.vitest': mode !== 'production',
  },
}));
