FROM node:25-alpine AS base
ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
RUN npm i -g --force corepack
RUN corepack enable

FROM base AS deps
WORKDIR /app
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
RUN --mount=type=cache,id=pnpm,target=/pnpm/store \
    pnpm fetch --frozen-lockfile

FROM base AS build
WORKDIR /app
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
RUN --mount=type=cache,id=pnpm,target=/pnpm/store \
    pnpm install --frozen-lockfile --prefer-offline

COPY .prettierignore svelte.config.js tsconfig.json vite.config.ts components.json orval.config.ts prettier.config.js ./
COPY messages/ ./messages/
COPY project.inlang/ ./project.inlang/
COPY src/ ./src/
COPY static/ ./static/

RUN pnpm run build

RUN --mount=type=cache,id=pnpm,target=/pnpm/store \
    pnpm install --prod --frozen-lockfile --offline --prefer-offline

FROM node:25-alpine AS runtime
WORKDIR /app

COPY --from=build /app/build ./build
COPY --from=build /app/package.json ./package.json
COPY --from=build /app/node_modules ./node_modules

ENV NODE_ENV=production

USER node

EXPOSE 3000

CMD ["node", "build"]
