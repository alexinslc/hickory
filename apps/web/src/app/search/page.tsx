'use client';

import { useState } from 'react';
import { useSearchTickets } from '@/lib/queries/search';
import { SearchInput } from '@/components/search/SearchInput';
import { SearchFilters } from '@/components/search/SearchFilters';
import { SearchResults } from '@/components/search/SearchResults';

export default function SearchPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [status, setStatus] = useState<string>();
  const [priority, setPriority] = useState<string>();
  const [createdAfter, setCreatedAfter] = useState<string>();
  const [createdBefore, setCreatedBefore] = useState<string>();
  const [page, setPage] = useState(1);
  const pageSize = 20;

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
