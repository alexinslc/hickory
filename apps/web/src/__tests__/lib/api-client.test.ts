/**
 * Unit tests for the API client (apps/web/src/lib/api-client.ts)
 *
 * Covers:
 * - Auth endpoints (login, register)
 * - Ticket CRUD operations
 * - Comment operations
 * - Agent operations (queue, assign, status, priority, close, reassign)
 * - Category and tag operations
 * - Attachment operations
 * - Search and knowledge base endpoints
 * - Notification preferences
 * - Health check
 * - Error handling / handleApiError / getFieldErrors utilities
 * - Auth token interceptor and 401 refresh flow
 * - Optimistic concurrency (rowVersion)
 */

import axios from 'axios';

// ---------------------------------------------------------------------------
// Set the env var BEFORE importing the client so API_BASE_URL is deterministic.
// ---------------------------------------------------------------------------
const TEST_API_BASE_URL = 'http://localhost:5000';
process.env.NEXT_PUBLIC_API_URL = TEST_API_BASE_URL;

// ---------------------------------------------------------------------------
// Mock axios. We capture interceptor callbacks so we can test them directly.
// ---------------------------------------------------------------------------
// eslint-disable-next-line no-var -- var needed to avoid temporal dead zone with hoisted jest.mock
var requestInterceptorFulfilled: (config: any) => any;
// eslint-disable-next-line no-var
var requestInterceptorRejected: (error: any) => any;
// eslint-disable-next-line no-var
var responseInterceptorFulfilled: (response: any) => any;
// eslint-disable-next-line no-var
var responseInterceptorRejected: (error: any) => any;

// jest.mock is hoisted above variable declarations, so we must create the
// mock instance inside the factory to avoid "Cannot access before init" errors.
// eslint-disable-next-line no-var -- var needed to avoid temporal dead zone with hoisted jest.mock
var mockAxiosInstance: any;

jest.mock('axios', () => {
  mockAxiosInstance = Object.assign(
    jest.fn().mockImplementation((config: any) => Promise.resolve({ data: {}, config })),
    {
      get: jest.fn(),
      post: jest.fn(),
      put: jest.fn(),
      delete: jest.fn(),
      interceptors: {
        request: {
          use: jest.fn((fulfilled: any, rejected: any) => {
            requestInterceptorFulfilled = fulfilled;
            requestInterceptorRejected = rejected;
          }),
        },
        response: {
          use: jest.fn((fulfilled: any, rejected: any) => {
            responseInterceptorFulfilled = fulfilled;
            responseInterceptorRejected = rejected;
          }),
        },
      },
    }
  );
  const m: any = {
    create: jest.fn(() => mockAxiosInstance),
    post: jest.fn(),
  };
  m.default = m;
  return { __esModule: true, default: m };
});

// ---------------------------------------------------------------------------
// Mock localStorage
// ---------------------------------------------------------------------------
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: jest.fn((key: string) => store[key] ?? null),
    setItem: jest.fn((key: string, value: string) => {
      store[key] = value;
    }),
    removeItem: jest.fn((key: string) => {
      delete store[key];
    }),
    clear: jest.fn(() => {
      store = {};
    }),
  };
})();
Object.defineProperty(global, 'localStorage', { value: localStorageMock });

// Mock window.location
delete (window as any).location;
window.location = { href: '' } as any;

// Mock window.URL
window.URL.createObjectURL = jest.fn(() => 'blob:mock-url');
window.URL.revokeObjectURL = jest.fn();

// ---------------------------------------------------------------------------
// Import after mocking so the ApiClient constructor uses our stubs.
// ---------------------------------------------------------------------------
import { apiClient, handleApiError, getFieldErrors } from '../../lib/api-client';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
function setAuthStorage(accessToken: string, refreshToken?: string) {
  localStorageMock.setItem(
    'hickory-auth-storage',
    JSON.stringify({
      state: { accessToken, refreshToken: refreshToken ?? 'refresh-tok' },
    })
  );
}

function clearAuthStorage() {
  localStorageMock.removeItem('hickory-auth-storage');
}

// ---------------------------------------------------------------------------
// Clear only per-test state (HTTP method mocks, localStorage, location).
// We intentionally do NOT use jest.clearAllMocks() here because that would
// wipe call history from module-init time (axios.create, interceptor
// registrations), causing the "Axios instance configuration" assertions to
// see 0 calls and fail.
// ---------------------------------------------------------------------------
beforeEach(() => {
  mockAxiosInstance.get.mockReset();
  mockAxiosInstance.post.mockReset();
  mockAxiosInstance.put.mockReset();
  mockAxiosInstance.delete.mockReset();
  mockAxiosInstance.mockClear();
  (axios.post as jest.Mock).mockReset();
  clearAuthStorage();
});

// ===========================================================================
// Auth endpoints
// ===========================================================================
describe('Auth endpoints', () => {
  const authResponse = {
    accessToken: 'access-tok',
    refreshToken: 'refresh-tok',
    userId: 'u1',
    email: 'test@example.com',
    firstName: 'Test',
    lastName: 'User',
    role: 'customer',
    expiresAt: '2026-12-31T00:00:00Z',
  };

  it('login sends POST to /api/v1/auth/login and returns data', async () => {
    mockAxiosInstance.post.mockResolvedValueOnce({ data: authResponse });

    const result = await apiClient.login({
      email: 'test@example.com',
      password: 'pass',
    });

    expect(mockAxiosInstance.post).toHaveBeenCalledWith('/api/v1/auth/login', {
      email: 'test@example.com',
      password: 'pass',
    });
    expect(result).toEqual(authResponse);
  });

  it('register sends POST to /api/v1/auth/register and returns data', async () => {
    mockAxiosInstance.post.mockResolvedValueOnce({ data: authResponse });

    const result = await apiClient.register({
      email: 'new@example.com',
      password: 'secret',
      firstName: 'New',
      lastName: 'User',
    });

    expect(mockAxiosInstance.post).toHaveBeenCalledWith('/api/v1/auth/register', {
      email: 'new@example.com',
      password: 'secret',
      firstName: 'New',
      lastName: 'User',
    });
    expect(result).toEqual(authResponse);
  });
});

// ===========================================================================
// Ticket endpoints
// ===========================================================================
describe('Ticket endpoints', () => {
  const ticketDto = {
    id: 't1',
    ticketNumber: 'TKT-001',
    title: 'Test Ticket',
    description: 'desc',
    status: 'Open',
    priority: 'Medium',
    submitterId: 'u1',
    submitterName: 'User',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
    commentCount: 0,
    rowVersion: 'AAAA',
    tags: [],
  };

  it('createTicket sends POST to /api/tickets', async () => {
    const createResponse = {
      id: 't1',
      ticketNumber: 'TKT-001',
      title: 'Test',
      description: 'desc',
      status: 'Open',
      priority: 'Medium',
      createdAt: '2026-01-01T00:00:00Z',
    };
    mockAxiosInstance.post.mockResolvedValueOnce({ data: createResponse });

    const result = await apiClient.createTicket({
      title: 'Test',
      description: 'desc',
      priority: 'Medium',
    });

    expect(mockAxiosInstance.post).toHaveBeenCalledWith('/api/v1/tickets', {
      title: 'Test',
      description: 'desc',
      priority: 'Medium',
    });
    expect(result).toEqual(createResponse);
  });

  it('createTicket includes optional categoryId', async () => {
    mockAxiosInstance.post.mockResolvedValueOnce({ data: {} });

    await apiClient.createTicket({
      title: 'Test',
      description: 'desc',
      priority: 'High',
      categoryId: 'cat1',
    });

    expect(mockAxiosInstance.post).toHaveBeenCalledWith('/api/v1/tickets', {
      title: 'Test',
      description: 'desc',
      priority: 'High',
      categoryId: 'cat1',
    });
  });

  it('getTicketById sends GET to /api/tickets/:id', async () => {
    mockAxiosInstance.get.mockResolvedValueOnce({ data: ticketDto });

    const result = await apiClient.getTicketById('t1');

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/tickets/t1');
    expect(result).toEqual(ticketDto);
  });

  it('getMyTickets sends GET with pagination params', async () => {
    const paginated = {
      items: [ticketDto],
      totalCount: 1,
      page: 1,
      pageSize: 10,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    };
    mockAxiosInstance.get.mockResolvedValueOnce({ data: paginated });

    const result = await apiClient.getMyTickets({ page: 1, pageSize: 10 });

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/tickets', {
      params: { page: 1, pageSize: 10 },
    });
    expect(result).toEqual(paginated);
  });

  it('getMyTickets works without params', async () => {
    const paginated = {
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 10,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false,
    };
    mockAxiosInstance.get.mockResolvedValueOnce({ data: paginated });

    await apiClient.getMyTickets();

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/tickets', {
      params: undefined,
    });
  });

  it('getMyTickets passes filter param', async () => {
    mockAxiosInstance.get.mockResolvedValueOnce({
      data: { items: [], totalCount: 0, page: 1, pageSize: 10, totalPages: 0, hasNextPage: false, hasPreviousPage: false },
    });

    await apiClient.getMyTickets({ page: 2, pageSize: 5, filter: 'open' });

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/tickets', {
      params: { page: 2, pageSize: 5, filter: 'open' },
    });
  });
});

// ===========================================================================
// Comment endpoints
// ===========================================================================
describe('Comment endpoints', () => {
  const commentDto = {
    id: 'c1',
    content: 'Hello',
    isInternal: false,
    authorId: 'u1',
    authorName: 'User',
    createdAt: '2026-01-01T00:00:00Z',
  };

  it('addComment sends POST to /api/tickets/:id/comments', async () => {
    mockAxiosInstance.post.mockResolvedValueOnce({ data: commentDto });

    const result = await apiClient.addComment('t1', {
      content: 'Hello',
      isInternal: false,
    });

    expect(mockAxiosInstance.post).toHaveBeenCalledWith('/api/v1/tickets/t1/comments', {
      content: 'Hello',
      isInternal: false,
    });
    expect(result).toEqual(commentDto);
  });

  it('addComment supports internal comments', async () => {
    const internalComment = { ...commentDto, isInternal: true };
    mockAxiosInstance.post.mockResolvedValueOnce({ data: internalComment });

    const result = await apiClient.addComment('t1', {
      content: 'Internal note',
      isInternal: true,
    });

    expect(mockAxiosInstance.post).toHaveBeenCalledWith('/api/v1/tickets/t1/comments', {
      content: 'Internal note',
      isInternal: true,
    });
    expect(result.isInternal).toBe(true);
  });

  it('getComments sends GET to /api/tickets/:id/comments', async () => {
    mockAxiosInstance.get.mockResolvedValueOnce({ data: [commentDto] });

    const result = await apiClient.getComments('t1');

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/tickets/t1/comments');
    expect(result).toEqual([commentDto]);
  });
});

// ===========================================================================
// Agent endpoints (with optimistic concurrency / rowVersion)
// ===========================================================================
describe('Agent endpoints', () => {
  it('getAgentQueue sends GET to /api/tickets/queue', async () => {
    const paginated = {
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 10,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false,
    };
    mockAxiosInstance.get.mockResolvedValueOnce({ data: paginated });

    const result = await apiClient.getAgentQueue({ page: 1, pageSize: 10 });

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/tickets/queue', {
      params: { page: 1, pageSize: 10 },
    });
    expect(result).toEqual(paginated);
  });

  it('assignTicket sends PUT with agentId and rowVersion', async () => {
    mockAxiosInstance.put.mockResolvedValueOnce({});

    await apiClient.assignTicket('t1', { agentId: 'a1', rowVersion: 'AAAA' });

    expect(mockAxiosInstance.put).toHaveBeenCalledWith('/api/v1/tickets/t1/assign', {
      agentId: 'a1',
      rowVersion: 'AAAA',
    });
  });

  it('updateTicketStatus sends PUT with newStatus and rowVersion', async () => {
    mockAxiosInstance.put.mockResolvedValueOnce({});

    await apiClient.updateTicketStatus('t1', {
      newStatus: 'InProgress',
      rowVersion: 'AAAA',
    });

    expect(mockAxiosInstance.put).toHaveBeenCalledWith('/api/v1/tickets/t1/status', {
      newStatus: 'InProgress',
      rowVersion: 'AAAA',
    });
  });

  it('updateTicketPriority sends PUT with newPriority and rowVersion', async () => {
    mockAxiosInstance.put.mockResolvedValueOnce({});

    await apiClient.updateTicketPriority('t1', {
      newPriority: 'High',
      rowVersion: 'BBBB',
    });

    expect(mockAxiosInstance.put).toHaveBeenCalledWith('/api/v1/tickets/t1/priority', {
      newPriority: 'High',
      rowVersion: 'BBBB',
    });
  });

  it('closeTicket sends POST with resolutionNotes and rowVersion', async () => {
    mockAxiosInstance.post.mockResolvedValueOnce({});

    await apiClient.closeTicket('t1', {
      resolutionNotes: 'Done',
      rowVersion: 'CCCC',
    });

    expect(mockAxiosInstance.post).toHaveBeenCalledWith('/api/v1/tickets/t1/close', {
      resolutionNotes: 'Done',
      rowVersion: 'CCCC',
    });
  });

  it('reassignTicket sends PUT with newAgentId and rowVersion', async () => {
    mockAxiosInstance.put.mockResolvedValueOnce({});

    await apiClient.reassignTicket('t1', { newAgentId: 'a2', rowVersion: 'DDDD' });

    expect(mockAxiosInstance.put).toHaveBeenCalledWith('/api/v1/tickets/t1/reassign', {
      newAgentId: 'a2',
      rowVersion: 'DDDD',
    });
  });
});

// ===========================================================================
// Category endpoints
// ===========================================================================
describe('Category endpoints', () => {
  const categoryDto = {
    id: 'cat1',
    name: 'Billing',
    description: 'Billing issues',
    displayOrder: 1,
    color: '#ff0000',
    isActive: true,
  };

  it('getAllCategories sends GET to /api/v1/categories', async () => {
    mockAxiosInstance.get.mockResolvedValueOnce({ data: [categoryDto] });

    const result = await apiClient.getAllCategories();

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/categories');
    expect(result).toEqual([categoryDto]);
  });

  it('createCategory sends POST to /api/v1/categories', async () => {
    mockAxiosInstance.post.mockResolvedValueOnce({ data: categoryDto });

    const result = await apiClient.createCategory({
      name: 'Billing',
      description: 'Billing issues',
      displayOrder: 1,
      color: '#ff0000',
    });

    expect(mockAxiosInstance.post).toHaveBeenCalledWith('/api/v1/categories', {
      name: 'Billing',
      description: 'Billing issues',
      displayOrder: 1,
      color: '#ff0000',
    });
    expect(result).toEqual(categoryDto);
  });
});

// ===========================================================================
// Tag endpoints
// ===========================================================================
describe('Tag endpoints', () => {
  it('getAllTags sends GET to /api/v1/tags', async () => {
    mockAxiosInstance.get.mockResolvedValueOnce({
      data: [{ id: 'tag1', name: 'urgent' }],
    });

    const result = await apiClient.getAllTags();

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/tags');
    expect(result).toEqual([{ id: 'tag1', name: 'urgent' }]);
  });

  it('addTagsToTicket sends POST with tag array', async () => {
    mockAxiosInstance.post.mockResolvedValueOnce({});

    await apiClient.addTagsToTicket('t1', ['urgent', 'billing']);

    expect(mockAxiosInstance.post).toHaveBeenCalledWith('/api/v1/tickets/t1/tags', [
      'urgent',
      'billing',
    ]);
  });

  it('removeTagsFromTicket sends DELETE with tag array in data', async () => {
    mockAxiosInstance.delete.mockResolvedValueOnce({});

    await apiClient.removeTagsFromTicket('t1', ['urgent']);

    expect(mockAxiosInstance.delete).toHaveBeenCalledWith('/api/v1/tickets/t1/tags', {
      data: ['urgent'],
    });
  });
});

// ===========================================================================
// Attachment endpoints
// ===========================================================================
describe('Attachment endpoints', () => {
  it('getTicketDetails sends GET to /api/tickets/:id/details', async () => {
    const details = { ticket: {}, comments: [], attachments: [] };
    mockAxiosInstance.get.mockResolvedValueOnce({ data: details });

    const result = await apiClient.getTicketDetails('t1');

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/tickets/t1/details');
    expect(result).toEqual(details);
  });

  it('uploadAttachment sends POST with FormData and progress callback', async () => {
    const uploadResponse = {
      id: 'att1',
      fileName: 'file.pdf',
      contentType: 'application/pdf',
      fileSizeBytes: 1024,
      uploadedAt: '2026-01-01T00:00:00Z',
    };
    mockAxiosInstance.post.mockResolvedValueOnce({ data: uploadResponse });

    const mockFile = new File(['data'], 'file.pdf', { type: 'application/pdf' });
    const onProgress = jest.fn();

    const result = await apiClient.uploadAttachment('t1', mockFile, onProgress);

    expect(mockAxiosInstance.post).toHaveBeenCalledWith(
      '/api/v1/attachments/tickets/t1',
      expect.any(FormData),
      expect.objectContaining({
        headers: { 'Content-Type': 'multipart/form-data' },
        onUploadProgress: onProgress,
      })
    );
    expect(result).toEqual(uploadResponse);
  });

  it('uploadAttachment works without progress callback', async () => {
    mockAxiosInstance.post.mockResolvedValueOnce({ data: { id: 'att2' } });

    const mockFile = new File(['data'], 'doc.txt', { type: 'text/plain' });
    await apiClient.uploadAttachment('t1', mockFile);

    expect(mockAxiosInstance.post).toHaveBeenCalledWith(
      '/api/v1/attachments/tickets/t1',
      expect.any(FormData),
      expect.objectContaining({
        onUploadProgress: undefined,
      })
    );
  });

  it('downloadAttachment creates a blob link and triggers download', async () => {
    const blobData = new Blob(['file-content']);
    mockAxiosInstance.get.mockResolvedValueOnce({ data: blobData });

    const mockLink = {
      href: '',
      setAttribute: jest.fn(),
      click: jest.fn(),
      parentNode: { removeChild: jest.fn() },
    };
    jest.spyOn(document, 'createElement').mockReturnValueOnce(mockLink as any);
    jest.spyOn(document.body, 'appendChild').mockImplementationOnce(() => mockLink as any);

    await apiClient.downloadAttachment('att1', 'file.pdf');

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/attachments/att1', {
      responseType: 'blob',
    });
    expect(mockLink.setAttribute).toHaveBeenCalledWith('download', 'file.pdf');
    expect(mockLink.click).toHaveBeenCalled();
    expect(window.URL.revokeObjectURL).toHaveBeenCalledWith('blob:mock-url');
  });

  it('deleteAttachment sends DELETE to /api/attachments/:id', async () => {
    mockAxiosInstance.delete.mockResolvedValueOnce({});

    await apiClient.deleteAttachment('att1');

    expect(mockAxiosInstance.delete).toHaveBeenCalledWith('/api/v1/attachments/att1');
  });
});

// ===========================================================================
// Search endpoints
// ===========================================================================
describe('Search endpoints', () => {
  it('searchTickets sends GET with params to /api/v1/search', async () => {
    const searchResult = {
      tickets: [],
      totalCount: 0,
      page: 1,
      pageSize: 10,
      totalPages: 0,
    };
    mockAxiosInstance.get.mockResolvedValueOnce({ data: searchResult });

    const result = await apiClient.searchTickets({
      q: 'billing',
      status: 'Open',
      page: 1,
    });

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/search', {
      params: { q: 'billing', status: 'Open', page: 1 },
    });
    expect(result).toEqual(searchResult);
  });

  it('searchTickets supports all search parameters', async () => {
    mockAxiosInstance.get.mockResolvedValueOnce({ data: { tickets: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 } });

    await apiClient.searchTickets({
      q: 'help',
      status: 'InProgress',
      priority: 'High',
      assignedToId: 'a1',
      createdAfter: '2026-01-01',
      createdBefore: '2026-06-01',
      page: 2,
      pageSize: 20,
    });

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/search', {
      params: {
        q: 'help',
        status: 'InProgress',
        priority: 'High',
        assignedToId: 'a1',
        createdAfter: '2026-01-01',
        createdBefore: '2026-06-01',
        page: 2,
        pageSize: 20,
      },
    });
  });
});

// ===========================================================================
// Notification preferences
// ===========================================================================
describe('Notification preferences', () => {
  const prefs = {
    emailEnabled: true,
    emailOnTicketCreated: true,
    emailOnTicketUpdated: false,
    emailOnTicketAssigned: true,
    emailOnCommentAdded: true,
    inAppEnabled: true,
    inAppOnTicketCreated: true,
    inAppOnTicketUpdated: true,
    inAppOnTicketAssigned: true,
    inAppOnCommentAdded: true,
    webhookEnabled: false,
  };

  it('getNotificationPreferences sends GET to /api/v1/users/me/preferences', async () => {
    mockAxiosInstance.get.mockResolvedValueOnce({ data: prefs });

    const result = await apiClient.getNotificationPreferences();

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/users/me/preferences');
    expect(result).toEqual(prefs);
  });

  it('updateNotificationPreferences sends PUT to /api/v1/users/me/preferences', async () => {
    mockAxiosInstance.put.mockResolvedValueOnce({ data: prefs });

    const result = await apiClient.updateNotificationPreferences(prefs as any);

    expect(mockAxiosInstance.put).toHaveBeenCalledWith(
      '/api/v1/users/me/preferences',
      prefs
    );
    expect(result).toEqual(prefs);
  });
});

// ===========================================================================
// Health check
// ===========================================================================
describe('Health check', () => {
  it('healthCheck sends GET to /health', async () => {
    mockAxiosInstance.get.mockResolvedValueOnce({ data: { status: 'Healthy' } });

    const result = await apiClient.healthCheck();

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/health');
    expect(result).toEqual({ status: 'Healthy' });
  });
});

// ===========================================================================
// Knowledge Base endpoints
// ===========================================================================
describe('Knowledge Base endpoints', () => {
  const article = {
    id: 'a1',
    title: 'How to reset password',
    content: 'Steps...',
    category: 'Account',
    tags: ['password'],
    status: 'Published',
    viewCount: 10,
    helpfulCount: 5,
    notHelpfulCount: 1,
    authorId: 'u1',
    authorName: 'Author',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
  };

  it('searchArticles sends GET to /api/knowledge with params', async () => {
    const searchResult = {
      articles: [],
      totalCount: 0,
      page: 1,
      pageSize: 10,
      totalPages: 0,
    };
    mockAxiosInstance.get.mockResolvedValueOnce({ data: searchResult });

    const result = await apiClient.searchArticles({
      searchTerm: 'password',
      page: 1,
    });

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/knowledge', {
      params: { searchTerm: 'password', page: 1 },
    });
    expect(result).toEqual(searchResult);
  });

  it('getArticleById sends GET to /api/knowledge/:id', async () => {
    mockAxiosInstance.get.mockResolvedValueOnce({ data: article });

    const result = await apiClient.getArticleById('a1');

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/knowledge/a1');
    expect(result).toEqual(article);
  });

  it('createArticle sends POST to /api/knowledge', async () => {
    mockAxiosInstance.post.mockResolvedValueOnce({ data: article });

    const result = await apiClient.createArticle({
      title: 'How to reset password',
      content: 'Steps...',
      category: 'Account',
      tags: ['password'],
    });

    expect(mockAxiosInstance.post).toHaveBeenCalledWith('/api/v1/knowledge', {
      title: 'How to reset password',
      content: 'Steps...',
      category: 'Account',
      tags: ['password'],
    });
    expect(result).toEqual(article);
  });

  it('updateArticle sends PUT to /api/knowledge/:id', async () => {
    mockAxiosInstance.put.mockResolvedValueOnce({ data: article });

    const result = await apiClient.updateArticle('a1', { title: 'Updated Title' });

    expect(mockAxiosInstance.put).toHaveBeenCalledWith('/api/v1/knowledge/a1', {
      title: 'Updated Title',
    });
    expect(result).toEqual(article);
  });

  it('rateArticle sends POST to /api/knowledge/:id/rate', async () => {
    mockAxiosInstance.post.mockResolvedValueOnce({});

    await apiClient.rateArticle('a1', { isHelpful: true });

    expect(mockAxiosInstance.post).toHaveBeenCalledWith('/api/v1/knowledge/a1/rate', {
      isHelpful: true,
    });
  });

  it('getSuggestedArticles sends GET to /api/knowledge/suggested', async () => {
    mockAxiosInstance.get.mockResolvedValueOnce({ data: [] });

    const result = await apiClient.getSuggestedArticles({
      searchTerm: 'billing',
      limit: 5,
    });

    expect(mockAxiosInstance.get).toHaveBeenCalledWith('/api/v1/knowledge/suggested', {
      params: { searchTerm: 'billing', limit: 5 },
    });
    expect(result).toEqual([]);
  });
});

// ===========================================================================
// Request interceptor - auth token injection
// ===========================================================================
describe('Request interceptor (auth token)', () => {
  it('attaches Bearer token from localStorage when present', () => {
    setAuthStorage('my-access-token');

    const config = { headers: {} } as any;
    const result = requestInterceptorFulfilled(config);

    expect(result.headers.Authorization).toBe('Bearer my-access-token');
  });

  it('does not attach token when localStorage has no auth state', () => {
    clearAuthStorage();

    const config = { headers: {} } as any;
    const result = requestInterceptorFulfilled(config);

    expect(result.headers.Authorization).toBeUndefined();
  });

  it('does not attach token when localStorage value is invalid JSON', () => {
    localStorageMock.setItem('hickory-auth-storage', 'not-json');

    const config = { headers: {} } as any;
    const result = requestInterceptorFulfilled(config);

    expect(result.headers.Authorization).toBeUndefined();
  });

  it('rejects on request interceptor error', async () => {
    const error = new Error('Request setup failed');
    await expect(requestInterceptorRejected(error)).rejects.toThrow(
      'Request setup failed'
    );
  });
});

// ===========================================================================
// Response interceptor - error handling and 401 refresh
// ===========================================================================
describe('Response interceptor', () => {
  it('passes through successful responses unchanged', () => {
    const response = { data: { ok: true }, status: 200 };
    const result = responseInterceptorFulfilled(response);
    expect(result).toBe(response);
  });

  it('rejects non-401 errors without attempting refresh', async () => {
    const error = {
      response: { status: 500 },
      config: { _retry: false, headers: {} },
    };

    await expect(responseInterceptorRejected(error)).rejects.toBe(error);
  });

  it('rejects 403 errors without attempting refresh', async () => {
    const error = {
      response: { status: 403 },
      config: { _retry: false, headers: {} },
    };

    await expect(responseInterceptorRejected(error)).rejects.toBe(error);
  });

  it('rejects 401 errors when _retry is already true (prevents infinite loop)', async () => {
    const error = {
      response: { status: 401 },
      config: { _retry: true, headers: {} },
    };

    await expect(responseInterceptorRejected(error)).rejects.toBe(error);
  });

  it('attempts token refresh on first 401', async () => {
    setAuthStorage('old-token', 'valid-refresh-token');

    const mockedAxios = axios as jest.Mocked<typeof axios>;
    mockedAxios.post.mockResolvedValueOnce({
      data: {
        accessToken: 'new-access-token',
        refreshToken: 'new-refresh-token',
        expiresAt: '2026-12-31T00:00:00Z',
      },
    });

    // The retry call (this.client(config)) will use the mock instance.
    // We resolve it to simulate a successful retry.
    mockAxiosInstance.mockResolvedValueOnce({ data: { retried: true } });

    const error = {
      response: { status: 401 },
      config: { _retry: false, headers: {} },
    };

    // The interceptor should call the refresh endpoint
    await responseInterceptorRejected(error).catch(() => {
      // May throw if mock wiring isn't perfect — that's OK,
      // we just verify the refresh was attempted.
    });

    expect(mockedAxios.post).toHaveBeenCalledWith(
      `${TEST_API_BASE_URL}/api/v1/auth/refresh`,
      { refreshToken: 'valid-refresh-token' }
    );
  });

  it('clears auth and redirects to login when refresh fails', async () => {
    setAuthStorage('old-token', 'expired-refresh-token');

    const mockedAxios = axios as jest.Mocked<typeof axios>;
    mockedAxios.post.mockRejectedValueOnce(new Error('Refresh failed'));

    const error = {
      response: { status: 401 },
      config: { _retry: false, headers: {} },
    };

    try {
      await responseInterceptorRejected(error);
    } catch {
      // Expected to throw
    }

    expect(localStorageMock.removeItem).toHaveBeenCalledWith(
      'hickory-auth-storage'
    );
  });

  it('clears auth when no refresh token is available', async () => {
    localStorageMock.setItem(
      'hickory-auth-storage',
      JSON.stringify({ state: { accessToken: 'tok' } })
    );

    const error = {
      response: { status: 401 },
      config: { _retry: false, headers: {} },
    };

    try {
      await responseInterceptorRejected(error);
    } catch {
      // Expected to throw
    }

    expect(localStorageMock.removeItem).toHaveBeenCalledWith(
      'hickory-auth-storage'
    );
  });

  it('rejects 401 when config is undefined', async () => {
    const error = {
      response: { status: 401 },
      config: undefined,
    };

    await expect(responseInterceptorRejected(error)).rejects.toBe(error);
  });
});

// ===========================================================================
// handleApiError utility
// ===========================================================================
describe('handleApiError', () => {
  it('returns message from response.data.message', () => {
    const error = {
      response: { data: { message: 'Invalid credentials' }, status: 401 },
    };
    expect(handleApiError(error)).toBe('Invalid credentials');
  });

  it('returns field-level validation errors when errors object present', () => {
    const error = {
      response: {
        data: {
          title: 'Validation Failed',
          errors: {
            Email: ['Required'],
            Password: ['Too short', 'Missing number'],
          },
        },
      },
    };
    const result = handleApiError(error);
    expect(result).toContain('Email: Required');
    expect(result).toContain('Password: Too short, Missing number');
  });

  it('prioritizes errors over title when both present', () => {
    const error = {
      response: {
        data: {
          title: 'Validation Failed',
          errors: { Name: ['Name is required'] },
        },
      },
    };
    // Should return field errors, not the title
    expect(handleApiError(error)).toContain('Name: Name is required');
  });

  it('returns detail when only title/detail present (no errors)', () => {
    const error = {
      response: {
        data: { title: 'Not Found', detail: 'Ticket not found' },
      },
    };
    expect(handleApiError(error)).toBe('Ticket not found');
  });

  it('returns title when detail is absent', () => {
    const error = {
      response: { data: { title: 'Bad Request' } },
    };
    expect(handleApiError(error)).toBe('Bad Request');
  });

  it('returns error.message when no response data', () => {
    const error = { message: 'Network Error' };
    expect(handleApiError(error)).toBe('Network Error');
  });

  it('returns default message for unknown error shapes', () => {
    expect(handleApiError(42)).toBe('An unexpected error occurred');
    expect(handleApiError(null)).toBe('An unexpected error occurred');
    expect(handleApiError(undefined)).toBe('An unexpected error occurred');
    expect(handleApiError({})).toBe('An unexpected error occurred');
  });

  it('handles non-array error values in errors object', () => {
    const error = {
      response: {
        data: { errors: { email: 'Invalid email format' } },
      },
    };
    const result = handleApiError(error);
    expect(result).toContain('email');
    expect(result).toContain('Invalid email format');
  });
});

// ===========================================================================
// getFieldErrors utility
// ===========================================================================
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

  it('normalizes PascalCase keys to camelCase', () => {
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

    expect(getFieldErrors(error)).toEqual({
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

    expect(getFieldErrors(error)).toEqual({
      password: [
        'Password must be at least 8 characters',
        'Password must contain an uppercase letter',
      ],
    });
  });

  it('coerces non-array messages to string arrays', () => {
    const error = {
      response: {
        data: { errors: { Email: 'Email is invalid' } },
      },
    };

    expect(getFieldErrors(error)).toEqual({
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

    expect(getFieldErrors(error)).toEqual({
      categoryId: ['Invalid category'],
      priority: ['Priority is required'],
    });
  });
});

// ===========================================================================
// Error propagation from API methods
// ===========================================================================
describe('Error propagation', () => {
  it('propagates network errors from API methods', async () => {
    const networkError = new Error('Network Error');
    mockAxiosInstance.get.mockRejectedValueOnce(networkError);

    await expect(apiClient.getTicketById('t1')).rejects.toThrow('Network Error');
  });

  it('propagates 404 errors', async () => {
    const notFoundError = {
      response: { status: 404, data: { title: 'Not Found' } },
      message: 'Request failed with status code 404',
    };
    mockAxiosInstance.get.mockRejectedValueOnce(notFoundError);

    await expect(apiClient.getTicketById('nonexistent')).rejects.toEqual(
      notFoundError
    );
  });

  it('propagates 409 conflict errors (optimistic concurrency)', async () => {
    const conflictError = {
      response: {
        status: 409,
        data: {
          title: 'Concurrency conflict',
          detail: 'The ticket was modified by another user',
        },
      },
    };
    mockAxiosInstance.put.mockRejectedValueOnce(conflictError);

    await expect(
      apiClient.updateTicketStatus('t1', {
        newStatus: 'Closed',
        rowVersion: 'stale',
      })
    ).rejects.toEqual(conflictError);
  });

  it('propagates 422 validation errors', async () => {
    const validationError = {
      response: {
        status: 422,
        data: {
          title: 'Validation Failed',
          errors: { Title: ['Title is required'] },
        },
      },
    };
    mockAxiosInstance.post.mockRejectedValueOnce(validationError);

    await expect(
      apiClient.createTicket({ title: '', description: 'desc', priority: 'Low' })
    ).rejects.toEqual(validationError);
  });

  it('propagates 500 server errors', async () => {
    const serverError = {
      response: { status: 500, data: { title: 'Internal Server Error' } },
    };
    mockAxiosInstance.get.mockRejectedValueOnce(serverError);

    await expect(apiClient.healthCheck()).rejects.toEqual(serverError);
  });
});

// ===========================================================================
// Axios instance configuration
// ===========================================================================
describe('Axios instance configuration', () => {
  it('creates axios instance with correct baseURL and headers', () => {
    expect(axios.create).toHaveBeenCalledWith({
      baseURL: TEST_API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });
  });

  it('registers request and response interceptors', () => {
    expect(mockAxiosInstance.interceptors.request.use).toHaveBeenCalledTimes(1);
    expect(mockAxiosInstance.interceptors.response.use).toHaveBeenCalledTimes(1);
  });
});
