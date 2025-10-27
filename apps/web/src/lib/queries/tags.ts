import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../api-client';

// Query keys
export const tagKeys = {
  all: ['tags'] as const,
  lists: () => [...tagKeys.all, 'list'] as const,
  list: () => [...tagKeys.lists()] as const,
};

// Get all tags
export function useGetAllTags() {
  return useQuery({
    queryKey: tagKeys.list(),
    queryFn: () => apiClient.getAllTags(),
  });
}
