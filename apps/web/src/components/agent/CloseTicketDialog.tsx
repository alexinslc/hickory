'use client';

import { useState } from 'react';
import { TicketDto } from '@/lib/api-client';

interface CloseTicketDialogProps {
  ticket: TicketDto;
  isOpen: boolean;
  onClose: () => void;
  onCloseTicket: (resolutionNotes: string, rowVersion: string) => Promise<void>;
}

export function CloseTicketDialog({
  ticket,
  isOpen,
  onClose,
  onCloseTicket,
}: CloseTicketDialogProps) {
  const [resolutionNotes, setResolutionNotes] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [touched, setTouched] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setTouched(true);

    if (!isValid) return;

    setIsSubmitting(true);
    setError(null);

    try {
      await onCloseTicket(resolutionNotes, ticket.rowVersion);
      onClose();
      setResolutionNotes('');
      setTouched(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to close ticket');
    } finally {
      setIsSubmitting(false);
    }
  };

  const isValid = resolutionNotes.trim().length >= 10 && resolutionNotes.length <= 5000;

  const getValidationMessage = () => {
    if (!touched || resolutionNotes.length === 0) return null;
    if (resolutionNotes.trim().length < 10) return 'Resolution notes must be at least 10 characters';
    if (resolutionNotes.length > 5000) return 'Resolution notes must be no more than 5,000 characters';
    return null;
  };

  const validationError = getValidationMessage();

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-40"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Dialog */}
      <div className="fixed inset-0 z-50 overflow-y-auto">
        <div className="flex min-h-full items-center justify-center p-4">
          <div className="relative bg-white rounded-lg shadow-xl max-w-2xl w-full">
            {/* Header */}
            <div className="px-6 py-4 border-b border-gray-200">
              <h3 className="text-lg font-semibold text-gray-900">
                Close Ticket
              </h3>
              <button
                onClick={onClose}
                className="absolute top-4 right-4 text-gray-400 hover:text-gray-600"
                disabled={isSubmitting}
              >
                <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            {/* Body */}
            <form onSubmit={handleSubmit}>
              <div className="px-6 py-4 space-y-4">
                {/* Ticket Info */}
                <div className="bg-gray-50 rounded-md p-4">
                  <p className="text-sm text-gray-600">
                    <span className="font-medium">Ticket:</span> {ticket.ticketNumber}
                  </p>
                  <p className="text-sm text-gray-900 mt-1 font-medium">
                    {ticket.title}
                  </p>
                  <div className="mt-2 flex items-center gap-2">
                    <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${
                      ticket.priority === 'Critical' ? 'bg-red-100 text-red-800' :
                      ticket.priority === 'High' ? 'bg-orange-100 text-orange-800' :
                      ticket.priority === 'Medium' ? 'bg-yellow-100 text-yellow-800' :
                      'bg-green-100 text-green-800'
                    }`}>
                      {ticket.priority}
                    </span>
                    <span className="text-xs text-gray-500">
                      Created {new Date(ticket.createdAt).toLocaleDateString()}
                    </span>
                  </div>
                </div>

                {/* Resolution Notes */}
                <div>
                  <label htmlFor="resolutionNotes" className="block text-sm font-medium text-gray-700 mb-2">
                    Resolution Notes <span className="text-red-500">*</span>
                  </label>
                  <textarea
                    id="resolutionNotes"
                    rows={6}
                    value={resolutionNotes}
                    onChange={(e) => setResolutionNotes(e.target.value)}
                    onBlur={() => setTouched(true)}
                    className={`block w-full rounded-md border px-3 py-2 shadow-sm focus:outline-none focus:ring-2 sm:text-sm ${
                      validationError
                        ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
                        : 'border-gray-300 focus:border-green-500 focus:ring-green-500'
                    }`}
                    placeholder="Describe how this issue was resolved..."
                    disabled={isSubmitting}
                    required
                    minLength={10}
                    maxLength={5000}
                    aria-invalid={!!validationError}
                    aria-describedby={validationError ? 'notes-error' : 'notes-description'}
                  />
                  {validationError ? (
                    <p id="notes-error" className="mt-1 text-xs text-red-600">
                      {validationError}
                    </p>
                  ) : (
                    <p id="notes-description" className="mt-1 text-xs text-gray-500">
                      {resolutionNotes.length}/5,000 characters (minimum 10)
                    </p>
                  )}
                </div>

                {/* Warning Message */}
                <div className="rounded-md bg-yellow-50 border border-yellow-200 p-4">
                  <div className="flex">
                    <div className="flex-shrink-0">
                      <svg className="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                        <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                      </svg>
                    </div>
                    <div className="ml-3">
                      <p className="text-sm text-yellow-700">
                        Closing this ticket will mark it as resolved. You can still reopen it later if needed.
                      </p>
                    </div>
                  </div>
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
              </div>

              {/* Footer */}
              <div className="px-6 py-4 border-t border-gray-200 flex justify-end space-x-3">
                <button
                  type="button"
                  onClick={onClose}
                  className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                  disabled={isSubmitting}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 text-sm font-medium text-white bg-green-600 border border-transparent rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
                  disabled={!isValid || isSubmitting}
                >
                  {isSubmitting ? (
                    <span className="flex items-center">
                      <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                      Closing...
                    </span>
                  ) : (
                    'Close Ticket'
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </>
  );
}
