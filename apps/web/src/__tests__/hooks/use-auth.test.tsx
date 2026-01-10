import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useAuth, useLogin, useRegister, useLogout } from '../../hooks/use-auth';
import { apiClient } from '../../lib/api-client';
import { useAuthStore } from '../../store/auth-store';
import { ReactNode } from 'react';

// Type for the auth store state used in tests
interface AuthState {
  user: {
    userId: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
  } | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  setAuth: jest.Mock;
  clearAuth: jest.Mock;
  updateUser: jest.Mock;
}

// Mock next/navigation
jest.mock('next/navigation', () => ({
  useRouter: () => ({
    push: jest.fn(),
  }),
}));

// Mock the API client
jest.mock('../../lib/api-client', () => ({
  apiClient: {
    login: jest.fn(),
    register: jest.fn(),
  },
}));
const mockedApiClient = apiClient as jest.Mocked<typeof apiClient>;

// Mock the auth store
jest.mock('../../store/auth-store', () => ({
  useAuthStore: jest.fn(),
}));
const mockedUseAuthStore = useAuthStore as jest.MockedFunction<typeof useAuthStore>;

describe('useAuth hook', () => {
  let queryClient: QueryClient;

  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  // Helper function to mock the auth store with proper typing
  const mockAuthStore = (state: Partial<AuthState>) => {
    mockedUseAuthStore.mockImplementation(<T,>(selector: (state: AuthState) => T): T => {
      const fullState: AuthState = {
        user: null,
        accessToken: null,
        refreshToken: null,
        isAuthenticated: false,
        setAuth: jest.fn(),
        clearAuth: jest.fn(),
        updateUser: jest.fn(),
        ...state,
      };
      return selector(fullState);
    });
  };

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });

    // Reset mocks
    jest.clearAllMocks();

    // Setup default auth store mock
    mockAuthStore({});
  });

  afterEach(() => {
    queryClient.clear();
  });

  describe('login', () => {
    it('should login successfully with valid credentials', async () => {
      const mockResponse = {
        accessToken: 'mock-token',
        refreshToken: 'mock-refresh',
        userId: '123',
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
        role: 'User',
        expiresAt: '2025-12-31',
      };

      const setAuthMock = jest.fn();
      mockAuthStore({ setAuth: setAuthMock });

      mockedApiClient.login.mockResolvedValueOnce(mockResponse);

      const { result } = renderHook(() => useLogin(), { wrapper });

      result.current.mutate({
        email: 'test@example.com',
        password: 'password123',
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(mockedApiClient.login).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
      });

      expect(setAuthMock).toHaveBeenCalledWith(
        {
          userId: '123',
          email: 'test@example.com',
          firstName: 'John',
          lastName: 'Doe',
          role: 'User',
        },
        'mock-token',
        'mock-refresh',
        '2025-12-31'
      );
    });

    it('should handle login errors', async () => {
      const mockError = {
        response: {
          data: { message: 'Invalid credentials' },
          status: 401,
        },
      };

      mockAuthStore({});

      mockedApiClient.login.mockRejectedValueOnce(mockError);

      const { result } = renderHook(() => useLogin(), { wrapper });

      result.current.mutate({
        email: 'wrong@example.com',
        password: 'wrongpassword',
      });

      await waitFor(() => expect(result.current.isError).toBe(true));

      expect(result.current.error).toBeDefined();
    });

    it('should not call setAuth on login failure', async () => {
      const setAuthMock = jest.fn();
      mockAuthStore({ setAuth: setAuthMock });

      mockedApiClient.login.mockRejectedValueOnce(new Error('Network error'));

      const { result } = renderHook(() => useLogin(), { wrapper });

      result.current.mutate({
        email: 'test@example.com',
        password: 'password123',
      });

      await waitFor(() => expect(result.current.isError).toBe(true));

      expect(setAuthMock).not.toHaveBeenCalled();
    });
  });

  describe('logout', () => {
    it('should logout and clear user data', () => {
      const clearAuthMock = jest.fn();
      mockAuthStore({
        user: {
          userId: '123',
          email: 'test@example.com',
          firstName: 'John',
          lastName: 'Doe',
          role: 'User',
        },
        accessToken: 'token',
        refreshToken: 'refresh-token',
        isAuthenticated: true,
        clearAuth: clearAuthMock,
      });

      const { result } = renderHook(() => useLogout(), { wrapper });

      result.current();

      expect(clearAuthMock).toHaveBeenCalled();
    });
  });

  describe('register', () => {
    it('should register a new user successfully', async () => {
      const mockResponse = {
        accessToken: 'new-token',
        refreshToken: 'new-refresh-token',
        userId: '456',
        email: 'newuser@example.com',
        firstName: 'Jane',
        lastName: 'Smith',
        role: 'User',
        expiresAt: '2025-12-31',
      };

      const setAuthMock = jest.fn();
      mockAuthStore({ setAuth: setAuthMock });

      mockedApiClient.register.mockResolvedValueOnce(mockResponse);

      const { result } = renderHook(() => useRegister(), { wrapper });

      result.current.mutate({
        email: 'newuser@example.com',
        password: 'password123',
        firstName: 'Jane',
        lastName: 'Smith',
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(mockedApiClient.register).toHaveBeenCalledWith({
        email: 'newuser@example.com',
        password: 'password123',
        firstName: 'Jane',
        lastName: 'Smith',
      });

      expect(setAuthMock).toHaveBeenCalledWith(
        {
          userId: '456',
          email: 'newuser@example.com',
          firstName: 'Jane',
          lastName: 'Smith',
          role: 'User',
        },
        'new-token',
        'new-refresh-token',
        '2025-12-31'
      );
    });

    it('should handle registration validation errors', async () => {
      const mockError = {
        response: {
          data: { message: 'Email already exists' },
          status: 400,
        },
      };

      mockAuthStore({});

      mockedApiClient.register.mockRejectedValueOnce(mockError);

      const { result } = renderHook(() => useRegister(), { wrapper });

      result.current.mutate({
        email: 'existing@example.com',
        password: 'password123',
        firstName: 'John',
        lastName: 'Doe',
      });

      await waitFor(() => expect(result.current.isError).toBe(true));
    });
  });

  describe('isAuthenticated', () => {
    it('should return false when no user is logged in', () => {
      mockAuthStore({ isAuthenticated: false });

      const { result } = renderHook(() => useAuth(), { wrapper });

      expect(result.current.isAuthenticated).toBe(false);
    });

    it('should return true when user is logged in', () => {
      mockAuthStore({
        user: {
          userId: '123',
          email: 'test@example.com',
          firstName: 'John',
          lastName: 'Doe',
          role: 'User',
        },
        accessToken: 'token',
        refreshToken: 'refresh-token',
        isAuthenticated: true,
      });

      const { result } = renderHook(() => useAuth(), { wrapper });

      expect(result.current.isAuthenticated).toBe(true);
    });
  });
});
