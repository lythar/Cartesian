import type {
	CommunityDto,
	GetCommunityApiMeMembershipsParams,
	GetCommunityApiPublicListParams,
	MembershipDto,
} from "$lib/api/cartesian-client";
import {
	getCommunityApiMeMemberships,
	getCommunityApiPublicList,
	getGetCommunityApiMeMembershipsQueryKey,
	getGetCommunityApiPublicListQueryKey,
} from "$lib/api/cartesian-client";
import type { CreateQueryOptions, CreateQueryResult, QueryClient } from "@tanstack/svelte-query";
import { createQuery } from "@tanstack/svelte-query";

export function createGetPublicCommunitiesQuery<TData = CommunityDto[], TError = Error>(
	params: () => GetCommunityApiPublicListParams = () => ({}),
	options?: { query?: Partial<CreateQueryOptions<CommunityDto[], TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	return createQuery(() => {
		const p = params();
		return {
			queryKey: getGetCommunityApiPublicListQueryKey(p),
			queryFn: ({ signal }) => getCommunityApiPublicList(p, signal),
			staleTime: 1000 * 60 * 5, // 5 minutes
			...options?.query,
			queryClient,
		};
	}) as CreateQueryResult<TData, TError>;
}

export function createGetMyMembershipsQuery<TData = MembershipDto[], TError = Error>(
	params: () => GetCommunityApiMeMembershipsParams = () => ({}),
	options?: { query?: Partial<CreateQueryOptions<MembershipDto[], TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	return createQuery(() => {
		const p = params();
		return {
			queryKey: getGetCommunityApiMeMembershipsQueryKey(p),
			queryFn: ({ signal }) => getCommunityApiMeMemberships(p, signal),
			staleTime: 1000 * 60 * 5, // 5 minutes
			...options?.query,
			queryClient,
		};
	}) as CreateQueryResult<TData, TError>;
}
