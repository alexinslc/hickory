'use client';

import { useParams, useRouter } from 'next/navigation';
import { useArticle, useUpdateArticle, useCreateArticle } from '@/hooks/use-knowledge-base';
import { useAuth } from '@/hooks/use-auth';
import { getFieldErrors } from '@/lib/api-client';
import Link from 'next/link';
import { useState, useEffect, useCallback, useMemo } from 'react';
import { AlertCircle } from 'lucide-react';

export default function ArticleEditorPage() {
  const params = useParams();
  const router = useRouter();
  const { user } = useAuth();
  const articleId = params.id as string;
  const isNewArticle = articleId === 'new';

  // Only fetch if editing existing article (useArticle has enabled check built-in)
  const { data: article, isLoading } = useArticle(articleId);

  const updateArticle = useUpdateArticle(articleId);
  const createArticle = useCreateArticle();

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [category, setCategory] = useState('');
  const [tags, setTags] = useState('');
  const [status, setStatus] = useState('Draft');
  const [error, setError] = useState('');
  const [touched, setTouched] = useState({
    title: false,
    content: false,
    category: false,
  });

  const handleBlur = useCallback((field: keyof typeof touched) => {
    setTouched((prev) => ({ ...prev, [field]: true }));
  }, []);

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

  // Client-side validation
  const titleError = useMemo(() => {
    if (!touched.title) return null;
    if (title.length === 0 || title.trim().length < 1) return 'Title is required';
    if (title.length > 500) return 'Title must be no more than 500 characters';
    return null;
  }, [title, touched.title]);

  const contentError = useMemo(() => {
    if (!touched.content) return null;
    if (content.length === 0 || content.trim().length < 1) return 'Content is required';
    return null;
  }, [content, touched.content]);

  const categoryError = useMemo(() => {
    if (!touched.category) return null;
    if (category.length === 0 || category.trim().length < 1) return 'Category is required';
    return null;
  }, [category, touched.category]);

  // Server-side field errors
  const mutationError = isNewArticle ? createArticle.error : updateArticle.error;
  const isMutationError = isNewArticle ? createArticle.isError : updateArticle.isError;
  const serverFieldErrors = isMutationError ? getFieldErrors(mutationError) : null;

  const serverTitleError = serverFieldErrors?.title?.[0] ?? null;
  const serverContentError = serverFieldErrors?.content?.[0] ?? null;
  const serverCategoryError = serverFieldErrors?.category?.[0] ?? null;

  const displayTitleError = titleError || serverTitleError;
  const displayContentError = contentError || serverContentError;
  const displayCategoryError = categoryError || serverCategoryError;

  const isValid = title.trim().length > 0 && content.trim().length > 0 && category.trim().length > 0;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setTouched({ title: true, content: true, category: true });

    if (!isValid) {
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
      // Only set general error if there are no field-specific errors
      const fieldErrors = getFieldErrors(err);
      if (!fieldErrors) {
        if (err instanceof Error) {
          setError(err.message || 'Failed to save article');
        } else {
          setError('Failed to save article');
        }
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
          &larr; Cancel
        </Link>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-8">
        <h1 className="text-3xl font-bold mb-8 text-gray-900 dark:text-gray-100">
          {isNewArticle ? 'Create Article' : 'Edit Article'}
        </h1>

        {error && (
          <div className="mb-6 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4" role="alert">
            <div className="flex items-start gap-2">
              <AlertCircle className="h-5 w-5 text-red-500 mt-0.5 flex-shrink-0" aria-hidden="true" />
              <p className="text-red-800 dark:text-red-200">{error}</p>
            </div>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6" noValidate aria-busy={createArticle.isPending || updateArticle.isPending || undefined}>
          {/* Title */}
          <div>
            <label htmlFor="title" className="block text-sm font-medium mb-2">
              Title <span className="text-red-500" aria-hidden="true">*</span>
            </label>
            <input
              id="title"
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              onBlur={() => handleBlur('title')}
              required
              disabled={createArticle.isPending || updateArticle.isPending}
              aria-required="true"
              aria-invalid={!!displayTitleError || undefined}
              aria-describedby={displayTitleError ? 'article-title-error' : undefined}
              className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 dark:bg-gray-700 disabled:bg-gray-100 disabled:cursor-not-allowed ${
                displayTitleError
                  ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
                  : 'border-gray-300 dark:border-gray-600 focus:ring-blue-500'
              }`}
            />
            {displayTitleError && (
              <p id="article-title-error" className="mt-1 flex items-center gap-1 text-xs text-red-600" role="alert">
                <AlertCircle className="h-3 w-3 flex-shrink-0" aria-hidden="true" />
                {displayTitleError}
              </p>
            )}
          </div>

          {/* Category */}
          <div>
            <label htmlFor="category" className="block text-sm font-medium mb-2">
              Category <span className="text-red-500" aria-hidden="true">*</span>
            </label>
            <input
              id="category"
              type="text"
              value={category}
              onChange={(e) => setCategory(e.target.value)}
              onBlur={() => handleBlur('category')}
              required
              disabled={createArticle.isPending || updateArticle.isPending}
              aria-required="true"
              aria-invalid={!!displayCategoryError || undefined}
              aria-describedby={displayCategoryError ? 'article-category-error' : 'article-category-hint'}
              placeholder="e.g., Billing, Technical, Account"
              className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 dark:bg-gray-700 disabled:bg-gray-100 disabled:cursor-not-allowed ${
                displayCategoryError
                  ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
                  : 'border-gray-300 dark:border-gray-600 focus:ring-blue-500'
              }`}
            />
            {displayCategoryError ? (
              <p id="article-category-error" className="mt-1 flex items-center gap-1 text-xs text-red-600" role="alert">
                <AlertCircle className="h-3 w-3 flex-shrink-0" aria-hidden="true" />
                {displayCategoryError}
              </p>
            ) : (
              <p id="article-category-hint" className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                Group related articles by category
              </p>
            )}
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
              disabled={createArticle.isPending || updateArticle.isPending}
              placeholder="e.g., password, reset, security"
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 disabled:bg-gray-100 disabled:cursor-not-allowed"
            />
          </div>

          {/* Content */}
          <div>
            <label htmlFor="content" className="block text-sm font-medium mb-2">
              Content (Markdown) <span className="text-red-500" aria-hidden="true">*</span>
            </label>
            <textarea
              id="content"
              value={content}
              onChange={(e) => setContent(e.target.value)}
              onBlur={() => handleBlur('content')}
              required
              disabled={createArticle.isPending || updateArticle.isPending}
              rows={20}
              aria-required="true"
              aria-invalid={!!displayContentError || undefined}
              aria-describedby={displayContentError ? 'article-content-error' : undefined}
              placeholder="Write your article content using Markdown formatting..."
              className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 dark:bg-gray-700 font-mono text-sm disabled:bg-gray-100 disabled:cursor-not-allowed ${
                displayContentError
                  ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
                  : 'border-gray-300 dark:border-gray-600 focus:ring-blue-500'
              }`}
            />
            {displayContentError && (
              <p id="article-content-error" className="mt-1 flex items-center gap-1 text-xs text-red-600" role="alert">
                <AlertCircle className="h-3 w-3 flex-shrink-0" aria-hidden="true" />
                {displayContentError}
              </p>
            )}
          </div>

          {/* Status */}
          <div>
            <label htmlFor="status" className="block text-sm font-medium mb-2">
              Status <span className="text-red-500" aria-hidden="true">*</span>
            </label>
            <select
              id="status"
              value={status}
              onChange={(e) => setStatus(e.target.value)}
              required
              disabled={createArticle.isPending || updateArticle.isPending}
              aria-required="true"
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 disabled:bg-gray-100 disabled:cursor-not-allowed"
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
              aria-busy={createArticle.isPending || updateArticle.isPending || undefined}
              className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-md disabled:opacity-50 disabled:cursor-not-allowed inline-flex items-center gap-2"
            >
              {createArticle.isPending || updateArticle.isPending ? (
                <>
                  <svg className="animate-spin h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" aria-hidden="true">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Saving...
                </>
              ) : isNewArticle ? (
                'Create Article'
              ) : (
                'Save Changes'
              )}
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
