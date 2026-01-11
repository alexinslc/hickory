'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useGetTicketById, useUpdateTicketStatus, useAssignTicket, useCloseTicket } from '@/lib/queries/tickets';
import { useGetComments, useAddComment, useAddInternalNote } from '@/lib/queries/comments';
import { useAuthStore } from '@/store/auth-store';
import { useTicketDetails } from '@/hooks/use-tickets';
import { AuthGuard } from '@/components/auth-guard';
import { FileUpload } from '@/components/attachments/FileUpload';
import { AttachmentList } from '@/components/attachments/AttachmentList';

// Utility functions for status and priority colors
function getStatusColor(status: string): string {
  const colors: Record<string, string> = {
    Open: 'bg-blue-100 text-blue-800',
    InProgress: 'bg-yellow-100 text-yellow-800',
    Resolved: 'bg-green-100 text-green-800',
    Closed: 'bg-gray-100 text-gray-800',
    Cancelled: 'bg-red-100 text-red-800',
  };
  return colors[status] || 'bg-gray-100 text-gray-800';
}

function getPriorityColor(priority: string): string {
  const colors: Record<string, string> = {
    Low: 'bg-slate-100 text-slate-700',
    Medium: 'bg-blue-100 text-blue-700',
    High: 'bg-orange-100 text-orange-700',
    Critical: 'bg-red-100 text-red-700',
  };
  return colors[priority] || 'bg-gray-100 text-gray-700';
}

function formatDate(dateString: string, format: 'short' | 'long' = 'short'): string {
  const date = new Date(dateString);
  if (format === 'long') {
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export default function TicketDetailPage() {
  const params = useParams();
  const router = useRouter();
  const ticketId = params.id as string;
  const { data: ticket, isLoading, error } = useGetTicketById(ticketId);
  const { data: ticketDetails, isLoading: detailsLoading, refetch: refetchTicketDetails } = useTicketDetails(ticketId);
  const { data: comments, isLoading: commentsLoading } = useGetComments(ticketId);
  const { user } = useAuthStore();
  const addCommentMutation = useAddComment(ticketId);
  const addInternalNoteMutation = useAddInternalNote(ticketId);
  const updateStatusMutation = useUpdateTicketStatus();
  const assignTicketMutation = useAssignTicket();
  const closeTicketMutation = useCloseTicket();
  
  const [showAssignDialog, setShowAssignDialog] = useState(false);
  const [showCloseDialog, setShowCloseDialog] = useState(false);
  const [commentContent, setCommentContent] = useState('');
  const [internalNoteContent, setInternalNoteContent] = useState('');
  const [resolutionNotes, setResolutionNotes] = useState('');
  const [selectedAgentId, setSelectedAgentId] = useState('');
  
  // Check if user is an agent
  const isAgent = user?.role === 'Agent' || user?.role === 'Admin';

  const handleCommentSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!commentContent.trim()) return;
    
    await addCommentMutation.mutateAsync({
      content: commentContent,
      isInternal: false,
    });
    setCommentContent('');
  };

  const handleInternalNoteSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!internalNoteContent.trim()) return;
    
    await addInternalNoteMutation.mutateAsync(internalNoteContent);
    setInternalNoteContent('');
  };

  const handleStatusChange = async (newStatus: string) => {
    if (!ticket) return;
    await updateStatusMutation.mutateAsync({
      ticketId: ticket.id,
      newStatus,
      rowVersion: ticket.rowVersion,
    });
  };

  const handleAssignTicket = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!ticket || !selectedAgentId) return;
    
    await assignTicketMutation.mutateAsync({
      ticketId: ticket.id,
      agentId: selectedAgentId,
      rowVersion: ticket.rowVersion,
    });
    setShowAssignDialog(false);
    setSelectedAgentId('');
  };

  const handleCloseTicket = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!ticket || !resolutionNotes.trim()) return;
    
    await closeTicketMutation.mutateAsync({
      ticketId: ticket.id,
      resolutionNotes,
      rowVersion: ticket.rowVersion,
    });
    setShowCloseDialog(false);
    setResolutionNotes('');
  };

  if (isLoading) {
    return (
      <AuthGuard>
        <div className="min-h-screen bg-gray-50">
          <div className="container mx-auto py-8">
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
          </div>
        </div>
      </AuthGuard>
    );
  }

  if (error) {
    return (
      <AuthGuard>
        <div className="min-h-screen bg-gray-50">
          <div className="container mx-auto py-8 px-4">
            <div className="bg-white rounded-lg shadow p-6 border-2 border-red-300">
              <h2 className="text-xl font-semibold text-red-700 mb-2">Error Loading Ticket</h2>
              <p className="text-gray-600">{error.message}</p>
            </div>
          </div>
        </div>
      </AuthGuard>
    );
  }

  if (!ticket) {
    return (
      <AuthGuard>
        <div className="min-h-screen bg-gray-50">
          <div className="container mx-auto py-8 px-4">
            <div className="bg-white rounded-lg shadow p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-2">Ticket Not Found</h2>
              <p className="text-gray-600">The requested ticket could not be found.</p>
            </div>
          </div>
        </div>
      </AuthGuard>
    );
  }

  return (
    <AuthGuard>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <div className="bg-white border-b shadow-sm">
          <div className="container mx-auto px-4 py-4">
            <div className="flex items-center justify-between flex-wrap gap-4">
              <div className="flex items-center gap-4">
                <button
                  onClick={() => router.back()}
                  className="text-gray-600 hover:text-gray-900 font-medium text-sm flex items-center gap-2"
                >
                  ← Back
                </button>
                <div className="border-l h-6 border-gray-300"></div>
                <div>
                  <p className="text-sm text-gray-500">{ticket.ticketNumber}</p>
                  <h1 className="text-2xl font-bold text-gray-900">{ticket.title}</h1>
                </div>
              </div>
              <div className="flex items-center gap-2">
                <span className={`px-3 py-1 text-sm font-semibold rounded-full ${getPriorityColor(ticket.priority)}`}>
                  {ticket.priority}
                </span>
                {isAgent ? (
                  <select
                    value={ticket.status}
                    onChange={(e) => handleStatusChange(e.target.value)}
                    disabled={updateStatusMutation.isPending}
                    className={`px-3 py-1 text-sm font-semibold rounded-full border-0 cursor-pointer ${getStatusColor(ticket.status)}`}
                  >
                    <option value="Open">Open</option>
                    <option value="InProgress">In Progress</option>
                    <option value="Resolved">Resolved</option>
                    <option value="Closed">Closed</option>
                    <option value="Cancelled">Cancelled</option>
                  </select>
                ) : (
                  <span className={`px-3 py-1 text-sm font-semibold rounded-full ${getStatusColor(ticket.status)}`}>
                    {ticket.status}
                  </span>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Main Content */}
        <div className="container mx-auto px-4 py-8">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Left Column - Main Content */}
            <div className="lg:col-span-2 space-y-6">
              {/* Ticket Description */}
              <div className="bg-white rounded-lg shadow p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">Description</h2>
                <p className="whitespace-pre-wrap text-gray-700">{ticket.description}</p>
              </div>

              {/* Attachments Section */}
              <div className="bg-white rounded-lg shadow p-6">
                <div className="mb-4">
                  <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                    Attachments
                    {ticketDetails?.attachments && ticketDetails.attachments.length > 0 && (
                      <span className="bg-gray-100 text-gray-600 px-2 py-1 rounded-full text-xs font-normal">
                        {ticketDetails.attachments.length}
                      </span>
                    )}
                  </h2>
                  <p className="text-sm text-gray-500 mt-1">Upload files related to this ticket</p>
                </div>

                {/* File Upload */}
                <div className="mb-6">
                  <FileUpload 
                    ticketId={ticketId}
                    onUploadComplete={() => refetchTicketDetails()}
                  />
                </div>

                {/* Attachment List */}
                {detailsLoading ? (
                  <div className="text-center py-4">
                    <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600 mx-auto"></div>
                    <p className="text-sm text-gray-500 mt-2">Loading attachments...</p>
                  </div>
                ) : ticketDetails?.attachments && ticketDetails.attachments.length > 0 ? (
                  <AttachmentList 
                    attachments={ticketDetails.attachments} 
                    canDelete={true}
                    onDelete={() => refetchTicketDetails()}
                  />
                ) : (
                  <div className="text-center py-4 bg-gray-50 rounded-lg">
                    <svg className="h-10 w-10 mx-auto text-gray-400 mb-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                    </svg>
                    <p className="text-sm text-gray-600">No attachments yet</p>
                    <p className="text-xs text-gray-500 mt-1">Upload files using the form above</p>
                  </div>
                )}
              </div>

              {/* Comments Section */}
              <div className="bg-white rounded-lg shadow p-6">
                <div className="mb-4">
                  <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                    Activity
                    <span className="bg-gray-100 text-gray-600 px-2 py-1 rounded-full text-xs font-normal">
                      {ticket.commentCount}
                    </span>
                  </h2>
                  <p className="text-sm text-gray-500 mt-1">Add comments to communicate about this ticket</p>
                </div>

                {/* Comment Form */}
                <form onSubmit={handleCommentSubmit} className="mb-6">
                  <label htmlFor="comment" className="block text-sm font-medium text-gray-700 mb-2">
                    Add a comment
                  </label>
                  <textarea
                    id="comment"
                    value={commentContent}
                    onChange={(e) => setCommentContent(e.target.value)}
                    rows={4}
                    maxLength={5000}
                    placeholder="Type your comment here..."
                    disabled={addCommentMutation.isPending}
                    className="block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 text-sm disabled:bg-gray-100"
                  />
                  <div className="mt-2 flex items-center justify-between">
                    <p className="text-xs text-gray-500">{commentContent.length}/5,000 characters</p>
                    <button
                      type="submit"
                      disabled={addCommentMutation.isPending || !commentContent.trim()}
                      className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {addCommentMutation.isPending ? 'Posting...' : 'Post Comment'}
                    </button>
                  </div>
                </form>

                <div className="border-t border-gray-200 my-6"></div>

                {/* Comment History */}
                <div className="space-y-4">
                  {commentsLoading && (
                    <div className="text-center py-8">
                      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
                      <p className="text-sm text-gray-500 mt-2">Loading comments...</p>
                    </div>
                  )}
                  
                  {!commentsLoading && comments && comments.length === 0 && (
                    <div className="text-center py-8 bg-gray-50 rounded-lg">
                      <svg className="h-12 w-12 mx-auto text-gray-400 mb-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                      </svg>
                      <p className="font-medium text-sm text-gray-600 mb-1">No comments yet</p>
                      <p className="text-xs text-gray-500 max-w-md mx-auto">
                        Be the first to add a comment to this ticket.
                      </p>
                    </div>
                  )}
                  
                  {!commentsLoading && comments && comments.length > 0 && (
                    <>
                      {comments.filter(c => !c.isInternal).map((comment) => (
                        <div key={comment.id} className="flex gap-3 p-4 bg-gray-50 rounded-lg">
                          <div className="flex-shrink-0">
                            <div className="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                              <span className="text-blue-700 font-semibold text-sm">
                                {comment.authorName.split(' ').map(n => n[0]).join('')}
                              </span>
                            </div>
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-2 mb-1">
                              <p className="text-sm font-semibold text-gray-900">{comment.authorName}</p>
                              <span className="text-xs text-gray-500">•</span>
                              <p className="text-xs text-gray-500">{formatDate(comment.createdAt, 'long')}</p>
                            </div>
                            <p className="text-sm text-gray-700 whitespace-pre-wrap">{comment.content}</p>
                          </div>
                        </div>
                      ))}
                      
                      {isAgent && comments.filter(c => c.isInternal).length > 0 && (
                        <div className="mt-6 pt-6 border-t-2 border-yellow-200">
                          <h3 className="text-sm font-semibold text-yellow-900 mb-4 flex items-center gap-2">
                            <svg className="h-4 w-4" fill="currentColor" viewBox="0 0 20 20">
                              <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
                            </svg>
                            Internal Notes (Agent Only)
                          </h3>
                          <div className="space-y-3">
                            {comments.filter(c => c.isInternal).map((note) => (
                              <div key={note.id} className="flex gap-3 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
                                <div className="flex-shrink-0">
                                  <div className="h-10 w-10 rounded-full bg-yellow-100 flex items-center justify-center">
                                    <span className="text-yellow-700 font-semibold text-sm">
                                      {note.authorName.split(' ').map(n => n[0]).join('')}
                                    </span>
                                  </div>
                                </div>
                                <div className="flex-1 min-w-0">
                                  <div className="flex items-center gap-2 mb-1">
                                    <p className="text-sm font-semibold text-yellow-900">{note.authorName}</p>
                                    <span className="text-xs text-yellow-600">•</span>
                                    <p className="text-xs text-yellow-600">{formatDate(note.createdAt, 'long')}</p>
                                  </div>
                                  <p className="text-sm text-yellow-900 whitespace-pre-wrap">{note.content}</p>
                                </div>
                              </div>
                            ))}
                          </div>
                        </div>
                      )}
                    </>
                  )}
                </div>
              </div>

              {/* Internal Note Form - Agent Only */}
              {isAgent && (
                <div className="bg-yellow-50 border-2 border-yellow-200 rounded-lg shadow p-6">
                  <div className="mb-4">
                    <h2 className="text-lg font-semibold text-yellow-900 flex items-center gap-2">
                      <svg className="h-5 w-5" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
                      </svg>
                      Internal Notes
                    </h2>
                    <p className="text-sm text-yellow-700 mt-1">Notes visible only to agents and admins</p>
                  </div>

                  <form onSubmit={handleInternalNoteSubmit}>
                    <textarea
                      value={internalNoteContent}
                      onChange={(e) => setInternalNoteContent(e.target.value)}
                      rows={3}
                      maxLength={5000}
                      placeholder="Add an internal note..."
                      disabled={addInternalNoteMutation.isPending}
                      className="block w-full rounded-md border border-yellow-300 bg-white px-3 py-2 shadow-sm focus:border-yellow-500 focus:outline-none focus:ring-1 focus:ring-yellow-500 text-sm disabled:bg-gray-100"
                    />
                    <div className="mt-2 flex items-center justify-between">
                      <p className="text-xs text-yellow-700">{internalNoteContent.length}/5,000 characters</p>
                      <button
                        type="submit"
                        disabled={addInternalNoteMutation.isPending || !internalNoteContent.trim()}
                        className="px-4 py-2 bg-yellow-600 text-white text-sm font-medium rounded-md hover:bg-yellow-700 focus:outline-none focus:ring-2 focus:ring-yellow-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        {addInternalNoteMutation.isPending ? 'Adding...' : 'Add Internal Note'}
                      </button>
                    </div>
                  </form>
                </div>
              )}
            </div>

            {/* Right Column - Sidebar */}
            <div className="space-y-6">
              {/* Agent Actions */}
              {isAgent && (
                <div className="bg-white rounded-lg shadow p-6">
                  <h2 className="text-lg font-semibold text-gray-900 mb-4">Agent Actions</h2>
                  <div className="space-y-3">
                    <button
                      onClick={() => setShowAssignDialog(true)}
                      disabled={ticket.status === 'Closed' || ticket.status === 'Cancelled'}
                      className="w-full px-4 py-2 bg-white border border-gray-300 text-gray-700 text-sm font-medium rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-start gap-2"
                    >
                      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                      </svg>
                      {ticket.assignedToName ? 'Reassign Ticket' : 'Assign Ticket'}
                    </button>

                    <button
                      onClick={() => setShowCloseDialog(true)}
                      disabled={ticket.status === 'Closed' || ticket.status === 'Cancelled'}
                      className="w-full px-4 py-2 bg-white border border-gray-300 text-gray-700 text-sm font-medium rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-start gap-2"
                    >
                      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      Close Ticket
                    </button>
                  </div>
                </div>
              )}

              {/* Ticket Details */}
              <div className="bg-white rounded-lg shadow p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">Details</h2>
                <div className="space-y-4">
                  <div>
                    <p className="text-xs font-medium text-gray-500 uppercase mb-1">Submitted by</p>
                    <p className="text-sm text-gray-900">{ticket.submitterName}</p>
                  </div>

                  {ticket.assignedToName && (
                    <div>
                      <p className="text-xs font-medium text-gray-500 uppercase mb-1">Assigned to</p>
                      <p className="text-sm text-gray-900">{ticket.assignedToName}</p>
                    </div>
                  )}

                  <div className="border-t border-gray-200 pt-4">
                    <p className="text-xs font-medium text-gray-500 uppercase mb-1">Created</p>
                    <p className="text-sm text-gray-900">{formatDate(ticket.createdAt, 'long')}</p>
                  </div>

                  <div>
                    <p className="text-xs font-medium text-gray-500 uppercase mb-1">Last updated</p>
                    <p className="text-sm text-gray-900">{formatDate(ticket.updatedAt, 'long')}</p>
                  </div>

                  {ticket.closedAt && (
                    <div>
                      <p className="text-xs font-medium text-gray-500 uppercase mb-1">Closed</p>
                      <p className="text-sm text-gray-900">{formatDate(ticket.closedAt, 'long')}</p>
                    </div>
                  )}

                  {ticket.categoryName && (
                    <div className="border-t border-gray-200 pt-4">
                      <p className="text-xs font-medium text-gray-500 uppercase mb-2">Category</p>
                      <span className="inline-block bg-gray-100 text-gray-700 px-2 py-1 rounded text-xs font-medium">
                        {ticket.categoryName}
                      </span>
                    </div>
                  )}

                  {ticket.tags && ticket.tags.length > 0 && (
                    <div>
                      <p className="text-xs font-medium text-gray-500 uppercase mb-2">Tags</p>
                      <div className="flex flex-wrap gap-2">
                        {ticket.tags.map((tag) => (
                          <span key={tag} className="inline-block bg-blue-50 text-blue-700 border border-blue-200 px-2 py-1 rounded text-xs">
                            {tag}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}

                  {ticket.resolutionNotes && (
                    <div className="border-t border-gray-200 pt-4">
                      <p className="text-xs font-medium text-gray-500 uppercase mb-2">Resolution Notes</p>
                      <p className="text-sm text-gray-700 whitespace-pre-wrap bg-gray-50 p-3 rounded">
                        {ticket.resolutionNotes}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Assign Dialog */}
      {showAssignDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              {ticket.assignedToName ? 'Reassign Ticket' : 'Assign Ticket'}
            </h3>
            <form onSubmit={handleAssignTicket}>
              <div className="mb-4">
                <label htmlFor="agent" className="block text-sm font-medium text-gray-700 mb-2">
                  Select Agent
                </label>
                <input
                  type="text"
                  id="agent"
                  value={selectedAgentId}
                  onChange={(e) => setSelectedAgentId(e.target.value)}
                  placeholder="Enter agent ID"
                  className="block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 text-sm"
                />
                <p className="text-xs text-gray-500 mt-1">Note: Agent selection dropdown will be implemented in Phase 5</p>
              </div>
              <div className="flex gap-3 justify-end">
                <button
                  type="button"
                  onClick={() => setShowAssignDialog(false)}
                  className="px-4 py-2 bg-white border border-gray-300 text-gray-700 text-sm font-medium rounded-md hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={assignTicketMutation.isPending || !selectedAgentId}
                  className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {assignTicketMutation.isPending ? 'Assigning...' : 'Assign'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Close Dialog */}
      {showCloseDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Close Ticket</h3>
            <form onSubmit={handleCloseTicket}>
              <div className="mb-4">
                <label htmlFor="resolution" className="block text-sm font-medium text-gray-700 mb-2">
                  Resolution Notes *
                </label>
                <textarea
                  id="resolution"
                  value={resolutionNotes}
                  onChange={(e) => setResolutionNotes(e.target.value)}
                  rows={4}
                  minLength={10}
                  maxLength={5000}
                  required
                  placeholder="Describe how this ticket was resolved..."
                  className="block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 text-sm"
                />
                <p className="text-xs text-gray-500 mt-1">{resolutionNotes.length}/5,000 characters (min 10)</p>
              </div>
              <div className="bg-yellow-50 border border-yellow-200 rounded p-3 mb-4">
                <p className="text-sm text-yellow-800">
                  Closing this ticket will change its status to Closed and it will no longer accept updates.
                </p>
              </div>
              <div className="flex gap-3 justify-end">
                <button
                  type="button"
                  onClick={() => setShowCloseDialog(false)}
                  className="px-4 py-2 bg-white border border-gray-300 text-gray-700 text-sm font-medium rounded-md hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={closeTicketMutation.isPending || resolutionNotes.trim().length < 10}
                  className="px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-md hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {closeTicketMutation.isPending ? 'Closing...' : 'Close Ticket'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </AuthGuard>
  );
}
