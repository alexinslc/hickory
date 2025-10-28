'use client';

import { useState } from 'react';

interface InternalNoteFormProps {
  ticketId: string;
  onSubmit: (content: string) => Promise<void>;
  isSubmitting?: boolean;
}

export function InternalNoteForm({ ticketId, onSubmit, isSubmitting = false }: InternalNoteFormProps) {
  const [content, setContent] = useState('');
  const [touched, setTouched] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setTouched(true);
    setError(null);

    if (!contentValid) {
      return;
    }

    try {
      await onSubmit(content);
      // Clear form on success
      setContent('');
      setTouched(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add internal note');
    }
  };

  const contentValid = content.trim().length >= 1 && content.length <= 5000;

  const getValidationMessage = () => {
    if (!touched || content.length === 0) return null;
    if (content.trim().length < 1) return 'Internal note cannot be empty';
    if (content.length > 5000) return 'Internal note must be no more than 5,000 characters';
    return null;
  };

  const validationError = getValidationMessage();

  return (
    <form onSubmit={handleSubmit} className="space-y-4 bg-yellow-50 border border-yellow-300 rounded-lg p-4">
      {/* Header with Icon */}
      <div className="flex items-center gap-2 border-b border-yellow-300 pb-2">
        <svg className="w-5 h-5 text-yellow-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
        </svg>
        <label htmlFor={`internal-note-${ticketId}`} className="block text-sm font-semibold text-yellow-900">
          Add Internal Note
        </label>
      </div>

      {/* Info Banner */}
      <div className="flex items-start gap-2 text-xs text-yellow-700 bg-yellow-100 rounded p-2">
        <svg className="w-4 h-4 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
          <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
        </svg>
        <p>Internal notes are only visible to agents and administrators. Customers cannot see these notes.</p>
      </div>

      {/* Note Textarea */}
      <div>
        <textarea
          id={`internal-note-${ticketId}`}
          rows={4}
          value={content}
          onChange={(e) => setContent(e.target.value)}
          onBlur={() => setTouched(true)}
          className={`block w-full rounded-md border px-3 py-2 shadow-sm focus:outline-none focus:ring-2 sm:text-sm bg-white ${
            validationError
              ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
              : 'border-yellow-300 focus:border-yellow-500 focus:ring-yellow-500'
          }`}
          placeholder="Add an internal note (not visible to the customer)..."
          disabled={isSubmitting}
          required
          maxLength={5000}
          aria-invalid={!!validationError}
          aria-describedby={validationError ? 'internal-note-error' : 'internal-note-description'}
        />
        {validationError ? (
          <p id="internal-note-error" className="mt-1 text-xs text-red-600">
            {validationError}
          </p>
        ) : (
          <p id="internal-note-description" className="mt-1 text-xs text-yellow-700">
            {content.length}/5,000 characters
          </p>
        )}
      </div>

      {/* Error Display */}
      {error && (
        <div className="rounded-md bg-red-50 p-3 border border-red-200">
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
          className="px-4 py-2 text-sm font-medium text-white bg-yellow-600 hover:bg-yellow-700 rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-yellow-500 disabled:opacity-50 disabled:cursor-not-allowed"
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
              <svg className="inline-block w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
              Add Internal Note
            </>
          )}
        </button>
      </div>
    </form>
  );
}
