import type { MyUserDto } from "$lib/api/cartesian-client";
import { getAccountApiMe, getGetAccountApiMeQueryKey } from "$lib/api/cartesian-client";
import type { CreateQueryOptions, CreateQueryResult, QueryClient } from "@tanstack/svelte-query";
import { createQuery } from "@tanstack/svelte-query";

export function createGetMeQuery<TData = MyUserDto, TError = Error>(
	options?: { query?: Partial<CreateQueryOptions<MyUserDto, TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	return createQuery(() => ({
		queryKey: getGetAccountApiMeQueryKey(),
		queryFn: ({ signal }) => getAccountApiMe(signal),
		staleTime: 1000 * 60 * 60, // 1 hour
		...options?.query,
		queryClient,
	})) as CreateQueryResult<TData, TError>;
}
