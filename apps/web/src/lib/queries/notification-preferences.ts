import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient, NotificationPreferencesDto, UpdateNotificationPreferencesRequest } from '@/lib/api-client';

export const NOTIFICATION_PREFERENCES_KEY = ['notification-preferences'];

/**
 * Hook to fetch notification preferences for the current user
 */
export function useNotificationPreferences() {
  return useQuery({
    queryKey: NOTIFICATION_PREFERENCES_KEY,
    queryFn: () => apiClient.getNotificationPreferences(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Hook to update notification preferences for the current user
 */
export function useUpdateNotificationPreferences() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateNotificationPreferencesRequest) =>
      apiClient.updateNotificationPreferences(request),
    onSuccess: (data: NotificationPreferencesDto) => {
      // Update the cache with the new preferences
      queryClient.setQueryData(NOTIFICATION_PREFERENCES_KEY, data);
    },
  });
}
