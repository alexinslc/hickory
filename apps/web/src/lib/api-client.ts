import axios, { AxiosInstance, AxiosError, AxiosRequestConfig } from 'axios';

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

export interface TicketDto {
  id: string;
  ticketNumber: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  submitterId: string;
  submitterName: string;
  assignedToId?: string;
  assignedToName?: string;
  createdAt: string;
  updatedAt: string;
  closedAt?: string;
  resolutionNotes?: string;
  commentCount: number;
  rowVersion: string; // Base64 encoded RowVersion for optimistic concurrency control
  categoryId?: string;
  categoryName?: string;
  tags: string[];
  // Note: Comments are not included in this DTO. Use a separate endpoint to fetch comments.
}

/**
 * Comment data transfer object
 * Note: Currently used for the addComment API response.
 * Comments are not returned with ticket details - a separate endpoint
 * will be added in a future phase to fetch the comments list for a ticket.
 */
export interface CommentDto {
  id: string;
  content: string;
  isInternal: boolean;
  authorId: string;
  authorName: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: string;
  categoryId?: string;
}

export interface CreateTicketResponse {
  id: string;
  ticketNumber: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  createdAt: string;
}

export interface AddCommentRequest {
  content: string;
  isInternal: boolean;
}

export interface AssignTicketRequest {
  agentId: string;
  rowVersion: string;
}

export interface UpdateTicketStatusRequest {
  newStatus: string;
  rowVersion: string;
}

export interface UpdateTicketPriorityRequest {
  newPriority: string;
  rowVersion: string;
}

export interface CloseTicketRequest {
  resolutionNotes: string;
  rowVersion: string;
}

export interface ReassignTicketRequest {
  newAgentId: string;
  rowVersion: string;
}

export interface CategoryDto {
  id: string;
  name: string;
  description?: string;
  displayOrder: number;
  color?: string;
  isActive: boolean;
}

export interface CreateCategoryCommand {
  name: string;
  description?: string;
  displayOrder: number;
  color?: string;
}

export interface TagDto {
  id: string;
  name: string;
  color?: string;
}

export interface SearchTicketsParams {
  q?: string;
  status?: string;
  priority?: string;
  assignedToId?: string;
  createdAfter?: string;
  createdBefore?: string;
  page?: number;
  pageSize?: number;
}

export interface SearchTicketsResult {
  tickets: TicketDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface NotificationPreferencesDto {
  emailEnabled: boolean;
  emailOnTicketCreated: boolean;
  emailOnTicketUpdated: boolean;
  emailOnTicketAssigned: boolean;
  emailOnCommentAdded: boolean;
  
  inAppEnabled: boolean;
  inAppOnTicketCreated: boolean;
  inAppOnTicketUpdated: boolean;
  inAppOnTicketAssigned: boolean;
  inAppOnCommentAdded: boolean;
  
  webhookEnabled: boolean;
  webhookUrl?: string;
  // Note: webhookSecret is write-only, not returned in GET responses
}

export interface UpdateNotificationPreferencesRequest {
  emailEnabled: boolean;
  emailOnTicketCreated: boolean;
  emailOnTicketUpdated: boolean;
  emailOnTicketAssigned: boolean;
  emailOnCommentAdded: boolean;
  
  inAppEnabled: boolean;
  inAppOnTicketCreated: boolean;
  inAppOnTicketUpdated: boolean;
  inAppOnTicketAssigned: boolean;
  inAppOnCommentAdded: boolean;
  
  webhookEnabled: boolean;
  webhookUrl?: string;
  webhookSecret?: string;
}

// Knowledge Base types
export interface ArticleDto {
  id: string;
  title: string;
  content: string;
  category: string;
  tags: string[];
  status: string;
  viewCount: number;
  helpfulCount: number;
  notHelpfulCount: number;
  authorId: string;
  authorName: string;
  createdAt: string;
  updatedAt: string;
  publishedAt?: string;
}

export interface ArticleListItemDto {
  id: string;
  title: string;
  category: string;
  tags: string[];
  status: string;
  viewCount: number;
  helpfulCount: number;
  notHelpfulCount: number;
  publishedAt?: string;
}

export interface SearchArticlesRequest {
  searchTerm?: string;
  category?: string;
  tags?: string[];
  status?: string;
  page?: number;
  pageSize?: number;
}

export interface SearchArticlesResult {
  articles: ArticleListItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateArticleRequest {
  title: string;
  content: string;
  category: string;
  tags?: string[];
  status?: string;
}

export interface UpdateArticleRequest {
  title?: string;
  content?: string;
  category?: string;
  tags?: string[];
  status?: string;
}

export interface RateArticleRequest {
  isHelpful: boolean;
}

export interface GetSuggestedArticlesRequest {
  categoryId?: string;
  searchTerm?: string;
  limit?: number;
}

// Custom interface to extend AxiosRequestConfig for retry tracking
interface AxiosRequestConfigWithRetry extends AxiosRequestConfig {
  _retry?: boolean;
}

class ApiClient {
  private client: AxiosInstance;
  private isRefreshing = false;
  private failedQueue: Array<{
    resolve: (value: string) => void;
    reject: (reason: Error) => void;
  }> = [];

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
        const originalRequest = error.config as AxiosRequestConfigWithRetry | undefined;
        
        // If error is 401 and we haven't tried refreshing yet
        if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
          if (this.isRefreshing) {
            // Wait for the current refresh to complete
            return new Promise((resolve, reject) => {
              this.failedQueue.push({ resolve, reject });
            })
              .then((token) => {
                if (originalRequest.headers) {
                  originalRequest.headers.Authorization = `Bearer ${token}`;
                }
                return this.client(originalRequest);
              })
              .catch((err) => {
                return Promise.reject(err);
              });
          }

          originalRequest._retry = true;
          this.isRefreshing = true;

          try {
            const newToken = await this.refreshAccessToken();
            this.isRefreshing = false;
            
            // Retry all queued requests with new token
            this.processQueue(null, newToken);
            
            // Retry the original request
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${newToken}`;
            }
            return this.client(originalRequest);
          } catch (refreshError) {
            this.isRefreshing = false;
            this.processQueue(refreshError as Error);
            this.handleUnauthorized();
            return Promise.reject(refreshError);
          }
        }

        return Promise.reject(error);
      }
    );
  }

  private processQueue(error: Error): void;
  private processQueue(error: null, token: string): void;
  private processQueue(error: Error | null, token?: string | null): void {
    this.failedQueue.forEach((prom) => {
      if (error) {
        prom.reject(error);
      } else if (token !== null && token !== undefined) {
        prom.resolve(token);
      } else {
        // This should never happen given the overloads above
        prom.reject(new Error('Both error and token are null in processQueue'));
      }
    });

    this.failedQueue = [];
  }

  private async refreshAccessToken(): Promise<string> {
    const refreshToken = this.getRefreshTokenFromStore();
    
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    try {
      // Call refresh endpoint without interceptors to avoid infinite loop
      const response = await axios.post<AuthResponse>(
        `${API_BASE_URL}/api/v1/auth/refresh`,
        { refreshToken }
      );

      const { accessToken, refreshToken: newRefreshToken, expiresAt } = response.data;

      // Update stored tokens using Zustand store
      this.updateTokensInStore(accessToken, newRefreshToken, expiresAt);

      return accessToken;
    } catch (error) {
      // Clear auth on refresh failure
      this.handleUnauthorized();
      throw error;
    }
  }

  private getRefreshTokenFromStore(): string | null {
    if (typeof window === 'undefined') return null;
    
    try {
      const stored = localStorage.getItem('hickory-auth-storage');
      if (!stored) return null;
      
      const parsed = JSON.parse(stored);
      return parsed.state?.refreshToken || null;
    } catch {
      return null;
    }
  }

  private updateTokensInStore(accessToken: string, refreshToken: string, expiresAt: string): void {
    if (typeof window === 'undefined') return;
    
    try {
      // Use dynamic import to avoid circular dependencies and ensure we get the latest state
      const { useAuthStore } = require('../store/auth-store');
      const updateTokens = useAuthStore.getState().updateTokens;
      updateTokens(accessToken, refreshToken, expiresAt);
    } catch (error) {
      console.error('Failed to update tokens in store:', error);
      // Fallback to direct localStorage update if store is not available
      try {
        const stored = localStorage.getItem('hickory-auth-storage');
        if (!stored) return;
        
        const parsed = JSON.parse(stored);
        if (parsed.state) {
          parsed.state.accessToken = accessToken;
          parsed.state.refreshToken = refreshToken;
          parsed.state.expiresAt = expiresAt;
          localStorage.setItem('hickory-auth-storage', JSON.stringify(parsed));
        }
      } catch (fallbackError) {
        console.error('Failed to update tokens in localStorage:', fallbackError);
      }
    }
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
    const response = await this.client.post<AuthResponse>('/api/v1/auth/login', request);
    return response.data;
  }

  async register(request: RegisterRequest): Promise<AuthResponse> {
    const response = await this.client.post<AuthResponse>('/api/v1/auth/register', request);
    return response.data;
  }

  // Ticket endpoints
  async createTicket(request: CreateTicketRequest): Promise<CreateTicketResponse> {
    const response = await this.client.post<CreateTicketResponse>('/api/tickets', request);
    return response.data;
  }

  async getTicketById(id: string): Promise<TicketDto> {
    const response = await this.client.get<TicketDto>(`/api/tickets/${id}`);
    return response.data;
  }

  async getMyTickets(): Promise<TicketDto[]> {
    const response = await this.client.get<TicketDto[]>('/api/tickets');
    return response.data;
  }

  async addComment(ticketId: string, request: AddCommentRequest): Promise<CommentDto> {
    const response = await this.client.post<CommentDto>(
      `/api/tickets/${ticketId}/comments`,
      request
    );
    return response.data;
  }

  async getComments(ticketId: string): Promise<CommentDto[]> {
    const response = await this.client.get<CommentDto[]>(
      `/api/tickets/${ticketId}/comments`
    );
    return response.data;
  }

  // Agent endpoints
  async getAgentQueue(): Promise<TicketDto[]> {
    const response = await this.client.get<TicketDto[]>('/api/tickets/queue');
    return response.data;
  }

  async assignTicket(ticketId: string, request: AssignTicketRequest): Promise<void> {
    await this.client.put(`/api/tickets/${ticketId}/assign`, request);
  }

  async updateTicketStatus(ticketId: string, request: UpdateTicketStatusRequest): Promise<void> {
    await this.client.put(`/api/tickets/${ticketId}/status`, request);
  }

  async updateTicketPriority(ticketId: string, request: UpdateTicketPriorityRequest): Promise<void> {
    await this.client.put(`/api/tickets/${ticketId}/priority`, request);
  }

  async closeTicket(ticketId: string, request: CloseTicketRequest): Promise<void> {
    await this.client.post(`/api/tickets/${ticketId}/close`, request);
  }

  async reassignTicket(ticketId: string, request: ReassignTicketRequest): Promise<void> {
    await this.client.put(`/api/tickets/${ticketId}/reassign`, request);
  }

  // Category endpoints
  async getAllCategories(): Promise<CategoryDto[]> {
    const response = await this.client.get<CategoryDto[]>('/api/v1/categories');
    return response.data;
  }

  async createCategory(command: CreateCategoryCommand): Promise<CategoryDto> {
    const response = await this.client.post<CategoryDto>('/api/v1/categories', command);
    return response.data;
  }

  // Tag endpoints
  async getAllTags(): Promise<TagDto[]> {
    const response = await this.client.get<TagDto[]>('/api/v1/tags');
    return response.data;
  }

  async addTagsToTicket(ticketId: string, tags: string[]): Promise<void> {
    await this.client.post(`/api/v1/tickets/${ticketId}/tags`, tags);
  }

  async removeTagsFromTicket(ticketId: string, tags: string[]): Promise<void> {
    await this.client.delete(`/api/v1/tickets/${ticketId}/tags`, { data: tags });
  }

  // Search endpoints
  async searchTickets(params: SearchTicketsParams): Promise<SearchTicketsResult> {
    const response = await this.client.get<SearchTicketsResult>('/api/v1/search', { params });
    return response.data;
  }

  // Notification preferences endpoints
  async getNotificationPreferences(): Promise<NotificationPreferencesDto> {
    const response = await this.client.get<NotificationPreferencesDto>('/api/v1/users/me/preferences');
    return response.data;
  }

  async updateNotificationPreferences(
    request: UpdateNotificationPreferencesRequest
  ): Promise<NotificationPreferencesDto> {
    const response = await this.client.put<NotificationPreferencesDto>(
      '/api/v1/users/me/preferences',
      request
    );
    return response.data;
  }

  // Health check
  async healthCheck(): Promise<{ status: string }> {
    const response = await this.client.get('/health');
    return response.data;
  }

  // Knowledge Base endpoints
  async searchArticles(request: SearchArticlesRequest): Promise<SearchArticlesResult> {
    const response = await this.client.get<SearchArticlesResult>('/api/knowledge', {
      params: request,
    });
    return response.data;
  }

  async getArticleById(id: string): Promise<ArticleDto> {
    const response = await this.client.get<ArticleDto>(`/api/knowledge/${id}`);
    return response.data;
  }

  async createArticle(request: CreateArticleRequest): Promise<ArticleDto> {
    const response = await this.client.post<ArticleDto>('/api/knowledge', request);
    return response.data;
  }

  async updateArticle(id: string, request: UpdateArticleRequest): Promise<ArticleDto> {
    const response = await this.client.put<ArticleDto>(`/api/knowledge/${id}`, request);
    return response.data;
  }

  async rateArticle(id: string, request: RateArticleRequest): Promise<void> {
    await this.client.post(`/api/knowledge/${id}/rate`, request);
  }

  async getSuggestedArticles(request: GetSuggestedArticlesRequest): Promise<ArticleListItemDto[]> {
    const response = await this.client.get<ArticleListItemDto[]>('/api/knowledge/suggested', {
      params: request,
    });
    return response.data;
  }
}

export const apiClient = new ApiClient();

/**
 * Extracts a user-friendly error message from an API error
 * @param error - The error object from an API call
 * @returns A formatted error message string
 */
export function handleApiError(error: unknown): string {
  // Type guard for axios error structure
  const isAxiosError = (err: unknown): err is { response?: { data?: { message?: string; errors?: Record<string, unknown> } } } => {
    if (typeof err !== 'object' || err === null || !('response' in err)) {
      return false;
    }
    const response = (err as { response: unknown }).response;
    if (typeof response !== 'object' || response === null) {
      return false;
    }
    return true;
  };

  // Type guard for error with message
  const hasMessage = (err: unknown): err is { message: string } => {
    return typeof err === 'object' && err !== null && 'message' in err && typeof (err as { message: unknown }).message === 'string';
  };

  // Check for response data with message
  if (isAxiosError(error) && error.response?.data?.message) {
    return error.response.data.message;
  }

  // Check for validation errors
  if (isAxiosError(error) && error.response?.data?.errors) {
    const errors = error.response.data.errors;
    const errorMessages = Object.entries(errors)
      .map(([field, messages]) => `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`)
      .join('; ');
    return errorMessages;
  }

  // Check for error message property
  if (hasMessage(error)) {
    return error.message;
  }

  // Default fallback
  return 'An unexpected error occurred';
}
