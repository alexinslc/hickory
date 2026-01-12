'use client';

import * as React from 'react';
import { cn } from '@/lib/utils';

const PAGE_SIZE_OPTIONS = [10, 25, 50, 100] as const;

export interface PaginationProps {
  /** Current page number (1-indexed) */
  page: number;
  /** Number of items per page */
  pageSize: number;
  /** Total number of items */
  totalCount: number;
  /** Total number of pages */
  totalPages: number;
  /** Callback when page changes */
  onPageChange: (page: number) => void;
  /** Callback when page size changes */
  onPageSizeChange?: (pageSize: number) => void;
  /** Whether the pagination is disabled */
  disabled?: boolean;
  /** Additional CSS classes */
  className?: string;
  /** Show page size selector */
  showPageSizeSelector?: boolean;
  /** Whether to show first/last page buttons */
  showFirstLastButtons?: boolean;
  /** Aria label for the pagination navigation */
  ariaLabel?: string;
}

export function Pagination({
  page,
  pageSize,
  totalCount,
  totalPages,
  onPageChange,
  onPageSizeChange,
  disabled = false,
  className,
  showPageSizeSelector = true,
  showFirstLastButtons = true,
  ariaLabel = 'Pagination navigation',
}: PaginationProps) {
  const startResult = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const endResult = Math.min(page * pageSize, totalCount);

  const handleKeyDown = (e: React.KeyboardEvent, action: () => void) => {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      action();
    }
  };

  // Generate page numbers to display
  const getPageNumbers = (): (number | 'ellipsis')[] => {
    if (totalPages <= 7) {
      return Array.from({ length: totalPages }, (_, i) => i + 1);
    }

    const pages: (number | 'ellipsis')[] = [1];

    if (page > 3) {
      pages.push('ellipsis');
    }

    const start = Math.max(2, page - 1);
    const end = Math.min(totalPages - 1, page + 1);

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    if (page < totalPages - 2) {
      pages.push('ellipsis');
    }

    if (totalPages > 1) {
      pages.push(totalPages);
    }

    return pages;
  };

  if (totalCount === 0) {
    return null;
  }

  return (
    <div className={cn('flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between', className)}>
      {/* Results summary and page size selector */}
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:gap-4">
        <p className="text-sm text-gray-700" role="status" aria-live="polite">
          Showing <span className="font-medium">{startResult}</span> to{' '}
          <span className="font-medium">{endResult}</span> of{' '}
          <span className="font-medium">{totalCount}</span> result{totalCount !== 1 ? 's' : ''}
        </p>

        {showPageSizeSelector && onPageSizeChange && (
          <div className="flex items-center gap-2">
            <label htmlFor="page-size-select" className="text-sm text-gray-700">
              Items per page:
            </label>
            <select
              id="page-size-select"
              value={pageSize}
              onChange={(e) => onPageSizeChange(Number(e.target.value))}
              disabled={disabled}
              className="rounded-md border border-gray-300 bg-white px-2 py-1 text-sm text-gray-700 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 disabled:cursor-not-allowed disabled:opacity-50"
              aria-label="Select number of items per page"
            >
              {PAGE_SIZE_OPTIONS.map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </div>
        )}
      </div>

      {/* Pagination controls */}
      {totalPages > 1 && (
        <nav aria-label={ariaLabel}>
          {/* Mobile pagination */}
          <div className="flex justify-between sm:hidden">
            <button
              onClick={() => onPageChange(page - 1)}
              disabled={disabled || page === 1}
              className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              aria-label="Go to previous page"
            >
              Previous
            </button>
            <span className="flex items-center text-sm text-gray-700">
              Page {page} of {totalPages}
            </span>
            <button
              onClick={() => onPageChange(page + 1)}
              disabled={disabled || page === totalPages}
              className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              aria-label="Go to next page"
            >
              Next
            </button>
          </div>

          {/* Desktop pagination */}
          <div className="hidden sm:flex sm:items-center sm:gap-2">
            {/* First page button */}
            {showFirstLastButtons && (
              <button
                onClick={() => onPageChange(1)}
                disabled={disabled || page === 1}
                onKeyDown={(e) => handleKeyDown(e, () => onPageChange(1))}
                className="relative inline-flex items-center rounded-md p-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:opacity-50"
                aria-label="Go to first page"
              >
                <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path fillRule="evenodd" d="M15.79 14.77a.75.75 0 01-1.06.02L10 10.06 5.27 14.79a.75.75 0 01-1.06-1.06l5.25-5.25a.75.75 0 011.08 0l5.25 5.25a.75.75 0 01.02 1.06z" clipRule="evenodd" transform="rotate(90, 10, 10)" />
                  <path fillRule="evenodd" d="M5 5a.75.75 0 01.75-.75h8.5a.75.75 0 010 1.5h-8.5A.75.75 0 015 5z" clipRule="evenodd" transform="translate(0, 5)" />
                </svg>
              </button>
            )}

            {/* Previous button */}
            <button
              onClick={() => onPageChange(page - 1)}
              disabled={disabled || page === 1}
              onKeyDown={(e) => handleKeyDown(e, () => onPageChange(page - 1))}
              className="relative inline-flex items-center rounded-md p-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:opacity-50"
              aria-label="Go to previous page"
            >
              <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                <path fillRule="evenodd" d="M12.79 5.23a.75.75 0 01-.02 1.06L8.832 10l3.938 3.71a.75.75 0 11-1.04 1.08l-4.5-4.25a.75.75 0 010-1.08l4.5-4.25a.75.75 0 011.06.02z" clipRule="evenodd" />
              </svg>
            </button>

            {/* Page numbers */}
            <div className="isolate inline-flex -space-x-px rounded-md shadow-sm">
              {getPageNumbers().map((pageNum, index) =>
                pageNum === 'ellipsis' ? (
                  <span
                    key={`ellipsis-${index}`}
                    className="relative inline-flex items-center px-4 py-2 text-sm font-semibold text-gray-700 ring-1 ring-inset ring-gray-300"
                    aria-hidden="true"
                  >
                    …
                  </span>
                ) : (
                  <button
                    key={pageNum}
                    onClick={() => onPageChange(pageNum)}
                    disabled={disabled}
                    onKeyDown={(e) => handleKeyDown(e, () => onPageChange(pageNum))}
                    className={cn(
                      'relative inline-flex items-center px-4 py-2 text-sm font-semibold focus:z-20 focus:outline-none focus:ring-2 focus:ring-blue-500',
                      page === pageNum
                        ? 'z-10 bg-blue-600 text-white'
                        : 'text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50'
                    )}
                    aria-label={`Go to page ${pageNum}`}
                    aria-current={page === pageNum ? 'page' : undefined}
                  >
                    {pageNum}
                  </button>
                )
              )}
            </div>

            {/* Next button */}
            <button
              onClick={() => onPageChange(page + 1)}
              disabled={disabled || page === totalPages}
              onKeyDown={(e) => handleKeyDown(e, () => onPageChange(page + 1))}
              className="relative inline-flex items-center rounded-md p-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:opacity-50"
              aria-label="Go to next page"
            >
              <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                <path fillRule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clipRule="evenodd" />
              </svg>
            </button>

            {/* Last page button */}
            {showFirstLastButtons && (
              <button
                onClick={() => onPageChange(totalPages)}
                disabled={disabled || page === totalPages}
                onKeyDown={(e) => handleKeyDown(e, () => onPageChange(totalPages))}
                className="relative inline-flex items-center rounded-md p-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:opacity-50"
                aria-label="Go to last page"
              >
                <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path fillRule="evenodd" d="M5.23 4.21a.75.75 0 011.06.02L10 8.94l4.73-4.71a.75.75 0 111.06 1.06L10.54 10l5.25 5.25a.75.75 0 01-1.06 1.06L10 11.06l-4.73 4.71a.75.75 0 01-1.06-1.06L9.46 10 4.21 4.75a.75.75 0 01.02-1.06z" clipRule="evenodd" transform="rotate(90, 10, 10)" />
                  <path fillRule="evenodd" d="M5 15a.75.75 0 01.75-.75h8.5a.75.75 0 010 1.5h-8.5A.75.75 0 015 15z" clipRule="evenodd" transform="translate(0, -5)" />
                </svg>
              </button>
            )}
          </div>
        </nav>
      )}
    </div>
  );
}

/** Hook to manage pagination state */
export function usePagination(options?: {
  defaultPage?: number;
  defaultPageSize?: number;
}) {
  const { defaultPage = 1, defaultPageSize = 10 } = options ?? {};

  const [page, setPage] = React.useState(defaultPage);
  const [pageSize, setPageSizeState] = React.useState(defaultPageSize);

  const setPageSize = React.useCallback((newPageSize: number) => {
    setPageSizeState(newPageSize);
    setPage(1); // Reset to first page when changing page size
  }, []);

  const reset = React.useCallback(() => {
    setPage(defaultPage);
    setPageSizeState(defaultPageSize);
  }, [defaultPage, defaultPageSize]);

  return {
    page,
    pageSize,
    setPage,
    setPageSize,
    reset,
  };
}

export type { PaginationProps as PaginationComponentProps };
