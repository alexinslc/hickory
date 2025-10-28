import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../api-client';

// Query keys
export const ticketKeys = {
  all: ['tickets'] as const,
  lists: () => [...ticketKeys.all, 'list'] as const,
  list: () => [...ticketKeys.lists()] as const,
  details: () => [...ticketKeys.all, 'detail'] as const,
  detail: (id: string) => [...ticketKeys.details(), id] as const,
  queue: () => [...ticketKeys.all, 'queue'] as const,
};

// Add tags to ticket
export function useAddTagsToTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, tags }: { ticketId: string; tags: string[] }) =>
      apiClient.addTagsToTicket(ticketId, tags),
    onSuccess: (_, variables) => {
      // Invalidate specific ticket and all ticket lists
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.queue() });
    },
  });
}

// Remove tags from ticket
export function useRemoveTagsFromTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, tags }: { ticketId: string; tags: string[] }) =>
      apiClient.removeTagsFromTicket(ticketId, tags),
    onSuccess: (_, variables) => {
      // Invalidate specific ticket and all ticket lists
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.queue() });
    },
  });
}

// Get ticket by ID
export function useGetTicketById(id: string) {
  return useQuery({
    queryKey: ticketKeys.detail(id),
    queryFn: () => apiClient.getTicketById(id),
    enabled: !!id,
  });
}

// Get my tickets
export function useGetMyTickets() {
  return useQuery({
    queryKey: ticketKeys.list(),
    queryFn: () => apiClient.getMyTickets(),
  });
}

// Get agent queue
export function useGetAgentQueue() {
  return useQuery({
    queryKey: ticketKeys.queue(),
    queryFn: () => apiClient.getAgentQueue(),
  });
}

// Assign ticket to agent
export function useAssignTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, agentId, rowVersion }: { ticketId: string; agentId: string; rowVersion: string }) =>
      apiClient.assignTicket(ticketId, { agentId, rowVersion }),
    onSuccess: (_, variables) => {
      // Invalidate specific ticket to refetch with new assignment
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      
      // Invalidate queue and lists
      queryClient.invalidateQueries({ queryKey: ticketKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
    onError: (error) => {
      console.error('Failed to assign ticket:', error);
    },
  });
}

// Update ticket status
export function useUpdateTicketStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, newStatus, rowVersion }: { ticketId: string; newStatus: string; rowVersion: string }) =>
      apiClient.updateTicketStatus(ticketId, { newStatus, rowVersion }),
    onSuccess: (_, variables) => {
      // Invalidate specific ticket to refetch with new status
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      
      // Invalidate queue and lists (ticket may move between views)
      queryClient.invalidateQueries({ queryKey: ticketKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
    onError: (error) => {
      console.error('Failed to update ticket status:', error);
    },
  });
}

// Update ticket priority
export function useUpdateTicketPriority() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, newPriority, rowVersion }: { ticketId: string; newPriority: string; rowVersion: string }) =>
      apiClient.updateTicketPriority(ticketId, { newPriority, rowVersion }),
    onSuccess: (_, variables) => {
      // Invalidate specific ticket to refetch with new priority
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      
      // Invalidate queue and lists (sorting may change)
      queryClient.invalidateQueries({ queryKey: ticketKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
    onError: (error) => {
      console.error('Failed to update ticket priority:', error);
    },
  });
}

// Close ticket
export function useCloseTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, resolutionNotes, rowVersion }: { ticketId: string; resolutionNotes: string; rowVersion: string }) =>
      apiClient.closeTicket(ticketId, { resolutionNotes, rowVersion }),
    onSuccess: (_, variables) => {
      // Invalidate specific ticket to refetch with closed status
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      
      // Invalidate queue and lists (ticket should move out of active views)
      queryClient.invalidateQueries({ queryKey: ticketKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
    onError: (error) => {
      console.error('Failed to close ticket:', error);
    },
  });
}

// Reassign ticket to different agent
export function useReassignTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ticketId, newAgentId, rowVersion }: { ticketId: string; newAgentId: string; rowVersion: string }) =>
      apiClient.reassignTicket(ticketId, { newAgentId, rowVersion }),
    onSuccess: (_, variables) => {
      // Invalidate specific ticket to refetch with new assignment
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(variables.ticketId) });
      
      // Invalidate queue and lists
      queryClient.invalidateQueries({ queryKey: ticketKeys.queue() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
    },
    onError: (error) => {
      console.error('Failed to reassign ticket:', error);
    },
  });
}
