'use client';

import { useMutation, useQuery, useQueryClient, UseQueryResult } from '@tanstack/react-query';
import { apiClient, TicketDto, CreateTicketRequest, CreateTicketResponse } from '@/lib/api-client';
import { useRouter } from 'next/navigation';

// Query keys
export const ticketKeys = {
  all: ['tickets'] as const,
  lists: () => [...ticketKeys.all, 'list'] as const,
  list: (filters?: Record<string, unknown>) => [...ticketKeys.lists(), filters] as const,
  details: () => [...ticketKeys.all, 'detail'] as const,
  detail: (id: string) => [...ticketKeys.details(), id] as const,
};

// Get all user's tickets
export function useMyTickets() {
  return useQuery({
    queryKey: ticketKeys.list(),
    queryFn: () => apiClient.getMyTickets(),
    staleTime: 30000, // 30 seconds
  });
}

// Get ticket by ID
export function useTicket(id: string): UseQueryResult<TicketDto, Error> {
  return useQuery({
    queryKey: ticketKeys.detail(id),
    queryFn: () => apiClient.getTicketById(id),
    staleTime: 30000,
  });
}

// Create ticket mutation
export function useCreateTicket() {
  const queryClient = useQueryClient();
  const router = useRouter();

  return useMutation({
    mutationFn: (request: CreateTicketRequest) => apiClient.createTicket(request),
    onSuccess: (data: CreateTicketResponse) => {
      // Invalidate tickets list
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
      
      // Redirect to ticket detail page
      router.push(`/tickets/${data.id}`);
    },
  });
}
