'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useGetTicketById, useUpdateTicketStatus, useAssignTicket, useCloseTicket } from '@/lib/queries/tickets';
import { useAddComment, useAddInternalNote } from '@/lib/queries/comments';
import { useAuthStore } from '@/store/auth-store';
import { AuthGuard } from '@/components/auth-guard';
import { CommentForm } from '@/components/tickets/CommentForm';
import { AssignTicketDialog } from '@/components/agent/AssignTicketDialog';
import { StatusUpdateDropdown } from '@/components/agent/StatusUpdateDropdown';
import { CloseTicketDialog } from '@/components/agent/CloseTicketDialog';
import { InternalNoteForm } from '@/components/agent/InternalNoteForm';

// Utility functions for status and priority colors
function getStatusColor(status: string): string {
  const colors: Record<string, string> = {
    Open: 'bg-blue-100 text-blue-800 hover:bg-blue-200',
    InProgress: 'bg-yellow-100 text-yellow-800 hover:bg-yellow-200',
    Resolved: 'bg-green-100 text-green-800 hover:bg-green-200',
    Closed: 'bg-gray-100 text-gray-800 hover:bg-gray-200',
    Cancelled: 'bg-red-100 text-red-800 hover:bg-red-200',
  };
  return colors[status] || 'bg-gray-100 text-gray-800 hover:bg-gray-200';
}

function getPriorityColor(priority: string): string {
  const colors: Record<string, string> = {
    Low: 'bg-slate-100 text-slate-800 hover:bg-slate-200',
    Medium: 'bg-blue-100 text-blue-800 hover:bg-blue-200',
    High: 'bg-orange-100 text-orange-800 hover:bg-orange-200',
    Critical: 'bg-red-100 text-red-800 hover:bg-red-200',
  };
  return colors[priority] || 'bg-gray-100 text-gray-800 hover:bg-gray-200';
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
  const { user } = useAuthStore();
  
  const [showAssignDialog, setShowAssignDialog] = useState(false);
  const [showCloseDialog, setShowCloseDialog] = useState(false);
  
  // Check if user is an agent
  const isAgent = user?.role === 'Agent' || user?.role === 'Admin';

  // Check if user is an agent
  const isAgent = user?.role === 'Agent' || user?.role === 'Admin';

  const handleCommentSubmit = async (content: string, isInternal: boolean) => {
    // The CommentForm component handles the mutation internally
    console.log('Comment submitted:', { content, isInternal });
  };

  if (isLoading) {
    return (
      <AuthGuard>
        <div className="min-h-screen bg-background">
          <div className="container mx-auto py-8">
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
            </div>
          </div>
        </div>
      </AuthGuard>
    );
  }

  if (error) {
    return (
      <AuthGuard>
        <div className="min-h-screen bg-background">
          <div className="container mx-auto py-8">
            <Card className="border-destructive">
              <CardHeader>
                <CardTitle className="text-destructive">Error Loading Ticket</CardTitle>
                <CardDescription>{error.message}</CardDescription>
              </CardHeader>
            </Card>
          </div>
        </div>
      </AuthGuard>
    );
  }

  if (!ticket) {
    return (
      <AuthGuard>
        <div className="min-h-screen bg-background">
          <div className="container mx-auto py-8">
            <Card>
              <CardHeader>
                <CardTitle>Ticket Not Found</CardTitle>
                <CardDescription>The requested ticket could not be found.</CardDescription>
              </CardHeader>
            </Card>
          </div>
        </div>
      </AuthGuard>
    );
  }

  return (
    <AuthGuard>
      <div className="min-h-screen bg-background">
        {/* Header */}
        <div className="border-b">
          <div className="container mx-auto px-4 py-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-4">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => router.back()}
                >
                  <ArrowLeft className="h-4 w-4 mr-2" />
                  Back
                </Button>
                <Separator orientation="vertical" className="h-6" />
                <div>
                  <p className="text-sm text-muted-foreground">{ticket.ticketNumber}</p>
                  <h1 className="text-2xl font-bold">{ticket.title}</h1>
                </div>
              </div>
              <div className="flex items-center gap-2">
                <Badge className={getPriorityColor(ticket.priority)}>
                  {ticket.priority}
                </Badge>
                {isAgent ? (
                  <StatusUpdateDropdown
                    ticketId={ticket.id}
                    currentStatus={ticket.status}
                    rowVersion={ticket.rowVersion}
                  />
                ) : (
                  <Badge className={getStatusColor(ticket.status)}>
                    {ticket.status}
                  </Badge>
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
              <Card>
                <CardHeader>
                  <CardTitle>Description</CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="whitespace-pre-wrap text-sm">{ticket.description}</p>
                </CardContent>
              </Card>

              {/* Comments Section */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <MessageSquare className="h-5 w-5" />
                    Activity
                    <Badge variant="secondary" className="ml-2">
                      {ticket.commentCount}
                    </Badge>
                  </CardTitle>
                  <CardDescription>
                    Add comments to communicate about this ticket
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-6">
                  {/* Comment Form */}
                  <CommentForm
                    ticketId={ticket.id}
                    isAgent={isAgent}
                    onSubmit={handleCommentSubmit}
                  />

                  <Separator />

                  {/* Comment History Placeholder */}
                  <div className="text-center py-8 bg-muted/50 rounded-lg">
                    <MessageSquare className="h-12 w-12 mx-auto text-muted-foreground mb-2" />
                    <p className="font-medium text-sm text-muted-foreground mb-1">
                      Comment History
                    </p>
                    <p className="text-xs text-muted-foreground max-w-md mx-auto">
                      A separate API endpoint to fetch comments will be added in a future phase.
                      Comments you add above are being saved successfully.
                    </p>
                  </div>
                </CardContent>
              </Card>

              {/* Internal Note Form - Agent Only */}
              {isAgent && (
                <Card className="border-yellow-200 bg-yellow-50/50">
                  <CardHeader>
                    <CardTitle className="text-yellow-900">Internal Notes</CardTitle>
                    <CardDescription className="text-yellow-700">
                      Notes visible only to agents and admins
                    </CardDescription>
                  </CardHeader>
                  <CardContent>
                    <InternalNoteForm ticketId={ticket.id} />
                  </CardContent>
                </Card>
              )}
            </div>

            {/* Right Column - Sidebar */}
            <div className="space-y-6">
              {/* Agent Actions */}
              {isAgent && (
                <Card>
                  <CardHeader>
                    <CardTitle>Agent Actions</CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    <Button
                      variant="outline"
                      className="w-full justify-start"
                      onClick={() => setShowAssignDialog(true)}
                      disabled={ticket.status === 'Closed' || ticket.status === 'Cancelled'}
                    >
                      <UserCircle className="h-4 w-4 mr-2" />
                      {ticket.assignedToName ? 'Reassign Ticket' : 'Assign Ticket'}
                    </Button>

                    <Button
                      variant="outline"
                      className="w-full justify-start"
                      onClick={() => setShowCloseDialog(true)}
                      disabled={ticket.status === 'Closed' || ticket.status === 'Cancelled'}
                    >
                      <Clock className="h-4 w-4 mr-2" />
                      Close Ticket
                    </Button>
                  </CardContent>
                </Card>
              )}

              {/* Ticket Details */}
              <Card>
                <CardHeader>
                  <CardTitle>Details</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1">
                      <User className="h-4 w-4" />
                      <span>Submitted by</span>
                    </div>
                    <p className="text-sm font-medium">{ticket.submitterName}</p>
                  </div>

                  {ticket.assignedToName && (
                    <div>
                      <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1">
                        <UserCircle className="h-4 w-4" />
                        <span>Assigned to</span>
                      </div>
                      <p className="text-sm font-medium">{ticket.assignedToName}</p>
                    </div>
                  )}

                  <Separator />

                  <div>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1">
                      <Calendar className="h-4 w-4" />
                      <span>Created</span>
                    </div>
                    <p className="text-sm font-medium">{formatDate(ticket.createdAt, 'long')}</p>
                  </div>

                  <div>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1">
                      <Clock className="h-4 w-4" />
                      <span>Last updated</span>
                    </div>
                    <p className="text-sm font-medium">{formatDate(ticket.updatedAt, 'long')}</p>
                  </div>

                  {ticket.closedAt && (
                    <div>
                      <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1">
                        <Clock className="h-4 w-4" />
                        <span>Closed</span>
                      </div>
                      <p className="text-sm font-medium">{formatDate(ticket.closedAt, 'long')}</p>
                    </div>
                  )}

                  {ticket.categoryName && (
                    <>
                      <Separator />
                      <div>
                        <div className="flex items-center gap-2 text-sm text-muted-foreground mb-1">
                          <Tag className="h-4 w-4" />
                          <span>Category</span>
                        </div>
                        <Badge variant="secondary">{ticket.categoryName}</Badge>
                      </div>
                    </>
                  )}

                  {ticket.tags && ticket.tags.length > 0 && (
                    <div>
                      <div className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
                        <Tag className="h-4 w-4" />
                        <span>Tags</span>
                      </div>
                      <div className="flex flex-wrap gap-2">
                        {ticket.tags.map((tag) => (
                          <Badge key={tag} variant="outline" className="text-xs">
                            {tag}
                          </Badge>
                        ))}
                      </div>
                    </div>
                  )}

                  {ticket.resolutionNotes && (
                    <>
                      <Separator />
                      <div>
                        <p className="text-sm text-muted-foreground mb-2">Resolution Notes</p>
                        <p className="text-sm whitespace-pre-wrap bg-muted p-3 rounded-md">
                          {ticket.resolutionNotes}
                        </p>
                      </div>
                    </>
                  )}
                </CardContent>
              </Card>
            </div>
          </div>
        </div>
      </div>

      {/* Dialogs */}
      {isAgent && (
        <>
          <AssignTicketDialog
            ticketId={ticket.id}
            currentAssignee={ticket.assignedToName}
            ticketTitle={ticket.title}
            rowVersion={ticket.rowVersion}
            open={showAssignDialog}
            onOpenChange={setShowAssignDialog}
          />
          
          <CloseTicketDialog
            ticketId={ticket.id}
            ticketNumber={ticket.ticketNumber}
            ticketTitle={ticket.title}
            rowVersion={ticket.rowVersion}
            open={showCloseDialog}
            onOpenChange={setShowCloseDialog}
          />
        </>
      )}
    </AuthGuard>
  );
}
