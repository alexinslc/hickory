import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient, LoginRequest, RegisterRequest, AuthResponse } from '@/lib/api-client';
import { useAuthStore } from '@/store/auth-store';
import { useRouter } from 'next/navigation';

export function useLogin() {
  const setAuth = useAuthStore((state) => state.setAuth);
  const router = useRouter();
  const queryClient = useQueryClient();

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
        data.refreshToken
      );
      queryClient.invalidateQueries();
      router.push('/dashboard');
    },
  });
}

export function useRegister() {
  const setAuth = useAuthStore((state) => state.setAuth);
  const router = useRouter();
  const queryClient = useQueryClient();

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
        data.refreshToken
      );
      queryClient.invalidateQueries();
      router.push('/dashboard');
    },
  });
}

export function useLogout() {
  const clearAuth = useAuthStore((state) => state.clearAuth);
  const router = useRouter();
  const queryClient = useQueryClient();

  return () => {
    clearAuth();
    queryClient.clear();
    router.push('/auth/login');
  };
}
