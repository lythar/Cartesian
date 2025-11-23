import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";
import { baseUrl } from "./api/client";

export function cn(...inputs: ClassValue[]) {
	return twMerge(clsx(inputs));
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type WithoutChild<T> = T extends { child?: any } ? Omit<T, "child"> : T;
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type WithoutChildren<T> = T extends { children?: any } ? Omit<T, "children"> : T;
export type WithoutChildrenOrChild<T> = WithoutChildren<WithoutChild<T>>;
export type WithElementRef<T, U extends HTMLElement = HTMLElement> = T & { ref?: U | null };

export type LatLng = { lng: number; lat: number };

export function getInitials(name: string): string {
	return name
		.split(" ")
		.map((n) => n[0])
		.join("")
		.toUpperCase()
		.slice(0, 2);
}

export function getAvatarUrl(avatar: { id: string } | null): string | null {
	if (!avatar) return null;
	return `${baseUrl}/media/api/${avatar.id}`;
}

export function getMediaUrl(mediaId: string): string {
	return `${baseUrl}/media/api/${mediaId}`;
}
