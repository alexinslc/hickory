'use client';

import Link from 'next/link';
import { TicketDto } from '@/lib/api-client';
import { CategoryBadge, TagList } from './Badges';

interface TicketCardProps {
  ticket: TicketDto;
  showAssignee?: boolean;
}

export function TicketCard({ ticket, showAssignee = false }: TicketCardProps) {
  const statusColors: Record<string, string> = {
    New: 'bg-blue-100 text-blue-800',
    Open: 'bg-green-100 text-green-800',
    InProgress: 'bg-yellow-100 text-yellow-800',
    Resolved: 'bg-blue-100 text-blue-800',
    Closed: 'bg-gray-100 text-gray-800',
  };

  const priorityColors: Record<string, string> = {
    Low: 'bg-gray-100 text-gray-800',
    Medium: 'bg-blue-100 text-blue-800',
    High: 'bg-orange-100 text-orange-800',
    Critical: 'bg-red-100 text-red-800',
  };

  return (
    <Link
      href={`/tickets/${ticket.id}`}
      className="block rounded-lg border border-gray-200 bg-white p-4 shadow-sm hover:shadow-md transition-shadow"
    >
      <div className="flex items-start justify-between gap-4">
        <div className="flex-1 min-w-0">
          {/* Header with ticket number and title */}
          <div className="flex items-center gap-2 mb-2">
            <span className="text-xs font-mono text-gray-500">
              {ticket.ticketNumber}
            </span>
            <h3 className="text-lg font-semibold text-gray-900 truncate">
              {ticket.title}
            </h3>
          </div>

          {/* Description preview */}
          <p className="text-sm text-gray-600 line-clamp-2 mb-3">
            {ticket.description}
          </p>

          {/* Category and Tags */}
          <div className="flex flex-wrap items-center gap-2 mb-3">
            {ticket.categoryName && (
              <CategoryBadge name={ticket.categoryName} />
            )}
            {ticket.tags && ticket.tags.length > 0 && (
              <TagList tags={ticket.tags} />
            )}
          </div>

          {/* Metadata */}
          <div className="flex flex-wrap items-center gap-4 text-xs text-gray-500">
            <span>Submitted by {ticket.submitterName}</span>
            {showAssignee && ticket.assignedToName && (
              <span>Assigned to {ticket.assignedToName}</span>
            )}
            <span>{new Date(ticket.createdAt).toLocaleDateString()}</span>
            {ticket.commentCount > 0 && (
              <span>{ticket.commentCount} comment{ticket.commentCount !== 1 ? 's' : ''}</span>
            )}
          </div>
        </div>

        {/* Status and Priority badges */}
        <div className="flex flex-col gap-2">
          <span
            className={`inline-flex items-center rounded-md px-2.5 py-0.5 text-xs font-medium ${
              statusColors[ticket.status] || statusColors.New
            }`}
          >
            {ticket.status}
          </span>
          <span
            className={`inline-flex items-center rounded-md px-2.5 py-0.5 text-xs font-medium ${
              priorityColors[ticket.priority] || priorityColors.Medium
            }`}
          >
            {ticket.priority}
          </span>
        </div>
      </div>
    </Link>
  );
}
