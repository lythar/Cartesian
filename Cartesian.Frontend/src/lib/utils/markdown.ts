import DOMPurify from "dompurify";
import { Marked } from "marked";
import twemoji from "@discordapp/twemoji";

const marked = new Marked({
	gfm: true,
	breaks: true,
});

const EVENT_LINK_REGEX =
	/(?:https?:\/\/[^\s]*\/app\?event=([a-f0-9-]+)|\/app\?event=([a-f0-9-]+))/gi;

const emojiCache = new Map<string, string>();

export interface ParsedContent {
	html: string;
	eventLinks: string[];
}

export function parseMessageContent(content: string): ParsedContent {
	const eventLinks: string[] = [];

	let match;
	const regex = new RegExp(EVENT_LINK_REGEX);
	while ((match = regex.exec(content)) !== null) {
		const eventId = match[1] || match[2];
		if (eventId && !eventLinks.includes(eventId)) {
			eventLinks.push(eventId);
		}
	}

	let html = marked.parse(content, { async: false }) as string;

	html = twemoji.parse(html, {
		folder: "svg",
		ext: ".svg",
		base: "https://cdn.jsdelivr.net/gh/twitter/twemoji@latest/assets/",
	});

	html = DOMPurify.sanitize(html, {
		ALLOWED_TAGS: [
			"p",
			"br",
			"strong",
			"em",
			"u",
			"s",
			"code",
			"pre",
			"blockquote",
			"ul",
			"ol",
			"li",
			"a",
			"img",
			"span",
		],
		ALLOWED_ATTR: ["href", "target", "rel", "class", "src", "alt", "draggable", "style"],
		ADD_ATTR: ["target"],
	});

	html = html.replace(/<a /g, '<a target="_blank" rel="noopener noreferrer" ');

	return { html, eventLinks };
}

export function parseEmoji(text: string): string {
	return twemoji.parse(text, {
		folder: "svg",
		ext: ".svg",
		base: "https://cdn.jsdelivr.net/gh/twitter/twemoji@latest/assets/",
		className: "twemoji",
	});
}

export function getCachedEmoji(emoji: string): string {
	if (!emojiCache.has(emoji)) {
		emojiCache.set(emoji, parseEmoji(emoji));
	}
	return emojiCache.get(emoji)!;
}
