import { createQuery } from "@tanstack/svelte-query";
import type { CreateQueryOptions, CreateQueryResult, QueryClient } from "@tanstack/svelte-query";
import {
	getSearchApiAll,
	getSearchApiEvents,
	getSearchApiCommunities,
	getSearchApiUsers,
	getGetSearchApiAllQueryKey,
	getGetSearchApiEventsQueryKey,
	getGetSearchApiCommunitiesQueryKey,
	getGetSearchApiUsersQueryKey,
} from "$lib/api/cartesian-client";
import type {
	EventDto,
	CommunityDto,
	CartesianUserDto,
	MergedSearchResultsDto,
	GetSearchApiEventsParams,
	GetSearchApiCommunitiesParams,
	GetSearchApiUsersParams,
	GetSearchApiAllParams,
} from "$lib/api/cartesian-client";

export function createSearchEventsQuery<TData = EventDto[], TError = Error>(
	params: () => GetSearchApiEventsParams,
	options?: { query?: Partial<CreateQueryOptions<EventDto[], TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	return createQuery(() => {
		const p = params();
		return {
			queryKey: getGetSearchApiEventsQueryKey(p),
			queryFn: ({ signal }) => getSearchApiEvents(p, signal),
			staleTime: 1000 * 60,
			enabled: !!p.query && p.query.length >= 2,
			...options?.query,
			queryClient,
		};
	}) as CreateQueryResult<TData, TError>;
}

export function createSearchCommunitiesQuery<TData = CommunityDto[], TError = Error>(
	params: () => GetSearchApiCommunitiesParams,
	options?: { query?: Partial<CreateQueryOptions<CommunityDto[], TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	return createQuery(() => {
		const p = params();
		return {
			queryKey: getGetSearchApiCommunitiesQueryKey(p),
			queryFn: ({ signal }) => getSearchApiCommunities(p, signal),
			staleTime: 1000 * 60,
			enabled: !!p.query && p.query.length >= 2,
			...options?.query,
			queryClient,
		};
	}) as CreateQueryResult<TData, TError>;
}

export function createSearchUsersQuery<TData = CartesianUserDto[], TError = Error>(
	params: () => GetSearchApiUsersParams,
	options?: { query?: Partial<CreateQueryOptions<CartesianUserDto[], TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	return createQuery(() => {
		const p = params();
		return {
			queryKey: getGetSearchApiUsersQueryKey(p),
			queryFn: ({ signal }) => getSearchApiUsers(p, signal),
			staleTime: 1000 * 60,
			enabled: !!p.query && p.query.length >= 2,
			...options?.query,
			queryClient,
		};
	}) as CreateQueryResult<TData, TError>;
}

export function createSearchAllQuery<TData = MergedSearchResultsDto, TError = Error>(
	params: () => GetSearchApiAllParams,
	options?: { query?: Partial<CreateQueryOptions<MergedSearchResultsDto, TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	return createQuery(() => {
		const p = params();
		return {
			queryKey: getGetSearchApiAllQueryKey(p),
			queryFn: ({ signal }) => getSearchApiAll(p, signal),
			staleTime: 1000 * 60,
			enabled: !!p.query && p.query.length >= 2,
			...options?.query,
			queryClient,
		};
	}) as CreateQueryResult<TData, TError>;
}
