FROM node:25-alpine AS build

RUN npm install -g pnpm

WORKDIR /app
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY .prettierignore svelte.config.js tsconfig.json vite.config.ts components.json orval.config.ts prettier.config.js ./
COPY messages/ ./messages/
COPY project.inlang/ ./project.inlang/
COPY src/ ./src/
COPY static/ ./static/
RUN --mount=type=cache,target=/pnpm/store pnpm install --frozen-lockfile
RUN pnpm run build

FROM node:25-alpine AS runtime

WORKDIR /app
COPY --from=build /app/build ./build
COPY --from=build /app/package.json ./package.json
COPY --from=build /app/node_modules ./node_modules

ENV NODE_ENV=production

USER node

EXPOSE 3000

ENTRYPOINT ["node","build"]
