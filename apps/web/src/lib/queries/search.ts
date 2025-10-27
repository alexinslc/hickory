import { useQuery } from '@tanstack/react-query';
import { apiClient, SearchTicketsParams } from '../api-client';

// Query keys
export const searchKeys = {
  all: ['search'] as const,
  searches: () => [...searchKeys.all, 'tickets'] as const,
  search: (params: SearchTicketsParams) => [...searchKeys.searches(), params] as const,
};

// Search tickets
export function useSearchTickets(params: SearchTicketsParams, enabled = true) {
  return useQuery({
    queryKey: searchKeys.search(params),
    queryFn: () => apiClient.searchTickets(params),
    enabled,
    staleTime: 30000, // 30 seconds
    gcTime: 5 * 60 * 1000, // 5 minutes (formerly cacheTime)
  });
}
