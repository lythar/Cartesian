/** @type {import("prettier-plugin-tailwindcss").PluginOptions} */
const prettierPluginTailwindCssConfig = {
	tailwindStylesheet: "./src/app.css",
	tailwindFunctions: ["clsx"],
};

/**
 * @see https://prettier.io/docs/configuration
 * @type {import("prettier").Config}
 */
const prettierConfig = {
	plugins: [
		"@prettier/plugin-oxc",
		"prettier-plugin-svelte",
		"prettier-plugin-organize-attributes",
		"prettier-plugin-organize-imports",
		"prettier-plugin-tailwindcss",
	],
	useTabs: true,
	tabWidth: 4,
	printWidth: 100,
	semi: true,
	singleQuote: false,
	overrides: [
		{
			files: "*.{yml,yaml}",
			options: {
				useTabs: false,
				tabWidth: 2,
				singleQuote: true,
			},
		},
		{
			files: "*.svelte",
			options: {
				parser: "svelte",
			},
		},
	],
};

/** @type {import("prettier").Config} */
const config = {
	...prettierConfig,
	...prettierPluginTailwindCssConfig,
};

export default config;
