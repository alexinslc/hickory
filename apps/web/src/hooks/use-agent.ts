'use client';

import { useMutation, useQuery, useQueryClient, UseQueryResult } from '@tanstack/react-query';
import { apiClient, PaginatedResult, PaginationParams, TicketDto } from '@/lib/api-client';
import { ticketKeys } from './use-tickets';

// Query keys for agent-specific queries
export const agentKeys = {
  all: ['agent'] as const,
  queue: (params?: PaginationParams) => [...agentKeys.all, 'queue', params] as const,
};

// Get agent queue with pagination
export function useAgentQueue(params?: PaginationParams): UseQueryResult<PaginatedResult<TicketDto>, Error> {
  return useQuery({
    queryKey: agentKeys.queue(params),
    queryFn: () => apiClient.getAgentQueue(params),
    staleTime: 10000,
    refetchInterval: 30000,
  });
}

// Assign ticket mutation with optimistic update
export function useAssignTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, agentId, rowVersion }: { ticketId: string; agentId: string; rowVersion: string }) =>
      apiClient.assignTicket(ticketId, { agentId, rowVersion }),
    onMutate: async (variables) => {
      await queryClient.cancelQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      const previous = queryClient.getQueryData<TicketDto>(ticketKeys.detail(variables.ticketId));
      if (previous) {
        queryClient.setQueryData<TicketDto>(ticketKeys.detail(variables.ticketId), {
          ...previous,
          assignedToId: variables.agentId,
        });
      }
      return { previous };
    },
    onError: (_err, variables, context) => {
      if (context?.previous) {
        queryClient.setQueryData(ticketKeys.detail(variables.ticketId), context.previous);
      }
    },
    onSettled: (_, __, variables) => {
      queryClient.invalidateQueries({ queryKey: agentKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
  });
}

// Update ticket status mutation with optimistic update
export function useUpdateTicketStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, newStatus, rowVersion }: { ticketId: string; newStatus: string; rowVersion: string }) =>
      apiClient.updateTicketStatus(ticketId, { newStatus, rowVersion }),
    onMutate: async (variables) => {
      await queryClient.cancelQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      const previous = queryClient.getQueryData<TicketDto>(ticketKeys.detail(variables.ticketId));
      if (previous) {
        queryClient.setQueryData<TicketDto>(ticketKeys.detail(variables.ticketId), {
          ...previous,
          status: variables.newStatus,
        });
      }
      return { previous };
    },
    onError: (_err, variables, context) => {
      if (context?.previous) {
        queryClient.setQueryData(ticketKeys.detail(variables.ticketId), context.previous);
      }
    },
    onSettled: (_, __, variables) => {
      queryClient.invalidateQueries({ queryKey: agentKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
  });
}

// Update ticket priority mutation with optimistic update
export function useUpdateTicketPriority() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, newPriority, rowVersion }: { ticketId: string; newPriority: string; rowVersion: string }) =>
      apiClient.updateTicketPriority(ticketId, { newPriority, rowVersion }),
    onMutate: async (variables) => {
      await queryClient.cancelQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      const previous = queryClient.getQueryData<TicketDto>(ticketKeys.detail(variables.ticketId));
      if (previous) {
        queryClient.setQueryData<TicketDto>(ticketKeys.detail(variables.ticketId), {
          ...previous,
          priority: variables.newPriority,
        });
      }
      return { previous };
    },
    onError: (_err, variables, context) => {
      if (context?.previous) {
        queryClient.setQueryData(ticketKeys.detail(variables.ticketId), context.previous);
      }
    },
    onSettled: (_, __, variables) => {
      queryClient.invalidateQueries({ queryKey: agentKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
  });
}

// Close ticket mutation with optimistic update
export function useCloseTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, resolutionNotes, rowVersion }: { ticketId: string; resolutionNotes: string; rowVersion: string }) =>
      apiClient.closeTicket(ticketId, { resolutionNotes, rowVersion }),
    onMutate: async (variables) => {
      await queryClient.cancelQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      const previous = queryClient.getQueryData<TicketDto>(ticketKeys.detail(variables.ticketId));
      if (previous) {
        queryClient.setQueryData<TicketDto>(ticketKeys.detail(variables.ticketId), {
          ...previous,
          status: 'Closed',
          resolutionNotes: variables.resolutionNotes,
          closedAt: new Date().toISOString(),
        });
      }
      return { previous };
    },
    onError: (_err, variables, context) => {
      if (context?.previous) {
        queryClient.setQueryData(ticketKeys.detail(variables.ticketId), context.previous);
      }
    },
    onSettled: (_, __, variables) => {
      queryClient.invalidateQueries({ queryKey: agentKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
  });
}

// Reassign ticket mutation with optimistic update
export function useReassignTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, newAgentId, rowVersion }: { ticketId: string; newAgentId: string; rowVersion: string }) =>
      apiClient.reassignTicket(ticketId, { newAgentId, rowVersion }),
    onMutate: async (variables) => {
      await queryClient.cancelQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      const previous = queryClient.getQueryData<TicketDto>(ticketKeys.detail(variables.ticketId));
      if (previous) {
        queryClient.setQueryData<TicketDto>(ticketKeys.detail(variables.ticketId), {
          ...previous,
          assignedToId: variables.newAgentId,
        });
      }
      return { previous };
    },
    onError: (_err, variables, context) => {
      if (context?.previous) {
        queryClient.setQueryData(ticketKeys.detail(variables.ticketId), context.previous);
      }
    },
    onSettled: (_, __, variables) => {
      queryClient.invalidateQueries({ queryKey: agentKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
  });
}
