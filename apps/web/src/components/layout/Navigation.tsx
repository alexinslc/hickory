'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { NotificationCenter } from '@/components/notifications/NotificationCenter';

export function Navigation() {
  const router = useRouter();
  const [searchQuery, setSearchQuery] = useState('');

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      router.push(`/search?q=${encodeURIComponent(searchQuery.trim())}`);
      setSearchQuery('');
    }
  };

  return (
    <nav className="bg-white shadow-sm border-b border-gray-200" aria-label="Main navigation">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          {/* Logo and main nav */}
          <div className="flex">
            <Link href="/" className="flex items-center px-2 text-xl font-bold text-indigo-600" aria-label="Hickory home">
              Hickory
            </Link>
            <div className="hidden sm:ml-6 sm:flex sm:space-x-8" role="navigation" aria-label="Primary">
              <Link
                href="/tickets"
                className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-900 hover:text-indigo-600"
                aria-label="View my tickets"
              >
                My Tickets
              </Link>
              <Link
                href="/agent/queue"
                className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-500 hover:text-gray-900"
                aria-label="View agent queue"
              >
                Agent Queue
              </Link>
              <Link
                href="/search"
                className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-500 hover:text-gray-900"
                aria-label="Search tickets"
              >
                Search
              </Link>
              <Link
                href="/settings/notifications"
                className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-500 hover:text-gray-900"
                aria-label="View settings"
              >
                Settings
              </Link>
            </div>
          </div>

          {/* Search bar */}
          <div className="flex items-center flex-1 max-w-md mx-4">
            <form onSubmit={handleSearch} className="w-full" role="search">
              <div className="relative">
                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                  <svg
                    className="h-5 w-5 text-gray-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                    aria-hidden="true"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                    />
                  </svg>
                </div>
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Search tickets..."
                  className="block w-full rounded-md border-gray-300 pl-10 pr-3 py-2 text-sm placeholder-gray-500 focus:border-indigo-500 focus:ring-indigo-500"
                  aria-label="Search for tickets"
                />
              </div>
            </form>
          </div>

          {/* User menu */}
          <div className="flex items-center gap-4">
            <NotificationCenter />
            <Link
              href="/tickets/create"
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              aria-label="Create new ticket"
            >
              New Ticket
            </Link>
          </div>
        </div>
      </div>

      {/* Mobile menu */}
      <div className="sm:hidden border-t border-gray-200" role="navigation" aria-label="Mobile navigation">
        <div className="pt-2 pb-3 space-y-1">
          <Link
            href="/tickets"
            className="block pl-3 pr-4 py-2 text-base font-medium text-gray-900 hover:bg-gray-50"
            aria-label="View my tickets"
          >
            My Tickets
          </Link>
          <Link
            href="/agent/queue"
            className="block pl-3 pr-4 py-2 text-base font-medium text-gray-500 hover:bg-gray-50"
            aria-label="View agent queue"
          >
            Agent Queue
          </Link>
          <Link
            href="/search"
            className="block pl-3 pr-4 py-2 text-base font-medium text-gray-500 hover:bg-gray-50"
            aria-label="Search tickets"
          >
            Search
          </Link>
        </div>
      </div>
    </nav>
  );
}
