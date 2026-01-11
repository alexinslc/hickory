import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient, AddCommentRequest } from '../api-client';
import { ticketKeys } from './tickets';
import { useToast } from '@/components/ui/toast';

// Query keys
export const commentKeys = {
  all: ['comments'] as const,
  lists: () => [...commentKeys.all, 'list'] as const,
  list: (ticketId: string) => [...commentKeys.lists(), ticketId] as const,
  details: () => [...commentKeys.all, 'detail'] as const,
  detail: (id: string) => [...commentKeys.details(), id] as const,
};

// Get comments for a ticket
export function useGetComments(ticketId: string) {
  return useQuery({
    queryKey: commentKeys.list(ticketId),
    queryFn: () => apiClient.getComments(ticketId),
    enabled: !!ticketId,
  });
}

// Add comment mutation
export function useAddComment(ticketId: string) {
  const queryClient = useQueryClient();
  const toast = useToast();

  return useMutation({
    mutationFn: (data: AddCommentRequest) => apiClient.addComment(ticketId, data),
    onSuccess: () => {
      // Invalidate comments list to show new comment
      queryClient.invalidateQueries({ queryKey: commentKeys.list(ticketId) });
      
      // Invalidate ticket details to refetch with new comment count
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(ticketId) });
      
      // Invalidate ticket lists (comment count changed)
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.queue() });
      
      // Show success toast
      toast.success('Comment added successfully!');
    },
    onError: (error: Error) => {
      console.error('Failed to add comment:', error);
      toast.error(error.message || 'Failed to add comment. Please try again.');
    },
  });
}

// Add internal note mutation (for agents)
export function useAddInternalNote(ticketId: string) {
  const queryClient = useQueryClient();
  const toast = useToast();

  return useMutation({
    mutationFn: (content: string) => 
      apiClient.addComment(ticketId, { content, isInternal: true }),
    onSuccess: () => {
      // Invalidate comments list to show new note
      queryClient.invalidateQueries({ queryKey: commentKeys.list(ticketId) });
      
      // Invalidate ticket details to refetch with new comment
      queryClient.invalidateQueries({ queryKey: ticketKeys.detail(ticketId) });
      
      // Invalidate ticket lists (comment count changed)
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
      queryClient.invalidateQueries({ queryKey: ticketKeys.queue() });
      
      // Show success toast
      toast.success('Internal note added!');
    },
    onError: (error: Error) => {
      console.error('Failed to add internal note:', error);
      toast.error(error.message || 'Failed to add internal note. Please try again.');
    },
  });
}
