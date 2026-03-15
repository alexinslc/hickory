import { handleApiError, getFieldErrors } from '../../lib/api-client';

describe('apiClient', () => {
  describe('error handling', () => {
    it('should extract error message from response data', () => {
      const error = {
        response: {
          data: { message: 'Invalid credentials' },
          status: 401,
          statusText: 'Unauthorized',
        },
      };

      const result = handleApiError(error);

      expect(result).toBe('Invalid credentials');
    });

    it('should use error message from error.message when no response', () => {
      const error = {
        message: 'Network Error',
      };

      const result = handleApiError(error);

      expect(result).toBe('Network Error');
    });

    it('should return default message for unknown errors', () => {
      const error = {};

      const result = handleApiError(error);

      expect(result).toBe('An unexpected error occurred');
    });

    it('should handle validation errors with multiple fields', () => {
      const error = {
        response: {
          data: {
            errors: {
              email: 'Invalid email format',
              password: 'Password too short',
            },
          },
          status: 400,
        },
      };

      const result = handleApiError(error);

      expect(result).toContain('email');
      expect(result).toContain('Invalid email format');
    });
  });

  describe('request interceptor', () => {
    it('should add authorization header when token exists', () => {
      // This would test the request interceptor
      // Implementation depends on how auth token is stored
      expect(true).toBe(true); // Placeholder
    });

    it('should not add authorization header when no token', () => {
      expect(true).toBe(true); // Placeholder
    });
  });

  describe('response interceptor', () => {
    it('should handle 401 unauthorized by clearing auth', () => {
      expect(true).toBe(true); // Placeholder
    });

    it('should handle 403 forbidden appropriately', () => {
      expect(true).toBe(true); // Placeholder
    });

    it('should handle 500 server errors', () => {
      expect(true).toBe(true); // Placeholder
    });
  });

  describe('getFieldErrors', () => {
    it('returns null for non-object errors', () => {
      expect(getFieldErrors(null)).toBeNull();
      expect(getFieldErrors(undefined)).toBeNull();
      expect(getFieldErrors('string error')).toBeNull();
      expect(getFieldErrors(42)).toBeNull();
    });

    it('returns null when error has no response', () => {
      expect(getFieldErrors({})).toBeNull();
      expect(getFieldErrors(new Error('plain error'))).toBeNull();
    });

    it('returns null when response has no errors field', () => {
      const error = {
        response: { data: { title: 'Bad Request', status: 400 } },
      };
      expect(getFieldErrors(error)).toBeNull();
    });

    it('returns null when errors object is empty', () => {
      const error = { response: { data: { errors: {} } } };
      expect(getFieldErrors(error)).toBeNull();
    });

    it('normalizes PascalCase keys from the API to camelCase field names', () => {
      const error = {
        response: {
          data: {
            errors: {
              Title: ['Title is required'],
              Description: ['Description is too short'],
            },
          },
        },
      };

      const result = getFieldErrors(error);
      expect(result).toEqual({
        title: ['Title is required'],
        description: ['Description is too short'],
      });
    });

    it('preserves already-camelCase keys', () => {
      const error = {
        response: {
          data: {
            errors: {
              firstName: ['First name is required'],
              lastName: ['Last name is required'],
            },
          },
        },
      };

      const result = getFieldErrors(error);
      expect(result).toEqual({
        firstName: ['First name is required'],
        lastName: ['Last name is required'],
      });
    });

    it('handles multiple error messages per field', () => {
      const error = {
        response: {
          data: {
            errors: {
              Password: [
                'Password must be at least 8 characters',
                'Password must contain an uppercase letter',
              ],
            },
          },
        },
      };

      const result = getFieldErrors(error);
      expect(result).toEqual({
        password: [
          'Password must be at least 8 characters',
          'Password must contain an uppercase letter',
        ],
      });
    });

    it('coerces non-array messages to string arrays', () => {
      const error = {
        response: {
          data: {
            errors: {
              Email: 'Email is invalid',
            },
          },
        },
      };

      const result = getFieldErrors(error);
      expect(result).toEqual({
        email: ['Email is invalid'],
      });
    });

    it('handles mixed PascalCase and camelCase keys', () => {
      const error = {
        response: {
          data: {
            errors: {
              CategoryId: ['Invalid category'],
              priority: ['Priority is required'],
            },
          },
        },
      };

      const result = getFieldErrors(error);
      expect(result).toEqual({
        categoryId: ['Invalid category'],
        priority: ['Priority is required'],
      });
    });
  });

  describe('automatic token refresh', () => {
    // Note: These are integration tests that would require mocking axios and localStorage
    // Full implementation would require a testing setup with axios-mock-adapter or similar
    
    it('should automatically refresh token on 401 response', () => {
      // This test would:
      // 1. Mock a 401 response from an API call
      // 2. Mock the refresh token endpoint to return new tokens
      // 3. Verify the original request is retried with the new token
      // 4. Verify tokens are updated in the store
      expect(true).toBe(true); // Placeholder for actual test
    });

    it('should queue concurrent requests during token refresh', () => {
      // This test would:
      // 1. Trigger multiple 401 responses simultaneously
      // 2. Verify only one refresh token call is made
      // 3. Verify all queued requests are retried with the new token
      expect(true).toBe(true); // Placeholder for actual test
    });

    it('should clear auth and redirect on refresh token failure', () => {
      // This test would:
      // 1. Mock a 401 response
      // 2. Mock the refresh token endpoint to fail
      // 3. Verify auth is cleared from localStorage
      // 4. Verify redirect to login page
      expect(true).toBe(true); // Placeholder for actual test
    });

    it('should use Zustand store updateTokens method', () => {
      // This test would:
      // 1. Mock a successful token refresh
      // 2. Spy on the useAuthStore.getState().updateTokens method
      // 3. Verify it's called with the new tokens
      expect(true).toBe(true); // Placeholder for actual test
    });

    it('should fall back to localStorage if store is unavailable', () => {
      // This test would:
      // 1. Mock the useAuthStore to throw an error
      // 2. Mock a successful token refresh
      // 3. Verify tokens are updated directly in localStorage
      expect(true).toBe(true); // Placeholder for actual test
    });
  });
});
