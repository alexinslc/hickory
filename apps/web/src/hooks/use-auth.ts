import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient, LoginRequest, RegisterRequest, AuthResponse } from '@/lib/api-client';
import { useAuthStore } from '@/store/auth-store';
import { useRouter } from 'next/navigation';
import { useToast } from '@/components/ui/toast';

export function useAuth() {
  const user = useAuthStore((state) => state.user);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  
  return {
    user,
    isAuthenticated,
    isLoading: false, // Auth state is synchronous from Zustand
  };
}

export function useLogin() {
  const setAuth = useAuthStore((state) => state.setAuth);
  const router = useRouter();
  const queryClient = useQueryClient();
  const toast = useToast();

  return useMutation({
    mutationFn: (request: LoginRequest) => apiClient.login(request),
    onSuccess: (data: AuthResponse) => {
      setAuth(
        {
          userId: data.userId,
          email: data.email,
          firstName: data.firstName,
          lastName: data.lastName,
          role: data.role,
        },
        data.accessToken,
        data.refreshToken,
        data.expiresAt
      );
      queryClient.invalidateQueries();
      toast.success(`Welcome back, ${data.firstName}!`);
      router.push('/dashboard');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Login failed. Please check your credentials.');
    },
  });
}

export function useRegister() {
  const setAuth = useAuthStore((state) => state.setAuth);
  const router = useRouter();
  const queryClient = useQueryClient();
  const toast = useToast();

  return useMutation({
    mutationFn: (request: RegisterRequest) => apiClient.register(request),
    onSuccess: (data: AuthResponse) => {
      setAuth(
        {
          userId: data.userId,
          email: data.email,
          firstName: data.firstName,
          lastName: data.lastName,
          role: data.role,
        },
        data.accessToken,
        data.refreshToken,
        data.expiresAt
      );
      queryClient.invalidateQueries();
      toast.success(`Welcome to Hickory, ${data.firstName}!`);
      router.push('/dashboard');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Registration failed. Please try again.');
    },
  });
}

export function useLogout() {
  const clearAuth = useAuthStore((state) => state.clearAuth);
  const router = useRouter();
  const queryClient = useQueryClient();
  const toast = useToast();

  return () => {
    clearAuth();
    queryClient.clear();
    toast.info('You have been logged out.');
    router.push('/auth/login');
  };
}
