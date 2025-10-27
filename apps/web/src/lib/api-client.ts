import axios, { AxiosInstance, AxiosError } from 'axios';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface ErrorResponse {
  status: number;
  title: string;
  detail?: string;
  traceId: string;
  timestamp: string;
  errors?: Record<string, string[]>;
}

class ApiClient {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor to add auth token
    this.client.interceptors.request.use(
      (config) => {
        // Get token from auth store (Zustand with persist middleware)
        const token = this.getTokenFromStore();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor for error handling
    this.client.interceptors.response.use(
      (response) => response,
      async (error: AxiosError<ErrorResponse>) => {
        if (error.response?.status === 401) {
          // Token expired - clear auth state and redirect
          this.handleUnauthorized();
        }
        return Promise.reject(error);
      }
    );
  }

  private getTokenFromStore(): string | null {
    if (typeof window === 'undefined') return null;
    
    // Read from the persisted auth store (localStorage key: 'hickory-auth-storage')
    try {
      const stored = localStorage.getItem('hickory-auth-storage');
      if (!stored) return null;
      
      const parsed = JSON.parse(stored);
      return parsed.state?.accessToken || null;
    } catch {
      return null;
    }
  }

  private handleUnauthorized(): void {
    if (typeof window === 'undefined') return;
    
    // Clear the entire auth store by removing the persisted state
    localStorage.removeItem('hickory-auth-storage');
    
    // Redirect to login
    window.location.href = '/auth/login';
  }

  // Auth endpoints
  async login(request: LoginRequest): Promise<AuthResponse> {
    const response = await this.client.post<AuthResponse>('/api/auth/login', request);
    return response.data;
  }

  async register(request: RegisterRequest): Promise<AuthResponse> {
    const response = await this.client.post<AuthResponse>('/api/auth/register', request);
    return response.data;
  }

  // Health check
  async healthCheck(): Promise<{ status: string }> {
    const response = await this.client.get('/health');
    return response.data;
  }
}

export const apiClient = new ApiClient();
