import type {
	CartesianUserDto,
	CommunityDto,
	GetCommunityApiCommunityIdMembersParams,
	GetCommunityApiMeMembershipsParams,
	GetCommunityApiPublicListParams,
	MembershipDto,
} from "$lib/api/cartesian-client";
import {
	deleteCommunityApiCommunityId,
	deleteCommunityApiCommunityIdMembersTargetUserId,
	getCommunityApiCommunityIdMembers,
	getCommunityApiMeMemberships,
	getCommunityApiPublicCommunityId,
	getCommunityApiPublicList,
	getGetCommunityApiCommunityIdMembersQueryKey,
	getGetCommunityApiMeMembershipsQueryKey,
	getGetCommunityApiPublicCommunityIdQueryKey,
	getGetCommunityApiPublicListQueryKey,
	postCommunityApiCommunityIdMembersJoin,
	putCommunityApiCommunityIdMembersTargetUserIdPermissions,
} from "$lib/api/cartesian-client";
import type { CreateQueryOptions, CreateQueryResult, QueryClient } from "@tanstack/svelte-query";
import { createMutation, createQuery } from "@tanstack/svelte-query";

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

export function createGetCommunityQuery<TData = CommunityDto, TError = Error>(
	communityId: () => string,
	options?: { query?: Partial<CreateQueryOptions<CommunityDto, TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	return createQuery(() => {
		const id = communityId();
		return {
			queryKey: getGetCommunityApiPublicCommunityIdQueryKey(id),
			queryFn: ({ signal }) => getCommunityApiPublicCommunityId(id, signal),
			enabled: !!id,
			...options?.query,
			queryClient,
		};
	}) as CreateQueryResult<TData, TError>;
}

export function createGetCommunityMembersQuery<TData = MembershipDto[], TError = Error>(
	communityId: () => string,
	params: () => GetCommunityApiCommunityIdMembersParams = () => ({}),
	options?: { query?: Partial<CreateQueryOptions<MembershipDto[], TError, TData>> },
	queryClient?: QueryClient,
): CreateQueryResult<TData, TError> {
	return createQuery(() => {
		const id = communityId();
		const p = params();
		return {
			queryKey: getGetCommunityApiCommunityIdMembersQueryKey(id, p),
			queryFn: ({ signal }) => getCommunityApiCommunityIdMembers(id, p, signal),
			enabled: !!id,
			...options?.query,
			queryClient,
		};
	}) as CreateQueryResult<TData, TError>;
}

export function createJoinCommunityMutation(queryClient?: QueryClient) {
	return createMutation(() => ({
		mutationFn: ({ communityId }: { communityId: string }) =>
			postCommunityApiCommunityIdMembersJoin(communityId),
		onSuccess: (_, { communityId }) => {
			queryClient?.invalidateQueries({
				queryKey: getGetCommunityApiPublicCommunityIdQueryKey(communityId),
			});
			queryClient?.invalidateQueries({ queryKey: getGetCommunityApiMeMembershipsQueryKey() });
			queryClient?.invalidateQueries({
				queryKey: getGetCommunityApiCommunityIdMembersQueryKey(communityId),
			});
		},
	}));
}

export function createLeaveCommunityMutation(queryClient?: QueryClient) {
	return createMutation(() => ({
		mutationFn: ({ communityId, userId }: { communityId: string; userId: string }) =>
			deleteCommunityApiCommunityIdMembersTargetUserId(communityId, userId),
		onSuccess: (_, { communityId }) => {
			queryClient?.invalidateQueries({
				queryKey: getGetCommunityApiPublicCommunityIdQueryKey(communityId),
			});
			queryClient?.invalidateQueries({ queryKey: getGetCommunityApiMeMembershipsQueryKey() });
			queryClient?.invalidateQueries({
				queryKey: getGetCommunityApiCommunityIdMembersQueryKey(communityId),
			});
		},
	}));
}

export function createDeleteCommunityMutation(queryClient?: QueryClient) {
	return createMutation(() => ({
		mutationFn: ({ communityId }: { communityId: string }) =>
			deleteCommunityApiCommunityId(communityId),
		onSuccess: () => {
			queryClient?.invalidateQueries({ queryKey: getGetCommunityApiPublicListQueryKey() });
			queryClient?.invalidateQueries({ queryKey: getGetCommunityApiMeMembershipsQueryKey() });
		},
	}));
}

export function createRemoveMemberMutation(queryClient?: QueryClient) {
	return createMutation(() => ({
		mutationFn: ({ communityId, userId }: { communityId: string; userId: string }) =>
			deleteCommunityApiCommunityIdMembersTargetUserId(communityId, userId),
		onSuccess: (_, { communityId }) => {
			queryClient?.invalidateQueries({
				queryKey: getGetCommunityApiCommunityIdMembersQueryKey(communityId),
			});
		},
	}));
}

export function createUpdateMemberPermissionsMutation(queryClient?: QueryClient) {
	return createMutation(() => ({
		mutationFn: ({
			communityId,
			userId,
			permissions,
		}: {
			communityId: string;
			userId: string;
			permissions: number;
		}) =>
			putCommunityApiCommunityIdMembersTargetUserIdPermissions(communityId, userId, {
				permissions,
			}),
		onSuccess: (_, { communityId }) => {
			queryClient?.invalidateQueries({
				queryKey: getGetCommunityApiCommunityIdMembersQueryKey(communityId),
			});
		},
	}));
}
