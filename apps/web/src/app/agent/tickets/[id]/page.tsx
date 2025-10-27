'use client';

import { useTicket } from '@/hooks/use-tickets';
import { useAuth } from '@/hooks/use-auth';
import { useParams, useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { useAddComment } from '@/hooks/use-comments';
import {
  useAssignTicket,
  useUpdateTicketStatus,
  useUpdateTicketPriority,
  useCloseTicket,
} from '@/hooks/use-agent';
import { formatDistanceToNow } from 'date-fns';
import { AxiosError } from 'axios';

const STATUS_OPTIONS = ['Open', 'InProgress', 'Resolved'];
const PRIORITY_OPTIONS = ['Low', 'Medium', 'High', 'Critical'];

export default function AgentTicketDetailPage() {
  const params = useParams();
  const router = useRouter();
  const ticketId = params.id as string;
  const { user } = useAuth();
  const { data: ticket, isLoading, error, refetch } = useTicket(ticketId);
  
  const [commentContent, setCommentContent] = useState('');
  const [isInternal, setIsInternal] = useState(false);
  const [showCloseDialog, setShowCloseDialog] = useState(false);
  const [resolutionNotes, setResolutionNotes] = useState('');
  const [showSuccess, setShowSuccess] = useState(false);
  const [showConflictDialog, setShowConflictDialog] = useState(false);
  const [conflictAction, setConflictAction] = useState<(() => void) | null>(null);
  
  const addComment = useAddComment(ticketId);
  const assignTicket = useAssignTicket();
  const updateStatus = useUpdateTicketStatus();
  const updatePriority = useUpdateTicketPriority();
  const closeTicket = useCloseTicket();

  useEffect(() => {
    if (addComment.isSuccess && showSuccess) {
      const timer = setTimeout(() => {
        setShowSuccess(false);
      }, 3000);
      return () => clearTimeout(timer);
    }
  }, [addComment.isSuccess, showSuccess]);

  // Check if user is agent or admin
  useEffect(() => {
    if (user && user.role !== 'Agent' && user.role !== 'Administrator') {
      router.push(`/tickets/${ticketId}`);
    }
  }, [user, router, ticketId]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
      </div>
    );
  }

  if (error || !ticket) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <p className="text-red-600">Failed to load ticket</p>
      </div>
    );
  }

  const handleAddComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (commentContent.trim()) {
      await addComment.mutateAsync({
        content: commentContent,
        isInternal,
      });
      setCommentContent('');
      setIsInternal(false);
      setShowSuccess(true);
    }
  };

  const handleConflictRetry = async () => {
    setShowConflictDialog(false);
    await refetch(); // Refresh ticket data to get latest rowVersion
    if (conflictAction) {
      conflictAction(); // Retry the original action
    }
  };

  const handleConcurrencyError = (error: unknown, retryAction: () => void) => {
    if (error instanceof AxiosError && error.response?.status === 409) {
      setConflictAction(() => retryAction);
      setShowConflictDialog(true);
      return true;
    }
    return false;
  };

  const handleAssignToMe = async () => {
    if (user?.userId && ticket?.rowVersion) {
      try {
        await assignTicket.mutateAsync({ 
          ticketId, 
          agentId: user.userId,
          rowVersion: ticket.rowVersion 
        });
      } catch (error) {
        if (!handleConcurrencyError(error, handleAssignToMe)) {
          throw error; // Re-throw if not a concurrency error
        }
      }
    }
  };

  const handleStatusChange = async (newStatus: string) => {
    if (ticket?.rowVersion) {
      try {
        await updateStatus.mutateAsync({ 
          ticketId, 
          newStatus,
          rowVersion: ticket.rowVersion 
        });
      } catch (error) {
        if (!handleConcurrencyError(error, () => handleStatusChange(newStatus))) {
          throw error;
        }
      }
    }
  };

  const handlePriorityChange = async (newPriority: string) => {
    if (ticket?.rowVersion) {
      try {
        await updatePriority.mutateAsync({ 
          ticketId, 
          newPriority,
          rowVersion: ticket.rowVersion 
        });
      } catch (error) {
        if (!handleConcurrencyError(error, () => handlePriorityChange(newPriority))) {
          throw error;
        }
      }
    }
  };

  const handleCloseTicket = async () => {
    if (resolutionNotes.trim().length >= 10 && ticket?.rowVersion) {
      try {
        await closeTicket.mutateAsync({ 
          ticketId, 
          resolutionNotes,
          rowVersion: ticket.rowVersion 
        });
        setShowCloseDialog(false);
        setResolutionNotes('');
      } catch (error) {
        if (!handleConcurrencyError(error, handleCloseTicket)) {
          throw error;
        }
      }
    }
  };

  const isClosed = ticket.status === 'Closed' || ticket.status === 'Cancelled';
  const isAssignedToMe = ticket.assignedToId === user?.userId;

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
        {/* Header with actions */}
        <div className="mb-6 bg-white shadow rounded-lg p-6">
          <div className="flex items-start justify-between mb-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">{ticket.ticketNumber}</h1>
              <p className="mt-1 text-gray-600">{ticket.title}</p>
            </div>
            <div className="flex gap-2">
              {!ticket.assignedToId && (
                <button
                  onClick={handleAssignToMe}
                  disabled={assignTicket.isPending}
                  className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
                >
                  Assign to Me
                </button>
              )}
              {isAssignedToMe && !isClosed && (
                <button
                  onClick={() => setShowCloseDialog(true)}
                  disabled={closeTicket.isPending}
                  className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50"
                >
                  Close Ticket
                </button>
              )}
            </div>
          </div>

          {/* Status and Priority selectors */}
          <div className="grid grid-cols-2 gap-4 mt-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select
                value={ticket.status}
                onChange={(e) => handleStatusChange(e.target.value)}
                disabled={isClosed || updateStatus.isPending}
                className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm disabled:bg-gray-100"
              >
                {STATUS_OPTIONS.map((status) => (
                  <option key={status} value={status}>
                    {status}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Priority</label>
              <select
                value={ticket.priority}
                onChange={(e) => handlePriorityChange(e.target.value)}
                disabled={isClosed || updatePriority.isPending}
                className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm disabled:bg-gray-100"
              >
                {PRIORITY_OPTIONS.map((priority) => (
                  <option key={priority} value={priority}>
                    {priority}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Ticket metadata */}
          <div className="mt-4 grid grid-cols-2 gap-4 text-sm">
            <div>
              <span className="text-gray-500">Submitter:</span>{' '}
              <span className="font-medium">{ticket.submitterName}</span>
            </div>
            <div>
              <span className="text-gray-500">Assigned to:</span>{' '}
              <span className="font-medium">
                {ticket.assignedToName || (
                  <span className="text-red-600">Unassigned</span>
                )}
              </span>
            </div>
            <div>
              <span className="text-gray-500">Created:</span>{' '}
              <span className="font-medium">
                {formatDistanceToNow(new Date(ticket.createdAt), { addSuffix: true })}
              </span>
            </div>
            <div>
              <span className="text-gray-500">Last updated:</span>{' '}
              <span className="font-medium">
                {formatDistanceToNow(new Date(ticket.updatedAt), { addSuffix: true })}
              </span>
            </div>
          </div>
        </div>

        {/* Ticket description */}
        <div className="mb-6 bg-white shadow rounded-lg p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-3">Description</h2>
          <div className="prose max-w-none text-gray-700 whitespace-pre-wrap">
            {ticket.description}
          </div>
        </div>

        {/* Resolution notes (if closed) */}
        {ticket.resolutionNotes && (
          <div className="mb-6 bg-green-50 border border-green-200 rounded-lg p-6">
            <h2 className="text-lg font-semibold text-green-900 mb-3">Resolution</h2>
            <div className="prose max-w-none text-green-800 whitespace-pre-wrap">
              {ticket.resolutionNotes}
            </div>
            {ticket.closedAt && (
              <p className="mt-2 text-sm text-green-600">
                Closed {formatDistanceToNow(new Date(ticket.closedAt), { addSuffix: true })}
              </p>
            )}
          </div>
        )}

        {/* Add comment form */}
        {!isClosed && (
          <div className="mb-6 bg-white shadow rounded-lg p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Add Comment</h2>
            
            {showSuccess && (
              <div className="mb-4 flex items-center gap-2 rounded-md bg-green-50 border border-green-200 p-3 text-green-800">
                <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                    clipRule="evenodd"
                  />
                </svg>
                <span>Comment added successfully</span>
              </div>
            )}

            {addComment.isError && (
              <div className="mb-4 rounded-md bg-red-50 border border-red-200 p-3 text-red-800">
                <p>Failed to add comment. Please try again.</p>
              </div>
            )}

            <form onSubmit={handleAddComment}>
              <textarea
                value={commentContent}
                onChange={(e) => setCommentContent(e.target.value)}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-blue-500 sm:text-sm"
                rows={4}
                placeholder="Add your response or internal note..."
                disabled={addComment.isPending}
                maxLength={5000}
              />
              <div className="mt-2 flex items-center justify-between">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={isInternal}
                    onChange={(e) => setIsInternal(e.target.checked)}
                    className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="ml-2 text-sm text-gray-700">Internal note (not visible to user)</span>
                </label>
                <div className="flex items-center gap-3">
                  <span className="text-sm text-gray-500">{commentContent.length}/5,000</span>
                  <button
                    type="submit"
                    disabled={!commentContent.trim() || addComment.isPending}
                    className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {addComment.isPending ? 'Adding...' : 'Add Comment'}
                  </button>
                </div>
              </div>
            </form>
          </div>
        )}

        {/* Close ticket dialog */}
        {showCloseDialog && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-lg w-full mx-4">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Close Ticket</h3>
              <p className="text-gray-600 mb-4">
                Please provide resolution notes to close this ticket. This will be visible to the user.
              </p>
              <textarea
                value={resolutionNotes}
                onChange={(e) => setResolutionNotes(e.target.value)}
                className="block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-blue-500 sm:text-sm"
                rows={6}
                placeholder="Describe how the issue was resolved..."
                maxLength={5000}
              />
              <p className="mt-1 text-sm text-gray-500">{resolutionNotes.length}/5,000 characters (minimum 10)</p>
              
              {closeTicket.isError && (
                <div className="mt-3 text-sm text-red-600">
                  Failed to close ticket. Please try again.
                </div>
              )}

              <div className="mt-6 flex justify-end gap-3">
                <button
                  onClick={() => {
                    setShowCloseDialog(false);
                    setResolutionNotes('');
                  }}
                  disabled={closeTicket.isPending}
                  className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                >
                  Cancel
                </button>
                <button
                  onClick={handleCloseTicket}
                  disabled={resolutionNotes.trim().length < 10 || closeTicket.isPending}
                  className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {closeTicket.isPending ? 'Closing...' : 'Close Ticket'}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Conflict dialog */}
        {showConflictDialog && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
              <div className="flex items-center gap-3 mb-4">
                <div className="flex-shrink-0">
                  <svg className="h-6 w-6 text-yellow-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                  </svg>
                </div>
                <h3 className="text-lg font-semibold text-gray-900">Ticket Updated</h3>
              </div>
              <p className="text-gray-600 mb-6">
                This ticket was modified by another user. Please refresh to see the latest changes and try again.
              </p>
              <div className="flex justify-end gap-3">
                <button
                  onClick={() => setShowConflictDialog(false)}
                  className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  onClick={handleConflictRetry}
                  className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                >
                  Refresh & Retry
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
