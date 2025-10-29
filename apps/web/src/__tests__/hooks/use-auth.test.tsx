import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useAuth } from '../../hooks/use-auth';
import { apiClient } from '../../lib/api-client';
import { useAuthStore } from '../../store/auth-store';
import { ReactNode } from 'react';

// Mock the API client
jest.mock('../../lib/api-client');
const mockedApiClient = apiClient as jest.Mocked<typeof apiClient>;

// Mock the auth store
jest.mock('../../store/auth-store');
const mockedUseAuthStore = useAuthStore as unknown as jest.Mock;

describe('useAuth hook', () => {
  let queryClient: QueryClient;

  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

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
    mockedUseAuthStore.mockReturnValue({
      user: null,
      setUser: jest.fn(),
      clearUser: jest.fn(),
    });
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
        expiresAt: new Date('2025-12-31'),
      };

      const setUserMock = jest.fn();
      mockedUseAuthStore.mockReturnValue({
        user: null,
        setUser: setUserMock,
        clearUser: jest.fn(),
      });

      mockedApiClient.post.mockResolvedValueOnce({ data: mockResponse });

      const { result } = renderHook(() => useAuth(), { wrapper });

      result.current.login.mutate({
        email: 'test@example.com',
        password: 'password123',
      });

      await waitFor(() => expect(result.current.login.isSuccess).toBe(true));

      expect(mockedApiClient.post).toHaveBeenCalledWith('/auth/login', {
        email: 'test@example.com',
        password: 'password123',
      });

      expect(setUserMock).toHaveBeenCalledWith(mockResponse);
    });

    it('should handle login errors', async () => {
      const mockError = {
        response: {
          data: { message: 'Invalid credentials' },
          status: 401,
        },
      };

      mockedApiClient.post.mockRejectedValueOnce(mockError);

      const { result } = renderHook(() => useAuth(), { wrapper });

      result.current.login.mutate({
        email: 'wrong@example.com',
        password: 'wrongpassword',
      });

      await waitFor(() => expect(result.current.login.isError).toBe(true));

      expect(result.current.login.error).toBeDefined();
    });

    it('should not call setUser on login failure', async () => {
      const setUserMock = jest.fn();
      mockedUseAuthStore.mockReturnValue({
        user: null,
        setUser: setUserMock,
        clearUser: jest.fn(),
      });

      mockedApiClient.post.mockRejectedValueOnce(new Error('Network error'));

      const { result } = renderHook(() => useAuth(), { wrapper });

      result.current.login.mutate({
        email: 'test@example.com',
        password: 'password123',
      });

      await waitFor(() => expect(result.current.login.isError).toBe(true));

      expect(setUserMock).not.toHaveBeenCalled();
    });
  });

  describe('logout', () => {
    it('should logout and clear user data', async () => {
      const clearUserMock = jest.fn();
      mockedUseAuthStore.mockReturnValue({
        user: {
          userId: '123',
          email: 'test@example.com',
          firstName: 'John',
          lastName: 'Doe',
          role: 'User',
          accessToken: 'token',
        },
        setUser: jest.fn(),
        clearUser: clearUserMock,
      });

      mockedApiClient.post.mockResolvedValueOnce({ data: { message: 'Logged out' } });

      const { result } = renderHook(() => useAuth(), { wrapper });

      result.current.logout.mutate();

      await waitFor(() => expect(result.current.logout.isSuccess).toBe(true));

      expect(mockedApiClient.post).toHaveBeenCalledWith('/auth/logout');
      expect(clearUserMock).toHaveBeenCalled();
    });

    it('should clear user data even if logout API call fails', async () => {
      const clearUserMock = jest.fn();
      mockedUseAuthStore.mockReturnValue({
        user: { userId: '123', email: 'test@example.com' },
        setUser: jest.fn(),
        clearUser: clearUserMock,
      });

      mockedApiClient.post.mockRejectedValueOnce(new Error('Network error'));

      const { result } = renderHook(() => useAuth(), { wrapper });

      result.current.logout.mutate();

      await waitFor(() => expect(clearUserMock).toHaveBeenCalled());
    });
  });

  describe('register', () => {
    it('should register a new user successfully', async () => {
      const mockResponse = {
        accessToken: 'new-token',
        userId: '456',
        email: 'newuser@example.com',
        firstName: 'Jane',
        lastName: 'Smith',
        role: 'User',
      };

      mockedApiClient.post.mockResolvedValueOnce({ data: mockResponse });

      const { result } = renderHook(() => useAuth(), { wrapper });

      result.current.register.mutate({
        email: 'newuser@example.com',
        password: 'password123',
        firstName: 'Jane',
        lastName: 'Smith',
      });

      await waitFor(() => expect(result.current.register.isSuccess).toBe(true));

      expect(mockedApiClient.post).toHaveBeenCalledWith('/auth/register', {
        email: 'newuser@example.com',
        password: 'password123',
        firstName: 'Jane',
        lastName: 'Smith',
      });
    });

    it('should handle registration validation errors', async () => {
      const mockError = {
        response: {
          data: { message: 'Email already exists' },
          status: 400,
        },
      };

      mockedApiClient.post.mockRejectedValueOnce(mockError);

      const { result } = renderHook(() => useAuth(), { wrapper });

      result.current.register.mutate({
        email: 'existing@example.com',
        password: 'password123',
        firstName: 'John',
        lastName: 'Doe',
      });

      await waitFor(() => expect(result.current.register.isError).toBe(true));
    });
  });

  describe('isAuthenticated', () => {
    it('should return false when no user is logged in', () => {
      mockedUseAuthStore.mockReturnValue({
        user: null,
        setUser: jest.fn(),
        clearUser: jest.fn(),
      });

      const { result } = renderHook(() => useAuth(), { wrapper });

      expect(result.current.isAuthenticated).toBe(false);
    });

    it('should return true when user is logged in', () => {
      mockedUseAuthStore.mockReturnValue({
        user: {
          userId: '123',
          email: 'test@example.com',
          firstName: 'John',
          lastName: 'Doe',
          role: 'User',
          accessToken: 'token',
        },
        setUser: jest.fn(),
        clearUser: jest.fn(),
      });

      const { result } = renderHook(() => useAuth(), { wrapper });

      expect(result.current.isAuthenticated).toBe(true);
    });
  });
});
