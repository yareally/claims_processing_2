import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    // Run every test, even after one fails.
    bail: 0,
  },
});
