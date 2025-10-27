'use client';

import { useParams } from 'next/navigation';
import { useArticle, useRateArticle } from '@/hooks/use-knowledge-base';
import { useAuth } from '@/hooks/use-auth';
import Link from 'next/link';
import { useState } from 'react';

export default function ArticlePage() {
  const params = useParams();
  const { user } = useAuth();
  const articleId = params.id as string;
  const [hasRated, setHasRated] = useState(false);

  const { data: article, isLoading, error } = useArticle(articleId);
  const rateArticle = useRateArticle(articleId);

  const handleRate = async (isHelpful: boolean) => {
    try {
      await rateArticle.mutateAsync({ isHelpful });
      setHasRated(true);
    } catch (error) {
      console.error('Failed to rate article:', error);
    }
  };

  const isAgent = user?.role === 'Agent' || user?.role === 'Administrator';
  const canEdit = isAgent && article;

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center py-12">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <p className="mt-4 text-gray-600 dark:text-gray-400">Loading article...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
          <p className="text-red-800 dark:text-red-200">Error loading article: {error.message}</p>
        </div>
      </div>
    );
  }

  if (!article) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center py-12">
          <p className="text-gray-600 dark:text-gray-400">Article not found.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-6">
        <Link href="/knowledge-base" className="text-blue-600 hover:text-blue-700 dark:text-blue-400">
          ← Back to Knowledge Base
        </Link>
      </div>

      <article className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-8">
        {/* Header */}
        <div className="mb-6 pb-6 border-b border-gray-200 dark:border-gray-700">
          <div className="flex items-start justify-between mb-4">
            <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">{article.title}</h1>
            {canEdit && (
              <Link
                href={`/knowledge-base/${articleId}/edit`}
                className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md text-sm"
              >
                Edit
              </Link>
            )}
          </div>

          <div className="flex items-center gap-4 text-sm text-gray-600 dark:text-gray-400">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300">
              {article.category}
            </span>
            {article.tags.map((tag) => (
              <span
                key={tag}
                className="inline-flex items-center px-2.5 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300"
              >
                {tag}
              </span>
            ))}
            {isAgent && article.status !== 'Published' && (
              <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-300">
                {article.status}
              </span>
            )}
          </div>

          <div className="mt-4 flex items-center gap-6 text-sm text-gray-500 dark:text-gray-400">
            <span>By {article.authorName}</span>
            <span>👁️ {article.viewCount} views</span>
            <span>👍 {article.helpfulCount} helpful</span>
            {article.publishedAt && (
              <span>Published {new Date(article.publishedAt).toLocaleDateString()}</span>
            )}
          </div>
        </div>

        {/* Content */}
        <div className="prose dark:prose-invert max-w-none mb-8">
          {/* For now, render as plain text. In production, use a markdown renderer like react-markdown */}
          <div className="whitespace-pre-wrap">{article.content}</div>
        </div>

        {/* Rating Section */}
        {user && !hasRated && (
          <div className="mt-8 pt-8 border-t border-gray-200 dark:border-gray-700">
            <h2 className="text-lg font-semibold mb-4">Was this article helpful?</h2>
            <div className="flex gap-4">
              <button
                onClick={() => handleRate(true)}
                disabled={rateArticle.isPending}
                className="flex items-center gap-2 px-6 py-3 bg-green-600 hover:bg-green-700 text-white rounded-md disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                👍 Yes, this helped
              </button>
              <button
                onClick={() => handleRate(false)}
                disabled={rateArticle.isPending}
                className="flex items-center gap-2 px-6 py-3 bg-red-600 hover:bg-red-700 text-white rounded-md disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                👎 No, not helpful
              </button>
            </div>
          </div>
        )}

        {hasRated && (
          <div className="mt-8 pt-8 border-t border-gray-200 dark:border-gray-700">
            <p className="text-green-600 dark:text-green-400">✓ Thank you for your feedback!</p>
          </div>
        )}
      </article>
    </div>
  );
}
