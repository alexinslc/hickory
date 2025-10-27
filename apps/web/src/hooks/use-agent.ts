'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { ticketKeys } from './use-tickets';

// Query keys for agent-specific queries
export const agentKeys = {
  all: ['agent'] as const,
  queue: () => [...agentKeys.all, 'queue'] as const,
};

// Get agent queue
export function useAgentQueue() {
  return useQuery({
    queryKey: agentKeys.queue(),
    queryFn: () => apiClient.getAgentQueue(),
    staleTime: 10000, // 10 seconds - more frequent updates for agent queue
    refetchInterval: 30000, // Auto-refresh every 30 seconds
  });
}

// Assign ticket mutation
export function useAssignTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, agentId }: { ticketId: string; agentId: string }) =>
      apiClient.assignTicket(ticketId, { agentId }),
    onSuccess: (_, variables) => {
      // Invalidate agent queue and ticket details
      queryClient.invalidateQueries({ queryKey: agentKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
  });
}

// Update ticket status mutation
export function useUpdateTicketStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, newStatus }: { ticketId: string; newStatus: string }) =>
      apiClient.updateTicketStatus(ticketId, { newStatus }),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: agentKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
  });
}

// Update ticket priority mutation
export function useUpdateTicketPriority() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, newPriority }: { ticketId: string; newPriority: string }) =>
      apiClient.updateTicketPriority(ticketId, { newPriority }),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: agentKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
  });
}

// Close ticket mutation
export function useCloseTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, resolutionNotes }: { ticketId: string; resolutionNotes: string }) =>
      apiClient.closeTicket(ticketId, { resolutionNotes }),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: agentKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
  });
}

// Reassign ticket mutation
export function useReassignTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, newAgentId }: { ticketId: string; newAgentId: string }) =>
      apiClient.reassignTicket(ticketId, { newAgentId }),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: agentKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
  });
}
