'use client';

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient, AddCommentRequest } from '@/lib/api-client';
import { ticketKeys } from './use-tickets';

// Add comment mutation
export function useAddComment(ticketId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: AddCommentRequest) => apiClient.addComment(ticketId, request),
    onSuccess: () => {
      // Invalidate the ticket detail to refresh comments
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(ticketId) });
    },
  });
}
