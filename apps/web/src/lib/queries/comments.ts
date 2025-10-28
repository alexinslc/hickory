import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient, AddCommentRequest } from '../api-client';
import { ticketKeys } from './tickets';

// Query keys
export const commentKeys = {
  all: ['comments'] as const,
  lists: () => [...commentKeys.all, 'list'] as const,
  list: (ticketId: string) => [...commentKeys.lists(), ticketId] as const,
  details: () => [...commentKeys.all, 'detail'] as const,
  detail: (id: string) => [...commentKeys.details(), id] as const,
};

// Add comment mutation
export function useAddComment(ticketId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: AddCommentRequest) => apiClient.addComment(ticketId, data),
    onSuccess: () => {
      // Invalidate ticket details to refetch with new comment count
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(ticketId) });
      
      // Invalidate ticket lists (comment count changed)
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.queue() });
      
      // If we had a comments list query, we'd update it here
      // For now, we rely on ticket refetch
    },
    onError: (error) => {
      console.error('Failed to add comment:', error);
    },
  });
}

// Add internal note mutation (for agents)
export function useAddInternalNote(ticketId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (content: string) => 
      apiClient.addComment(ticketId, { content, isInternal: true }),
    onSuccess: () => {
      // Invalidate ticket details to refetch with new comment
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(ticketId) });
      
      // Invalidate ticket lists (comment count changed)
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.queue() });
    },
    onError: (error) => {
      console.error('Failed to add internal note:', error);
    },
  });
}
