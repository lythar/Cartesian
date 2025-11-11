const stripAbsolutePath = (filenames) => {
  return filenames.map((f) => {
    const match = f.match(/Cartesian\.Frontend\/(.+)/);
    return match ? match[1] : f;
  });
};

export default {
  "Cartesian.Frontend/src/**/*.{ts,svelte}": (filenames) => {
    const files = stripAbsolutePath(filenames).join(" ");
    return [
      `sh -c "cd Cartesian.Frontend && pnpm exec prettier --write ${files}"`,
    ];
  },
  "Cartesian.Frontend/**/*.{json,md}": (filenames) => {
    const files = stripAbsolutePath(filenames).join(" ");
    return [
      `sh -c "cd Cartesian.Frontend && pnpm exec prettier --write ${files}"`,
    ];
  },
  "**/*.cs": ["dotnet format --verify-no-changes --include"],
};
