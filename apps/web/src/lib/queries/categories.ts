import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient, CreateCategoryCommand } from '../api-client';

// Query keys
export const categoryKeys = {
  all: ['categories'] as const,
  lists: () => [...categoryKeys.all, 'list'] as const,
  list: () => [...categoryKeys.lists()] as const,
};

// Get all categories
export function useGetAllCategories() {
  return useQuery({
    queryKey: categoryKeys.list(),
    queryFn: () => apiClient.getAllCategories(),
  });
}

// Create category (admin only)
export function useCreateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (command: CreateCategoryCommand) => apiClient.createCategory(command),
    onSuccess: () => {
      // Invalidate and refetch categories
      queryClient.invalidateQueries({ queryKey: categoryKeys.all });
    },
  });
}
