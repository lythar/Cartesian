import { defineConfig } from "vitepress";

// refer https://vitepress.dev/reference/site-config for details
export default defineConfig({
  lang: "pl",
  title: "Lythar",
  description:
    'Dokumentacja techniczna z projektu na hackathon hackheroes.pl',

  vite: {
    assetsInclude: ['**/*.png', '**/*.jpg', '**/*.jpeg', '**/*.gif', '**/*.svg'],
  },

  themeConfig: {
    logo: "./logo.svg",
    nav: [{ text: "Dokumentacja techniczna", link: "/main" }],

    sidebar: [
      {
        text: "Ogólne",
        items: [
          { text: "Informacje", link: "/main" },
        ],
      },
      {
        text: "Projekt",
        items: [
          {
            text: "Opis rozwiązania",
            link: "/opis-rozwiazania",
          },
          {
            text: "Struktura Systemu",
            link: "/struktura-systemu",
          },
          {
            text: "Interfejs Użytkownika",
            link: "/interfejs",
          }
        ],
      },
    ],
  },
});