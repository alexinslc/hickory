/**
 * Utility functions for ticket display and formatting
 */

/**
 * Returns Tailwind CSS classes for status badge colors
 */
export function getStatusColor(status: string): string {
  switch (status.toLowerCase()) {
    case 'open':
      return 'bg-blue-100 text-blue-800';
    case 'inprogress':
      return 'bg-yellow-100 text-yellow-800';
    case 'resolved':
      return 'bg-green-100 text-green-800';
    case 'closed':
      return 'bg-gray-100 text-gray-800';
    case 'cancelled':
      return 'bg-red-100 text-red-800';
    default:
      return 'bg-gray-100 text-gray-800';
  }
}

/**
 * Returns Tailwind CSS classes for priority badge colors
 */
export function getPriorityColor(priority: string): string {
  switch (priority.toLowerCase()) {
    case 'critical':
      return 'bg-red-100 text-red-800';
    case 'high':
      return 'bg-orange-100 text-orange-800';
    case 'medium':
      return 'bg-yellow-100 text-yellow-800';
    case 'low':
      return 'bg-green-100 text-green-800';
    default:
      return 'bg-gray-100 text-gray-800';
  }
}

/**
 * Format options for date display
 */
type DateFormatType = 'short' | 'long';

/**
 * Formats a date string for display
 * @param dateString - ISO date string to format
 * @param format - 'short' for date only, 'long' for date with time
 */
export function formatDate(dateString: string, format: DateFormatType = 'short'): string {
  const date = new Date(dateString);
  
  if (format === 'long') {
    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: 'numeric',
      minute: 'numeric',
    }).format(date);
  }
  
  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  }).format(date);
}
