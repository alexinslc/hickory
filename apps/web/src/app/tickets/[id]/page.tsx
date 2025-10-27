'use client';

import { useState, useEffect } from 'react';
import { useParams } from 'next/navigation';
import { useTicket } from '@/hooks/use-tickets';
import { useAddComment } from '@/hooks/use-comments';
import { AuthGuard } from '@/components/auth-guard';
import Link from 'next/link';
import { getStatusColor, getPriorityColor, formatDate } from '@/lib/ticket-utils';

export default function TicketDetailPage() {
  const params = useParams();
  const ticketId = params.id as string;
  const { data: ticket, isLoading, error } = useTicket(ticketId);
  const addComment = useAddComment(ticketId);
  
  const [commentContent, setCommentContent] = useState('');
  const [showSuccessMessage, setShowSuccessMessage] = useState(false);

  useEffect(() => {
    if (addComment.isSuccess && showSuccessMessage) {
      const timer = setTimeout(() => {
        setShowSuccessMessage(false);
      }, 3000);
      return () => clearTimeout(timer);
    }
  }, [addComment.isSuccess, showSuccessMessage]);

  const handleAddComment = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (commentContent.trim()) {
      try {
        await addComment.mutateAsync({
          content: commentContent,
          isInternal: false,
        });
        setCommentContent('');
        setShowSuccessMessage(true);
      } catch {
        // Error is handled by the mutation
      }
    }
  };

  return (
    <AuthGuard>
      <div className="min-h-screen bg-gray-50">
        <nav className="bg-white shadow-sm">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between h-16">
              <div className="flex items-center">
                <Link href="/dashboard" className="text-2xl font-bold text-gray-900">
                  Hickory Help Desk
                </Link>
              </div>
              <div className="flex items-center space-x-4">
                <Link
                  href="/tickets"
                  className="text-gray-600 hover:text-gray-900"
                >
                  My Tickets
                </Link>
                <Link
                  href="/dashboard"
                  className="text-gray-600 hover:text-gray-900"
                >
                  Dashboard
                </Link>
              </div>
            </div>
          </div>
        </nav>

        <main className="max-w-5xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="px-4 py-6 sm:px-0">
            {isLoading && (
              <div className="text-center py-12">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                <p className="mt-4 text-gray-600">Loading ticket...</p>
              </div>
            )}

            {error && (
              <div className="bg-red-50 border border-red-200 rounded-md p-4">
                <p className="text-red-800">
                  Error loading ticket: {error.message}
                </p>
              </div>
            )}

            {ticket && (
              <div className="space-y-6">
                {/* Ticket Header */}
                <div className="bg-white shadow rounded-lg p-6">
                  <div className="flex items-start justify-between mb-4">
                    <div>
                      <p className="text-sm text-gray-500">{ticket.ticketNumber}</p>
                      <h1 className="text-2xl font-bold text-gray-900 mt-1">
                        {ticket.title}
                      </h1>
                    </div>
                    <div className="flex space-x-2">
                      <span
                        className={`px-3 py-1 text-sm font-semibold rounded-full ${getStatusColor(
                          ticket.status
                        )}`}
                      >
                        {ticket.status}
                      </span>
                      <span
                        className={`px-3 py-1 text-sm font-semibold rounded-full ${getPriorityColor(
                          ticket.priority
                        )}`}
                      >
                        {ticket.priority}
                      </span>
                    </div>
                  </div>

                  <div className="border-t pt-4">
                    <p className="text-gray-700 whitespace-pre-wrap">{ticket.description}</p>
                  </div>

                  <div className="mt-6 grid grid-cols-2 gap-4 border-t pt-4">
                    <div>
                      <p className="text-sm font-medium text-gray-500">Submitted by</p>
                      <p className="mt-1 text-sm text-gray-900">{ticket.submitterName}</p>
                    </div>
                    <div>
                      <p className="text-sm font-medium text-gray-500">Created</p>
                      <p className="mt-1 text-sm text-gray-900">{formatDate(ticket.createdAt, 'long')}</p>
                    </div>
                    {ticket.assignedToName && (
                      <div>
                        <p className="text-sm font-medium text-gray-500">Assigned to</p>
                        <p className="mt-1 text-sm text-gray-900">{ticket.assignedToName}</p>
                      </div>
                    )}
                    <div>
                      <p className="text-sm font-medium text-gray-500">Last updated</p>
                      <p className="mt-1 text-sm text-gray-900">{formatDate(ticket.updatedAt, 'long')}</p>
                    </div>
                  </div>
                </div>

                {/* Comments Section */}
                <div className="bg-white shadow rounded-lg p-6">
                  <h2 className="text-lg font-semibold text-gray-900 mb-4">
                    Activity
                  </h2>

                  {/* Add Comment Form */}
                  <form onSubmit={handleAddComment} className="mb-6">
                    <label htmlFor="comment" className="block text-sm font-medium text-gray-700 mb-2">
                      Add a comment
                    </label>
                    <textarea
                      id="comment"
                      value={commentContent}
                      onChange={(e) => setCommentContent(e.target.value)}
                      rows={3}
                      className="block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-blue-500 sm:text-sm"
                      placeholder="Type your comment here..."
                      disabled={addComment.isPending}
                      maxLength={5000}
                    />
                    <div className="mt-2 flex items-center justify-between">
                      <p className="text-xs text-gray-500">
                        {commentContent.length}/5,000 characters
                      </p>
                      <button
                        type="submit"
                        disabled={addComment.isPending || !commentContent.trim() || commentContent.length > 5000}
                        className="inline-flex justify-center rounded-md border border-transparent bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        {addComment.isPending ? 'Posting...' : 'Post Comment'}
                      </button>
                    </div>
                  </form>

                  {showSuccessMessage && (
                    <div className="rounded-md bg-green-50 border border-green-200 p-4 mb-4">
                      <div className="flex">
                        <div className="flex-shrink-0">
                          <svg className="h-5 w-5 text-green-400" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                          </svg>
                        </div>
                        <div className="ml-3">
                          <p className="text-sm font-medium text-green-800">
                            Comment posted successfully!
                          </p>
                        </div>
                      </div>
                    </div>
                  )}

                  {addComment.isError && (
                    <div className="rounded-md bg-red-50 border border-red-200 p-4 mb-4">
                      <div className="flex">
                        <div className="flex-shrink-0">
                          <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                          </svg>
                        </div>
                        <div className="ml-3">
                          <p className="text-sm font-medium text-red-800">
                            Error posting comment: {addComment.error?.message || 'Please try again'}
                          </p>
                        </div>
                      </div>
                    </div>
                  )}

                  {/* Comment list placeholder - will be implemented in a future phase */}
                  <div className="text-sm text-gray-500 text-center py-8 bg-gray-50 rounded-md">
                    <p className="font-medium mb-1">Comment History</p>
                    <p className="text-xs">
                      A separate API endpoint to fetch comments will be added in a future phase.
                      Comments you add above are being saved successfully.
                    </p>
                  </div>
                </div>
              </div>
            )}
          </div>
        </main>
      </div>
    </AuthGuard>
  );
}
