export default {
	"Cartesian.Frontend/src/**/*.{ts,svelte}": [
		"pnpm --filter cartesian.frontend run lint:fix",
	],
	"Cartesian.Frontend/**/*.{json,md}": [
		"pnpm --filter cartesian.frontend run format",
	],
	"**/*.cs": ["dotnet format --verify-no-changes --include"],
};
