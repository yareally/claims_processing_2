import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    // Stop the run at the first failing test.
    bail: 1,
  },
});
