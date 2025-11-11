import { defineConfig } from "orval";

export default defineConfig({
	cartesian: {
		input: {
			target: "../Cartesian.Services/obj/Cartesian.Services.json",
		},
		output: {
			mode: "single",
			target: "./src/lib/api/cartesian-client.ts",
			client: "svelte-query",
			override: {
				mutator: {
					path: "./src/lib/api/client.ts",
					name: "customInstance",
				},
			},
		},
	},
});
