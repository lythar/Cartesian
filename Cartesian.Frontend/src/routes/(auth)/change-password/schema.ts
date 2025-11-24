import { z } from "zod";

export const changePasswordSchema = z
	.object({
		oldPassword: z.string().min(1, "Old password is required"),
		newPassword: z.string().min(8, "Password must be at least 8 characters"),
		confirmPassword: z.string().min(1, "Please confirm your new password"),
	})
	.refine((data) => data.newPassword === data.confirmPassword, {
		message: "Passwords don't match",
		path: ["confirmPassword"],
	});

export type ChangePasswordSchema = typeof changePasswordSchema;
