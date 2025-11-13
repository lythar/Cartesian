const stripAbsolutePath = (filenames) => {
  return filenames.map((f) => {
    const match = f.match(/Cartesian\.Frontend\/(.+)/);
    return match ? match[1] : f;
  });
};

export default {
  "Cartesian.Frontend/src/**/*.{ts,svelte}": (filenames) => {
    const files = stripAbsolutePath(filenames).join(" ");
    return [`pnpm --filter @repo/frontend exec prettier --write ${files}`];
  },
  "Cartesian.Frontend/**/*.{json,md}": (filenames) => {
    const files = stripAbsolutePath(filenames).join(" ");
    return [`pnpm --filter @repo/frontend exec prettier --write ${files}`];
  },
  "**/*.cs": ["dotnet format --verify-no-changes --include"],
};
