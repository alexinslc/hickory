'use client';

import { useState } from 'react';
import { useAuth } from '@/hooks/use-auth';

interface CommentFormProps {
  ticketId: string;
  onSubmit: (content: string, isInternal: boolean) => Promise<void>;
  isSubmitting?: boolean;
  allowInternalNotes?: boolean;
}

export function CommentForm({ 
  ticketId, 
  onSubmit, 
  isSubmitting = false,
  allowInternalNotes = false 
}: CommentFormProps) {
  const { user } = useAuth();
  const [content, setContent] = useState('');
  const [isInternal, setIsInternal] = useState(false);
  const [touched, setTouched] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isAgent = user?.role === 'Agent' || user?.role === 'Administrator';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setTouched(true);
    setError(null);

    if (!contentValid) {
      return;
    }

    try {
      await onSubmit(content, isInternal);
      // Clear form on success
      setContent('');
      setIsInternal(false);
      setTouched(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add comment');
    }
  };

  const contentValid = content.trim().length >= 1 && content.length <= 5000;

  const getValidationMessage = () => {
    if (!touched || content.length === 0) return null;
    if (content.trim().length < 1) return 'Comment cannot be empty';
    if (content.length > 5000) return 'Comment must be no more than 5,000 characters';
    return null;
  };

  const validationError = getValidationMessage();

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {/* Internal Note Toggle (Agents only) */}
      {isAgent && allowInternalNotes && (
        <div className="flex items-center gap-2 p-3 bg-yellow-50 border border-yellow-200 rounded-md">
          <input
            type="checkbox"
            id={`internal-${ticketId}`}
            checked={isInternal}
            onChange={(e) => setIsInternal(e.target.checked)}
            disabled={isSubmitting}
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
          />
          <label htmlFor={`internal-${ticketId}`} className="text-sm font-medium text-gray-700 cursor-pointer">
            Internal Note
            <span className="ml-2 text-xs text-gray-500">(Only visible to agents)</span>
          </label>
        </div>
      )}

      {/* Comment Textarea */}
      <div>
        <label htmlFor={`comment-${ticketId}`} className="block text-sm font-medium text-gray-700 mb-2">
          {isInternal ? 'Internal Note' : 'Add Comment'}
        </label>
        <textarea
          id={`comment-${ticketId}`}
          rows={4}
          value={content}
          onChange={(e) => setContent(e.target.value)}
          onBlur={() => setTouched(true)}
          className={`block w-full rounded-md border px-3 py-2 shadow-sm focus:outline-none focus:ring-2 sm:text-sm ${
            validationError
              ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
              : isInternal
              ? 'border-yellow-300 focus:border-yellow-500 focus:ring-yellow-500 bg-yellow-50'
              : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500'
          }`}
          placeholder={isInternal ? "Add an internal note (not visible to the customer)..." : "Add your comment..."}
          disabled={isSubmitting}
          required
          maxLength={5000}
          aria-invalid={!!validationError}
          aria-describedby={validationError ? 'comment-error' : 'comment-description'}
        />
        {validationError ? (
          <p id="comment-error" className="mt-1 text-xs text-red-600">
            {validationError}
          </p>
        ) : (
          <p id="comment-description" className="mt-1 text-xs text-gray-500">
            {content.length}/5,000 characters
          </p>
        )}
      </div>

      {/* Error Display */}
      {error && (
        <div className="rounded-md bg-red-50 p-3">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm text-red-700">{error}</p>
            </div>
          </div>
        </div>
      )}

      {/* Submit Button */}
      <div className="flex justify-end">
        <button
          type="submit"
          className={`px-4 py-2 text-sm font-medium text-white rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed ${
            isInternal
              ? 'bg-yellow-600 hover:bg-yellow-700 focus:ring-yellow-500'
              : 'bg-blue-600 hover:bg-blue-700 focus:ring-blue-500'
          }`}
          disabled={!contentValid || isSubmitting}
        >
          {isSubmitting ? (
            <span className="flex items-center">
              <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Posting...
            </span>
          ) : (
            <>
              {isInternal ? (
                <>
                  <svg className="inline-block w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                  Add Internal Note
                </>
              ) : (
                'Add Comment'
              )}
            </>
          )}
        </button>
      </div>
    </form>
  );
}
