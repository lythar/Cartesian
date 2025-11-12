import { paraglideVitePlugin } from "@inlang/paraglide-js";
import tailwindcss from "@tailwindcss/vite";
import { sveltekit } from "@sveltejs/kit/vite";
import { defineConfig } from "vite";

let publicIp = "127.0.0.1";

const getPublicIp = async () => {
	try {
		const response = await fetch("https://api.ipify.org?format=json");
		const data = (await response.json()) as { ip: string };
		return data.ip;
	} catch {
		return "127.0.0.1";
	}
};

export default defineConfig(async ({ command }) => {
	const define: Record<string, string> = {};

	if (command === "serve") {
		publicIp = await getPublicIp();
		define.__PUBLIC_IP__ = JSON.stringify(publicIp);
	}

	return {
		plugins: [
			paraglideVitePlugin({ project: "./project.inlang", outdir: "./src/paraglide" }),
			tailwindcss(),
			sveltekit(),
		],
		define,
	};
});
