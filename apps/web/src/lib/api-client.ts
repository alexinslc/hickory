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

  // Health check
  async healthCheck(): Promise<{ status: string }> {
    const response = await this.client.get('/health');
    return response.data;
  }
}

export const apiClient = new ApiClient();
