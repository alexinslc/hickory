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
