'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { NotificationCenter } from '@/components/notifications/NotificationCenter';
import { ThemeToggle } from '@/components/ui/theme-toggle';
import { useAuthStore } from '@/store/auth-store';
import { LogOut } from 'lucide-react';

export function Navigation() {
  const router = useRouter();
  const [searchQuery, setSearchQuery] = useState('');
  const { user, clearAuth } = useAuthStore();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      router.push(`/search?q=${encodeURIComponent(searchQuery.trim())}`);
      setSearchQuery('');
    }
  };

  const handleLogout = () => {
    clearAuth();
    router.push('/auth/login');
  };

  const isAgent = user?.role === 'Agent' || user?.role === 'Administrator';

  return (
    <nav className="bg-white dark:bg-gray-900 shadow-sm border-b border-gray-200 dark:border-gray-700" aria-label="Main navigation">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          {/* Logo and main nav */}
          <div className="flex">
            <Link href="/dashboard" className="flex items-center px-2 text-xl font-bold text-blue-600 dark:text-blue-400" aria-label="Go to dashboard">
              Hickory
            </Link>
            <div className="hidden sm:ml-6 sm:flex sm:space-x-8" role="navigation" aria-label="Primary">
              <Link
                href="/tickets"
                className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-900 dark:text-gray-100 hover:text-blue-600 dark:hover:text-blue-400"
                aria-label="View my tickets"
              >
                My Tickets
              </Link>
              {isAgent && (
                <Link
                  href="/agent/queue"
                  className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-500 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100"
                  aria-label="View all tickets"
                >
                  All Tickets
                </Link>
              )}
              <Link
                href="/knowledge-base"
                className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-500 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100"
                aria-label="Browse knowledge base"
              >
                Knowledge Base
              </Link>
              <Link
                href="/search"
                className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-500 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100"
                aria-label="Search tickets"
              >
                Search
              </Link>
              <Link
                href="/settings/notifications"
                className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-500 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100"
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
                    className="h-5 w-5 text-gray-400 dark:text-gray-500"
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
                  className="block w-full rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 pl-10 pr-3 py-2 text-sm text-gray-900 dark:text-gray-100 placeholder-gray-500 dark:placeholder-gray-400 focus:border-blue-500 focus:ring-blue-500"
                  aria-label="Search for tickets"
                />
              </div>
            </form>
          </div>

          {/* User menu */}
          <div className="flex items-center gap-4">
            <ThemeToggle />
            <NotificationCenter />
            <Link
              href="/tickets/new"
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 dark:focus:ring-offset-gray-900"
              aria-label="Create new ticket"
            >
              New Ticket
            </Link>
            <button
              onClick={handleLogout}
              className="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-gray-500 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 rounded-md hover:bg-gray-100 dark:hover:bg-gray-800"
              aria-label="Log out"
            >
              <LogOut className="h-4 w-4" />
              <span className="hidden sm:inline">Logout</span>
            </button>
          </div>
        </div>
      </div>

      {/* Mobile menu */}
      <div className="sm:hidden border-t border-gray-200 dark:border-gray-700" role="navigation" aria-label="Mobile navigation">
        <div className="pt-2 pb-3 space-y-1">
          <Link
            href="/tickets"
            className="block pl-3 pr-4 py-2 text-base font-medium text-gray-900 dark:text-gray-100 hover:bg-gray-50 dark:hover:bg-gray-800"
            aria-label="View my tickets"
          >
            My Tickets
          </Link>
          {isAgent && (
            <Link
              href="/agent/queue"
              className="block pl-3 pr-4 py-2 text-base font-medium text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-800"
              aria-label="View all tickets"
            >
              All Tickets
            </Link>
          )}
          <Link
            href="/knowledge-base"
            className="block pl-3 pr-4 py-2 text-base font-medium text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-800"
            aria-label="Browse knowledge base"
          >
            Knowledge Base
          </Link>
          <Link
            href="/search"
            className="block pl-3 pr-4 py-2 text-base font-medium text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-800"
            aria-label="Search tickets"
          >
            Search
          </Link>
          <button
            onClick={handleLogout}
            className="w-full text-left pl-3 pr-4 py-2 text-base font-medium text-red-600 dark:text-red-400 hover:bg-gray-50 dark:hover:bg-gray-800"
            aria-label="Log out"
          >
            Logout
          </button>
        </div>
      </div>
    </nav>
  );
}
