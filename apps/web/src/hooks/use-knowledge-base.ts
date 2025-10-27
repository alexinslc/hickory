'use client';

import { useMutation, useQuery, useQueryClient, UseQueryResult } from '@tanstack/react-query';
import {
  apiClient,
  ArticleDto,
  ArticleListItemDto,
  CreateArticleRequest,
  UpdateArticleRequest,
  RateArticleRequest,
  SearchArticlesRequest,
  SearchArticlesResult,
  GetSuggestedArticlesRequest,
} from '@/lib/api-client';

// Query keys
export const knowledgeBaseKeys = {
  all: ['knowledge-base'] as const,
  lists: () => [...knowledgeBaseKeys.all, 'list'] as const,
  list: (filters?: SearchArticlesRequest) => [...knowledgeBaseKeys.lists(), filters] as const,
  details: () => [...knowledgeBaseKeys.all, 'detail'] as const,
  detail: (id: string) => [...knowledgeBaseKeys.details(), id] as const,
  suggested: (request?: GetSuggestedArticlesRequest) => [...knowledgeBaseKeys.all, 'suggested', request] as const,
};

// Search/list articles
export function useArticles(request?: SearchArticlesRequest): UseQueryResult<SearchArticlesResult, Error> {
  return useQuery({
    queryKey: knowledgeBaseKeys.list(request),
    queryFn: () => apiClient.searchArticles(request || {}),
    staleTime: 60000, // 60 seconds - articles don't change as frequently
  });
}

// Get article by ID
export function useArticle(id: string): UseQueryResult<ArticleDto, Error> {
  return useQuery({
    queryKey: knowledgeBaseKeys.detail(id),
    queryFn: () => apiClient.getArticleById(id),
    staleTime: 60000,
    enabled: !!id,
  });
}

// Get suggested articles
export function useSuggestedArticles(
  request?: GetSuggestedArticlesRequest
): UseQueryResult<ArticleListItemDto[], Error> {
  return useQuery({
    queryKey: knowledgeBaseKeys.suggested(request),
    queryFn: () => apiClient.getSuggestedArticles(request || {}),
    staleTime: 30000, // 30 seconds - suggestions can be refreshed more frequently
  });
}

// Create article mutation
export function useCreateArticle() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateArticleRequest) => apiClient.createArticle(request),
    onSuccess: () => {
      // Invalidate articles list
      queryClient.invalidateQueries({ queryKey: knowledgeBaseKeys.lists() });
    },
  });
}

// Update article mutation
export function useUpdateArticle(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateArticleRequest) => apiClient.updateArticle(id, request),
    onSuccess: () => {
      // Invalidate specific article and lists
      queryClient.invalidateQueries({ queryKey: knowledgeBaseKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: knowledgeBaseKeys.lists() });
    },
  });
}

// Rate article mutation
export function useRateArticle(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: RateArticleRequest) => apiClient.rateArticle(id, request),
    onSuccess: () => {
      // Invalidate specific article to refresh rating counts
      queryClient.invalidateQueries({ queryKey: knowledgeBaseKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: knowledgeBaseKeys.lists() });
    },
  });
}
