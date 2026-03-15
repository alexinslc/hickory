'use client';

import { useMutation, useQuery, useQueryClient, UseQueryResult } from '@tanstack/react-query';
import { apiClient, TicketDto, CreateTicketRequest, CreateTicketResponse, TicketDetailsResponse, PaginatedResult, PaginationParams } from '@/lib/api-client';
import { useRouter } from 'next/navigation';
import { useToast } from '@/components/ui/toast';

// Query keys
export const ticketKeys = {
  all: ['tickets'] as const,
  lists: () => [...ticketKeys.all, 'list'] as const,
  list: (filters?: Record<string, unknown>) => [...ticketKeys.lists(), filters] as const,
  details: () => [...ticketKeys.all, 'detail'] as const,
  detail: (id: string) => [...ticketKeys.details(), id] as const,
  detailsFull: (id: string) => [...ticketKeys.details(), id, 'full'] as const,
};

// Get user's tickets with pagination
export function useMyTickets(params?: PaginationParams): UseQueryResult<PaginatedResult<TicketDto>, Error> {
  return useQuery({
    queryKey: ticketKeys.list({ ...params }),
    queryFn: () => apiClient.getMyTickets(params),
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

// Get ticket details with comments and attachments
export function useTicketDetails(id: string): UseQueryResult<TicketDetailsResponse, Error> {
  return useQuery({
    queryKey: ticketKeys.detailsFull(id),
    queryFn: () => apiClient.getTicketDetails(id),
    enabled: !!id,
    staleTime: 10000, // 10 seconds
  });
}

// Create ticket mutation
export function useCreateTicket() {
  const queryClient = useQueryClient();
  const router = useRouter();
  const toast = useToast();

  return useMutation({
    mutationFn: (request: CreateTicketRequest) => apiClient.createTicket(request),
    onSuccess: (data: CreateTicketResponse) => {
      // Invalidate tickets list
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
      
      // Show success toast
      toast.success('Ticket created successfully!');
      
      // Redirect to ticket detail page
      router.push(`/tickets/${data.id}`);
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to create ticket. Please try again.');
    },
  });
}

// Create ticket with attachments mutation
interface CreateTicketWithAttachmentsRequest extends CreateTicketRequest {
  files: File[];
}

export function useCreateTicketWithAttachments() {
  const queryClient = useQueryClient();
  const router = useRouter();
  const toast = useToast();

  return useMutation({
    mutationFn: async (request: CreateTicketWithAttachmentsRequest) => {
      const { files, ...ticketData } = request;
      
      // First, create the ticket
      const ticketResponse = await apiClient.createTicket(ticketData);
      
      // Then, upload all attachments
      if (files.length > 0) {
        const uploadPromises = files.map((file) =>
          apiClient.uploadAttachment(ticketResponse.id, file).catch((error) => {
            console.error(`Failed to upload ${file.name}:`, error);
            return null; // Continue with other uploads even if one fails
          })
        );
        
        const results = await Promise.all(uploadPromises);
        const failedCount = results.filter((r) => r === null).length;
        
        if (failedCount > 0 && failedCount < files.length) {
          toast.warning(`Ticket created, but ${failedCount} attachment(s) failed to upload.`);
        } else if (failedCount === files.length && files.length > 0) {
          toast.warning('Ticket created, but all attachments failed to upload.');
        }
      }
      
      return ticketResponse;
    },
    onSuccess: (data: CreateTicketResponse) => {
      // Invalidate tickets list
      queryClient.invalidateQueries({ queryKey: ticketKeys.lists() });
      
      // Show success toast
      toast.success('Ticket created successfully!');
      
      // Redirect to ticket detail page
      router.push(`/tickets/${data.id}`);
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to create ticket. Please try again.');
    },
  });
}
