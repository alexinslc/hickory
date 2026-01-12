'use client';

import { useAgentQueue } from '@/hooks/use-agent';
import { useAuth } from '@/hooks/use-auth';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';

const STATUS_COLORS = {
  Open: 'bg-blue-100 text-blue-800',
  InProgress: 'bg-yellow-100 text-yellow-800',
  OnHold: 'bg-orange-100 text-orange-800',
  Resolved: 'bg-green-100 text-green-800',
  Closed: 'bg-gray-100 text-gray-800',
  Cancelled: 'bg-red-100 text-red-800',
};

const PRIORITY_COLORS = {
  Low: 'bg-gray-100 text-gray-800',
  Medium: 'bg-blue-100 text-blue-800',
  High: 'bg-orange-100 text-orange-800',
  Critical: 'bg-red-100 text-red-800',
};

export default function AgentQueuePage() {
  const router = useRouter();
  const { user, isLoading: authLoading } = useAuth();
  const { data: tickets, isLoading, error, refetch } = useAgentQueue();
  const [filter, setFilter] = useState<'all' | 'unassigned' | 'mine'>('all');

  // Check if user is agent or admin
  useEffect(() => {
    if (!authLoading && user && user.role !== 'Agent' && user.role !== 'Administrator') {
      router.push('/tickets');
    }
  }, [user, authLoading, router]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="text-center">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent mx-auto mb-4"></div>
          <p className="text-gray-600">Loading queue...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 mb-4">Failed to load agent queue</p>
          <button
            onClick={() => refetch()}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  const filteredTickets = tickets?.filter(ticket => {
    if (filter === 'unassigned') return !ticket.assignedToId;
    if (filter === 'mine') return ticket.assignedToId === user?.userId;
    return true;
  }) || [];

  const unassignedCount = tickets?.filter(t => !t.assignedToId).length || 0;
  const myTicketsCount = tickets?.filter(t => t.assignedToId === user?.userId).length || 0;

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">All Tickets</h1>
          <p className="mt-2 text-gray-600">
            Manage and respond to support tickets
          </p>
        </div>

        {/* Filter tabs */}
        <div className="mb-6 border-b border-gray-200">
          <nav className="-mb-px flex space-x-8">
            <button
              onClick={() => setFilter('all')}
              className={`whitespace-nowrap border-b-2 px-1 py-4 text-sm font-medium ${
                filter === 'all'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'
              }`}
            >
              All Tickets
              <span className="ml-2 rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-900">
                {tickets?.length || 0}
              </span>
            </button>
            <button
              onClick={() => setFilter('unassigned')}
              className={`whitespace-nowrap border-b-2 px-1 py-4 text-sm font-medium ${
                filter === 'unassigned'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'
              }`}
            >
              Unassigned
              <span className="ml-2 rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800">
                {unassignedCount}
              </span>
            </button>
            <button
              onClick={() => setFilter('mine')}
              className={`whitespace-nowrap border-b-2 px-1 py-4 text-sm font-medium ${
                filter === 'mine'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'
              }`}
            >
              My Tickets
              <span className="ml-2 rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-medium text-blue-900">
                {myTicketsCount}
              </span>
            </button>
          </nav>
        </div>

        {/* Tickets table */}
        {filteredTickets.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg shadow">
            <p className="text-gray-500">No tickets in this view</p>
          </div>
        ) : (
          <div className="bg-white shadow overflow-hidden sm:rounded-lg">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Ticket
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Priority
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Submitter
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Assigned To
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Age
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredTickets.map((ticket) => {
                  const age = Math.floor(
                    (Date.now() - new Date(ticket.createdAt).getTime()) / (1000 * 60 * 60)
                  );
                  return (
                    <tr key={ticket.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4">
                        <Link
                          href={`/agent/tickets/${ticket.id}`}
                          className="text-blue-600 hover:text-blue-800 hover:underline"
                        >
                          <div className="text-sm font-medium">{ticket.ticketNumber}</div>
                          <div className="text-sm text-gray-500 line-clamp-1">{ticket.title}</div>
                        </Link>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span
                          className={`inline-flex rounded-full px-2 text-xs font-semibold leading-5 ${
                            PRIORITY_COLORS[ticket.priority as keyof typeof PRIORITY_COLORS]
                          }`}
                        >
                          {ticket.priority}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span
                          className={`inline-flex rounded-full px-2 text-xs font-semibold leading-5 ${
                            STATUS_COLORS[ticket.status as keyof typeof STATUS_COLORS]
                          }`}
                        >
                          {ticket.status}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {ticket.submitterName}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {ticket.assignedToName || (
                          <span className="text-red-600 font-medium">Unassigned</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {age < 1 ? '< 1h' : `${age}h`}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
