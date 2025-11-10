import tailwindcss from "@tailwindcss/vite";
import { sveltekit } from "@sveltejs/kit/vite";
import { defineConfig } from "vite";

export default defineConfig({
	plugins: [tailwindcss(), sveltekit()],
	server: {
		// @ts-ignore: node
		port: Number(process.env.VITE_PORT ?? "5173"),
		host: "0.0.0.0",
	},
});
