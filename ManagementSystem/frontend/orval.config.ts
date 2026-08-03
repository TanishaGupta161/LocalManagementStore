import { defineConfig } from "orval";

export default defineConfig({
  managementApi: {
    input: {
      target: "http://localhost:5115/swagger/v1/swagger.json",
    },
    output: {
      target: "./src/api/generated.ts",
      client: "axios",
      mode: "split",
      schemas: "./src/api/model",
    },
  },
});