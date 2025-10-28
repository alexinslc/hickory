'use client';

import { Suspense, useState, useEffect } from 'react';
import { useSearchParams } from 'next/navigation';
import { useSearchTickets } from '@/lib/queries/search';
import { SearchInput } from '@/components/search/SearchInput';
import { SearchFilters } from '@/components/search/SearchFilters';
import { SearchResults } from '@/components/search/SearchResults';

function SearchPageContent() {
  const searchParams = useSearchParams();
  const initialQuery = searchParams.get('q') || '';
  
  const [searchQuery, setSearchQuery] = useState(initialQuery);
  const [status, setStatus] = useState<string>();
  const [priority, setPriority] = useState<string>();
  const [createdAfter, setCreatedAfter] = useState<string>();
  const [createdBefore, setCreatedBefore] = useState<string>();
  const [page, setPage] = useState(1);
  const pageSize = 20;

  // Update search query when URL param changes
  useEffect(() => {
    const urlQuery = searchParams.get('q') || '';
    if (urlQuery && urlQuery !== searchQuery) {
      setSearchQuery(urlQuery);
      setPage(1);
    }
  }, [searchParams, searchQuery]);

  const { data, isLoading } = useSearchTickets(
    {
      q: searchQuery || undefined,
      status,
      priority,
      createdAfter,
      createdBefore,
      page,
      pageSize,
    },
    true // Always enabled
  );

  const handleClearFilters = () => {
    setStatus(undefined);
    setPriority(undefined);
    setCreatedAfter(undefined);
    setCreatedBefore(undefined);
    setPage(1);
  };

  const handlePageChange = (newPage: number) => {
    setPage(newPage);
    // Scroll to top of results
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Reset page when search params change
  const handleSearchChange = (value: string) => {
    setSearchQuery(value);
    setPage(1);
  };

  const handleFilterChange = (setter: (value: string | undefined) => void) => (value: string | undefined) => {
    setter(value);
    setPage(1);
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Search Tickets</h1>
        <p className="mt-2 text-sm text-gray-600">
          Find tickets using keywords, filters, and advanced search
        </p>
      </div>

      {/* Search input */}
      <div className="mb-6">
        <SearchInput 
          value={searchQuery} 
          onChange={handleSearchChange}
          placeholder="Search by ticket number, title, or description..."
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        {/* Filters sidebar */}
        <div className="lg:col-span-1">
          <SearchFilters
            status={status}
            priority={priority}
            createdAfter={createdAfter}
            createdBefore={createdBefore}
            onStatusChange={handleFilterChange(setStatus)}
            onPriorityChange={handleFilterChange(setPriority)}
            onCreatedAfterChange={handleFilterChange(setCreatedAfter)}
            onCreatedBeforeChange={handleFilterChange(setCreatedBefore)}
            onClearFilters={handleClearFilters}
          />
        </div>

        {/* Results */}
        <div className="lg:col-span-3">
          <SearchResults
            tickets={data?.tickets || []}
            totalCount={data?.totalCount || 0}
            page={data?.page || 1}
            pageSize={data?.pageSize || pageSize}
            totalPages={data?.totalPages || 0}
            isLoading={isLoading}
            onPageChange={handlePageChange}
          />
        </div>
      </div>
    </div>
  );
}

export default function SearchPage() {
  return (
    <Suspense fallback={<div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="animate-pulse">
        <div className="h-8 bg-gray-200 rounded w-1/4 mb-4"></div>
        <div className="h-4 bg-gray-200 rounded w-1/2 mb-8"></div>
        <div className="h-10 bg-gray-200 rounded w-full mb-6"></div>
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          <div className="lg:col-span-1 h-64 bg-gray-200 rounded"></div>
          <div className="lg:col-span-3 h-96 bg-gray-200 rounded"></div>
        </div>
      </div>
    </div>}>
      <SearchPageContent />
    </Suspense>
  );
}
