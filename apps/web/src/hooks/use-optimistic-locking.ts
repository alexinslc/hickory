import { useState } from 'react';
import { AxiosError } from 'axios';

interface ConflictError {
  isConflict: boolean;
  message: string;
  currentVersion?: string;
}

interface UseOptimisticLockingOptions {
  onRetry?: () => void;
  onConflict?: (error: ConflictError) => void;
}

/**
 * Hook to handle optimistic locking conflicts (409 Conflict responses)
 * Provides conflict detection, user notification, and retry capabilities
 */
export function useOptimisticLocking(options: UseOptimisticLockingOptions = {}) {
  const [conflict, setConflict] = useState<ConflictError | null>(null);
  const [showRetryDialog, setShowRetryDialog] = useState(false);

  const handleError = (error: unknown): boolean => {
    if (error instanceof AxiosError) {
      // Check for 409 Conflict response
      if (error.response?.status === 409) {
        const conflictError: ConflictError = {
          isConflict: true,
          message: error.response.data?.detail || 
                   error.response.data?.message || 
                   'This item has been modified by another user. Please refresh and try again.',
          currentVersion: error.response.data?.currentVersion,
        };
        
        setConflict(conflictError);
        setShowRetryDialog(true);
        
        if (options.onConflict) {
          options.onConflict(conflictError);
        }
        
        return true; // Indicates this was a conflict error
      }
    }
    
    return false; // Not a conflict error
  };

  const handleRetry = () => {
    setShowRetryDialog(false);
    setConflict(null);
    
    if (options.onRetry) {
      options.onRetry();
    }
  };

  const dismissConflict = () => {
    setShowRetryDialog(false);
    setConflict(null);
  };

  return {
    conflict,
    showRetryDialog,
    handleError,
    handleRetry,
    dismissConflict,
    isConflictError: (error: unknown): boolean => {
      return error instanceof AxiosError && error.response?.status === 409;
    },
  };
}
