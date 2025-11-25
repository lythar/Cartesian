import { streamText } from "ai";
import { myProvider } from "$lib/server/ai/models";
import { descriptionEnhancePrompt } from "$lib/server/ai/prompts";
import type { RequestHandler } from "./$types";

export const POST: RequestHandler = async ({ request }) => {
	const { prompt } = await request.json();

	const systemPrompt = descriptionEnhancePrompt.replace("{{user_input}}", prompt);

	const result = streamText({
		model: myProvider.languageModel("description-enhance"),
		prompt: systemPrompt,
	});

	return result.toTextStreamResponse();
};
