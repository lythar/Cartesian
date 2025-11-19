import { z } from "zod";

export const schema = z
	.object({
		username: z.string().min(3).max(31),
		email: z.string().email(),
		password: z
			.string()
			.min(6, "Password must be at least 6 characters")
			.regex(/[A-Z]/, "Password must contain at least one uppercase letter")
			.regex(/[a-z]/, "Password must contain at least one lowercase letter")
			.regex(/[0-9]/, "Password must contain at least one digit")
			.regex(/[^A-Za-z0-9]/, "Password must contain at least one special character"),
		confirmPassword: z.string().min(6),
	})
	.refine((data) => data.password === data.confirmPassword, {
		message: "Passwords do not match",
		path: ["confirmPassword"],
	});

export type RegisterSchema = z.infer<typeof schema>;
