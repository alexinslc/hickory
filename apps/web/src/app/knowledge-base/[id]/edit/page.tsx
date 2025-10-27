'use client';

import { useParams, useRouter } from 'next/navigation';
import { useArticle, useUpdateArticle, useCreateArticle } from '@/hooks/use-knowledge-base';
import { useAuth } from '@/hooks/use-auth';
import Link from 'next/link';
import { useState, useEffect } from 'react';

export default function ArticleEditorPage() {
  const params = useParams();
  const router = useRouter();
  const { user } = useAuth();
  const articleId = params.id as string;
  const isNewArticle = articleId === 'new';

  // Only fetch if editing existing article
  const shouldFetchArticle = !isNewArticle && articleId;
  const { data: article, isLoading } = useArticle(shouldFetchArticle ? articleId : '');
  
  const updateArticle = useUpdateArticle(articleId);
  const createArticle = useCreateArticle();

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [category, setCategory] = useState('');
  const [tags, setTags] = useState('');
  const [status, setStatus] = useState('Draft');
  const [error, setError] = useState('');

  // Load article data when editing
  useEffect(() => {
    if (article) {
      setTitle(article.title);
      setContent(article.content);
      setCategory(article.category);
      setTags(article.tags.join(', '));
      setStatus(article.status);
    }
  }, [article]);

  // Authorization check
  const isAgent = user?.role === 'Agent' || user?.role === 'Administrator';
  
  if (!isAgent) {
    router.push('/knowledge-base');
    return null;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!title.trim() || !content.trim() || !category.trim()) {
      setError('Please fill in all required fields');
      return;
    }

    const tagArray = tags
      .split(',')
      .map((t) => t.trim())
      .filter((t) => t.length > 0);

    try {
      if (isNewArticle) {
        await createArticle.mutateAsync({
          title: title.trim(),
          content: content.trim(),
          category: category.trim(),
          tags: tagArray.length > 0 ? tagArray : undefined,
          status,
        });
        router.push('/knowledge-base');
      } else {
        await updateArticle.mutateAsync({
          title: title.trim(),
          content: content.trim(),
          category: category.trim(),
          tags: tagArray.length > 0 ? tagArray : undefined,
          status,
        });
        router.push(`/knowledge-base/${articleId}`);
      }
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message || 'Failed to save article');
      } else {
        setError('Failed to save article');
      }
    }
  };

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

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-6">
        <Link
          href={isNewArticle ? '/knowledge-base' : `/knowledge-base/${articleId}`}
          className="text-blue-600 hover:text-blue-700 dark:text-blue-400"
        >
          ← Cancel
        </Link>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-8">
        <h1 className="text-3xl font-bold mb-8 text-gray-900 dark:text-gray-100">
          {isNewArticle ? 'Create Article' : 'Edit Article'}
        </h1>

        {error && (
          <div className="mb-6 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
            <p className="text-red-800 dark:text-red-200">{error}</p>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Title */}
          <div>
            <label htmlFor="title" className="block text-sm font-medium mb-2">
              Title <span className="text-red-500">*</span>
            </label>
            <input
              id="title"
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700"
            />
          </div>

          {/* Category */}
          <div>
            <label htmlFor="category" className="block text-sm font-medium mb-2">
              Category <span className="text-red-500">*</span>
            </label>
            <input
              id="category"
              type="text"
              value={category}
              onChange={(e) => setCategory(e.target.value)}
              required
              placeholder="e.g., Billing, Technical, Account"
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700"
            />
          </div>

          {/* Tags */}
          <div>
            <label htmlFor="tags" className="block text-sm font-medium mb-2">
              Tags (comma-separated)
            </label>
            <input
              id="tags"
              type="text"
              value={tags}
              onChange={(e) => setTags(e.target.value)}
              placeholder="e.g., password, reset, security"
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700"
            />
          </div>

          {/* Content */}
          <div>
            <label htmlFor="content" className="block text-sm font-medium mb-2">
              Content (Markdown) <span className="text-red-500">*</span>
            </label>
            <textarea
              id="content"
              value={content}
              onChange={(e) => setContent(e.target.value)}
              required
              rows={20}
              placeholder="Write your article content using Markdown formatting..."
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 font-mono text-sm"
            />
          </div>

          {/* Status */}
          <div>
            <label htmlFor="status" className="block text-sm font-medium mb-2">
              Status <span className="text-red-500">*</span>
            </label>
            <select
              id="status"
              value={status}
              onChange={(e) => setStatus(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700"
            >
              <option value="Draft">Draft</option>
              <option value="Published">Published</option>
              <option value="Archived">Archived</option>
            </select>
            <p className="mt-2 text-sm text-gray-500 dark:text-gray-400">
              Only published articles are visible to users.
            </p>
          </div>

          {/* Actions */}
          <div className="flex gap-4 pt-4">
            <button
              type="submit"
              disabled={createArticle.isPending || updateArticle.isPending}
              className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-md disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {createArticle.isPending || updateArticle.isPending
                ? 'Saving...'
                : isNewArticle
                ? 'Create Article'
                : 'Save Changes'}
            </button>
            <Link
              href={isNewArticle ? '/knowledge-base' : `/knowledge-base/${articleId}`}
              className="bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-800 dark:text-gray-200 px-6 py-2 rounded-md"
            >
              Cancel
            </Link>
          </div>
        </form>
      </div>
    </div>
  );
}
