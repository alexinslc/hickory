'use client';

interface SearchFiltersProps {
  status?: string;
  priority?: string;
  createdAfter?: string;
  createdBefore?: string;
  onStatusChange: (status: string | undefined) => void;
  onPriorityChange: (priority: string | undefined) => void;
  onCreatedAfterChange: (date: string | undefined) => void;
  onCreatedBeforeChange: (date: string | undefined) => void;
  onClearFilters: () => void;
}

export function SearchFilters({
  status,
  priority,
  createdAfter,
  createdBefore,
  onStatusChange,
  onPriorityChange,
  onCreatedAfterChange,
  onCreatedBeforeChange,
  onClearFilters,
}: SearchFiltersProps) {
  const hasActiveFilters = status || priority || createdAfter || createdBefore;

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4 space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium text-gray-900">Filters</h3>
        {hasActiveFilters && (
          <button
            type="button"
            onClick={onClearFilters}
            className="text-sm text-indigo-600 hover:text-indigo-500"
          >
            Clear all
          </button>
        )}
      </div>

      {/* Status filter */}
      <div className="space-y-2">
        <label htmlFor="filter-status" className="block text-sm font-medium text-gray-700">
          Status
        </label>
        <select
          id="filter-status"
          value={status || ''}
          onChange={(e) => onStatusChange(e.target.value || undefined)}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
        >
          <option value="">All Statuses</option>
          <option value="Open">Open</option>
          <option value="InProgress">In Progress</option>
          <option value="Resolved">Resolved</option>
          <option value="Closed">Closed</option>
          <option value="Cancelled">Cancelled</option>
        </select>
      </div>

      {/* Priority filter */}
      <div className="space-y-2">
        <label htmlFor="filter-priority" className="block text-sm font-medium text-gray-700">
          Priority
        </label>
        <select
          id="filter-priority"
          value={priority || ''}
          onChange={(e) => onPriorityChange(e.target.value || undefined)}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
        >
          <option value="">All Priorities</option>
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
          <option value="Critical">Critical</option>
        </select>
      </div>

      {/* Date range filters */}
      <div className="space-y-2">
        <label htmlFor="filter-created-after" className="block text-sm font-medium text-gray-700">
          Created After
        </label>
        <input
          type="date"
          id="filter-created-after"
          value={createdAfter || ''}
          onChange={(e) => onCreatedAfterChange(e.target.value || undefined)}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
        />
      </div>

      <div className="space-y-2">
        <label htmlFor="filter-created-before" className="block text-sm font-medium text-gray-700">
          Created Before
        </label>
        <input
          type="date"
          id="filter-created-before"
          value={createdBefore || ''}
          onChange={(e) => onCreatedBeforeChange(e.target.value || undefined)}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
        />
      </div>
    </div>
  );
}
