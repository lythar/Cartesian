import { createGoogleGenerativeAI } from "@ai-sdk/google";
import { customProvider, extractReasoningMiddleware, wrapLanguageModel } from "ai";
import { env } from "$env/dynamic/private";

const google = createGoogleGenerativeAI({
	apiKey: env.GOOGLE_AI_KEY,
});

export const myProvider = customProvider({
	languageModels: {
		"description-enhance": google("gemini-2.5-flash"),
		"interactive-search": wrapLanguageModel({
			model: google("gemini-2.5-flash"),
			middleware: extractReasoningMiddleware({ tagName: "think" }),
		}),
	},
});
