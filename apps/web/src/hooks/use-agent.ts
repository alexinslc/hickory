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
    mutationFn: ({ ticketId, agentId, rowVersion }: { ticketId: string; agentId: string; rowVersion: string }) =>
      apiClient.assignTicket(ticketId, { agentId, rowVersion }),
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
    mutationFn: ({ ticketId, newStatus, rowVersion }: { ticketId: string; newStatus: string; rowVersion: string }) =>
      apiClient.updateTicketStatus(ticketId, { newStatus, rowVersion }),
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
    mutationFn: ({ ticketId, newPriority, rowVersion }: { ticketId: string; newPriority: string; rowVersion: string }) =>
      apiClient.updateTicketPriority(ticketId, { newPriority, rowVersion }),
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
    mutationFn: ({ ticketId, resolutionNotes, rowVersion }: { ticketId: string; resolutionNotes: string; rowVersion: string }) =>
      apiClient.closeTicket(ticketId, { resolutionNotes, rowVersion }),
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
    mutationFn: ({ ticketId, newAgentId, rowVersion }: { ticketId: string; newAgentId: string; rowVersion: string }) =>
      apiClient.reassignTicket(ticketId, { newAgentId, rowVersion }),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: agentKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
  });
}
