'use client';

import { useMyTickets } from '@/hooks/use-tickets';
import { AuthGuard } from '@/components/auth-guard';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { getStatusColor, getPriorityColor, formatDate } from '@/lib/ticket-utils';
import { Pagination, usePagination } from '@/components/ui/pagination';

export default function TicketsPage() {
  const { page, pageSize, setPage, setPageSize } = usePagination();
  const { data, isLoading, error } = useMyTickets({ page, pageSize });
  const router = useRouter();

  const tickets = data?.items ?? [];
  const hasTickets = tickets.length > 0;

  return (
    <AuthGuard>
      <div className="min-h-screen bg-gray-50">
        <nav className="bg-white shadow-sm" aria-label="Page header">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between h-16">
              <div className="flex items-center">
                <Link href="/dashboard" className="text-2xl font-bold text-gray-900" aria-label="Go to dashboard">
                  Hickory Help Desk
                </Link>
              </div>
              <div className="flex items-center space-x-4">
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

        <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="px-4 py-6 sm:px-0">
            <div className="flex justify-between items-center mb-6">
              <h1 className="text-3xl font-bold text-gray-900">My Tickets</h1>
              <Link
                href="/tickets/new"
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                aria-label="Create new ticket"
              >
                Create Ticket
              </Link>
            </div>

            {isLoading && (
              <div className="text-center py-12" role="status" aria-label="Loading tickets">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" aria-hidden="true"></div>
                <p className="mt-4 text-gray-600">Loading tickets...</p>
              </div>
            )}

            {error && (
              <div className="bg-red-50 border border-red-200 rounded-md p-4" role="alert">
                <p className="text-red-800">
                  Error loading tickets: {error.message}
                </p>
              </div>
            )}

            {data && !hasTickets && (
              <div className="bg-white shadow rounded-lg p-12 text-center">
                <p className="text-gray-500 text-lg mb-4">
                  You haven't created any tickets yet.
                </p>
                <Link
                  href="/tickets/new"
                  className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
                  aria-label="Create your first ticket"
                >
                  Create Your First Ticket
                </Link>
              </div>
            )}

            {data && hasTickets && (
              <div className="space-y-4">
                <div className="bg-white shadow rounded-lg overflow-hidden">
                  <table className="min-w-full divide-y divide-gray-200" aria-label="Tickets table">
                    <thead className="bg-gray-50">
                      <tr>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Ticket
                        </th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Status
                        </th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Priority
                        </th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Created
                        </th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Comments
                        </th>
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                      {tickets.map((ticket) => (
                        <tr
                          key={ticket.id}
                          onClick={() => router.push(`/tickets/${ticket.id}`)}
                          className="hover:bg-gray-50 cursor-pointer"
                          role="button"
                          tabIndex={0}
                          onKeyDown={(e) => {
                            if (e.key === 'Enter' || e.key === ' ') {
                              e.preventDefault();
                              router.push(`/tickets/${ticket.id}`);
                            }
                          }}
                          aria-label={`View ticket ${ticket.ticketNumber}: ${ticket.title}`}
                        >
                          <td className="px-6 py-4">
                            <div className="flex flex-col">
                              <span className="text-sm font-medium text-blue-600">
                                {ticket.ticketNumber}
                              </span>
                              <span className="text-sm text-gray-900 font-medium mt-1">
                                {ticket.title}
                              </span>
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span
                              className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusColor(
                                ticket.status
                              )}`}
                            >
                              {ticket.status}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span
                              className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getPriorityColor(
                                ticket.priority
                              )}`}
                            >
                              {ticket.priority}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            <time dateTime={ticket.createdAt}>{formatDate(ticket.createdAt)}</time>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            <span aria-label={`${ticket.commentCount} comments`}>{ticket.commentCount}</span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>

                <Pagination
                  page={page}
                  pageSize={pageSize}
                  totalCount={data.totalCount}
                  totalPages={data.totalPages}
                  onPageChange={setPage}
                  onPageSizeChange={setPageSize}
                  disabled={isLoading}
                />
              </div>
            )}
          </div>
        </main>
      </div>
    </AuthGuard>
  );
}
