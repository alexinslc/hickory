import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface User {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: string | null;
  isAuthenticated: boolean;
  
  setAuth: (user: User, accessToken: string, refreshToken: string, expiresAt: string) => void;
  clearAuth: () => void;
  updateUser: (user: Partial<User>) => void;
  updateTokens: (accessToken: string, refreshToken: string, expiresAt: string) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      expiresAt: null,
      isAuthenticated: false,

      setAuth: (user, accessToken, refreshToken, expiresAt) =>
        set({
          user,
          accessToken,
          refreshToken,
          expiresAt,
          isAuthenticated: true,
        }),

      clearAuth: () =>
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
          expiresAt: null,
          isAuthenticated: false,
        }),

      updateUser: (updatedUser) =>
        set((state) => ({
          user: state.user ? { ...state.user, ...updatedUser } : null,
        })),

      updateTokens: (accessToken, refreshToken, expiresAt) =>
        set({
          accessToken,
          refreshToken,
          expiresAt,
        }),
    }),
    {
      name: 'hickory-auth-storage',
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        expiresAt: state.expiresAt,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
