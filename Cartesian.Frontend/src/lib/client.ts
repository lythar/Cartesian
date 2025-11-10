import type { paths } from "./generated/services";
import createClient from "openapi-fetch";

const baseUrl =
	import.meta.env.services_url ??
	import.meta.env.services__https__0 ??
	import.meta.env.services__http__0 ??
	"http://localhost:5164";

export const client = createClient<paths>({ baseUrl });
