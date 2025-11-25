import { streamText, convertToModelMessages, stepCountIs, type UIMessage } from "ai";
import { myProvider } from "$lib/server/ai/models";
import { interactiveSearchPrompt } from "$lib/server/ai/prompts";
import { searchTools } from "$lib/server/ai/tools";
import type { RequestHandler } from "./$types";

export const POST: RequestHandler = async ({ request }) => {
	const body = await request.json();
	const messages: UIMessage[] = body.messages || [];

	if (!messages.length) {
		return new Response(JSON.stringify({ error: "No messages provided" }), {
			status: 400,
			headers: { "Content-Type": "application/json" },
		});
	}

	const result = streamText({
		model: myProvider.languageModel("interactive-search"),
		system: interactiveSearchPrompt,
		messages: convertToModelMessages(messages),
		tools: searchTools,
		stopWhen: stepCountIs(5),
	});

	return result.toUIMessageStreamResponse();
};
