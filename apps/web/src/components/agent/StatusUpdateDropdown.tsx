'use client';

import { useState } from 'react';
import { TicketDto } from '@/lib/api-client';

interface StatusUpdateDropdownProps {
  ticket: TicketDto;
  onUpdate: (newStatus: string, rowVersion: string) => Promise<void>;
  disabled?: boolean;
}

const STATUS_OPTIONS = [
  { value: 'Open', label: 'Open', color: 'bg-blue-100 text-blue-800' },
  { value: 'InProgress', label: 'In Progress', color: 'bg-yellow-100 text-yellow-800' },
  { value: 'Resolved', label: 'Resolved', color: 'bg-green-100 text-green-800' },
  { value: 'Closed', label: 'Closed', color: 'bg-gray-100 text-gray-800' },
  { value: 'Cancelled', label: 'Cancelled', color: 'bg-red-100 text-red-800' },
];

export function StatusUpdateDropdown({ ticket, onUpdate, disabled = false }: StatusUpdateDropdownProps) {
  const [isUpdating, setIsUpdating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const currentStatus = STATUS_OPTIONS.find(s => s.value === ticket.status);

  const handleStatusChange = async (newStatus: string) => {
    if (newStatus === ticket.status || disabled || isUpdating) return;

    setIsUpdating(true);
    setError(null);

    try {
      await onUpdate(newStatus, ticket.rowVersion);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update status');
      // Error will be shown in a toast or notification
      console.error('Failed to update ticket status:', err);
    } finally {
      setIsUpdating(false);
    }
  };

  return (
    <div className="relative">
      <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">
        Status
      </label>
      <select
        id="status"
        value={ticket.status}
        onChange={(e) => handleStatusChange(e.target.value)}
        disabled={disabled || isUpdating}
        className={`block w-full rounded-md border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 sm:text-sm ${
          currentStatus?.color || ''
        } disabled:opacity-50 disabled:cursor-not-allowed`}
      >
        {STATUS_OPTIONS.map((status) => (
          <option key={status.value} value={status.value}>
            {status.label}
          </option>
        ))}
      </select>
      
      {isUpdating && (
        <div className="absolute right-3 top-9 pointer-events-none">
          <svg className="animate-spin h-4 w-4 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
        </div>
      )}

      {error && (
        <p className="mt-1 text-xs text-red-600">{error}</p>
      )}
    </div>
  );
}
