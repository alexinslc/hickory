'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient, AddCommentRequest } from '@/lib/api-client';
import { ticketKeys } from './use-tickets';

// Comment query keys
export const commentKeys = {
  all: ['comments'] as const,
  byTicket: (ticketId: string) => [...commentKeys.all, ticketId] as const,
};

// Get comments for a ticket
export function useGetComments(ticketId: string) {
  return useQuery({
    queryKey: commentKeys.byTicket(ticketId),
    queryFn: () => apiClient.getComments(ticketId),
    enabled: !!ticketId,
  });
}

// Add comment mutation
export function useAddComment(ticketId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: AddCommentRequest) => apiClient.addComment(ticketId, request),
    onSuccess: () => {
      // Invalidate comments to refresh the list
      queryClient.invalidateQueries({ queryKey: commentKeys.byTicket(ticketId) });
      // Also invalidate the ticket detail to update comment count
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(ticketId) });
    },
  });
}
