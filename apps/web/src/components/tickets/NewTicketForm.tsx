'use client';

import { useState } from 'react';
import { useCreateTicket } from '@/hooks/use-tickets';
import { useGetAllCategories } from '@/lib/queries/categories';
import { useRouter } from 'next/navigation';

interface NewTicketFormProps {
  onSuccess?: (ticketId: string) => void;
  onCancel?: () => void;
}

export function NewTicketForm({ onSuccess, onCancel }: NewTicketFormProps) {
  const router = useRouter();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState('Medium');
  const [categoryId, setCategoryId] = useState<string>('');
  const [touched, setTouched] = useState({ title: false, description: false });
  
  const createTicket = useCreateTicket();
  const { data: categories } = useGetAllCategories();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Mark all fields as touched
    setTouched({ title: true, description: true });
    
    // Only submit if valid
    if (isValid) {
      createTicket.mutate(
        {
          title,
          description,
          priority,
          categoryId: categoryId || undefined,
        },
        {
          onSuccess: (data) => {
            if (onSuccess) {
              onSuccess(data.id);
            } else {
              router.push(`/tickets/${data.id}`);
            }
          },
        }
      );
    }
  };

  const titleValid = title.length >= 5 && title.length <= 200;
  const descriptionValid = description.length >= 10 && description.length <= 10000;
  const isValid = titleValid && descriptionValid;

  const getTitleValidationMessage = () => {
    if (!touched.title || title.length === 0) return null;
    if (title.length < 5) return 'Title must be at least 5 characters';
    if (title.length > 200) return 'Title must be no more than 200 characters';
    return null;
  };

  const getDescriptionValidationMessage = () => {
    if (!touched.description || description.length === 0) return null;
    if (description.length < 10) return 'Description must be at least 10 characters';
    if (description.length > 10000) return 'Description must be no more than 10,000 characters';
    return null;
  };

  const titleError = getTitleValidationMessage();
  const descriptionError = getDescriptionValidationMessage();

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Title Field */}
      <div>
        <label htmlFor="title" className="block text-sm font-medium text-gray-700">
          Title <span className="text-red-500">*</span>
        </label>
        <input
          type="text"
          id="title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          onBlur={() => setTouched(prev => ({ ...prev, title: true }))}
          className={`mt-1 block w-full rounded-md border px-3 py-2 shadow-sm focus:outline-none focus:ring-2 sm:text-sm ${
            titleError
              ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
              : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500'
          }`}
          placeholder="Brief description of your issue"
          disabled={createTicket.isPending}
          required
          minLength={5}
          maxLength={200}
          aria-invalid={!!titleError}
          aria-describedby={titleError ? 'title-error' : 'title-description'}
        />
        {titleError ? (
          <p id="title-error" className="mt-1 text-xs text-red-600">
            {titleError}
          </p>
        ) : (
          <p id="title-description" className="mt-1 text-xs text-gray-500">
            {title.length}/200 characters (minimum 5)
          </p>
        )}
      </div>

      {/* Description Field */}
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700">
          Description <span className="text-red-500">*</span>
        </label>
        <textarea
          id="description"
          rows={8}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          onBlur={() => setTouched(prev => ({ ...prev, description: true }))}
          className={`mt-1 block w-full rounded-md border px-3 py-2 shadow-sm focus:outline-none focus:ring-2 sm:text-sm ${
            descriptionError
              ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
              : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500'
          }`}
          placeholder="Provide detailed information about your issue..."
          disabled={createTicket.isPending}
          required
          minLength={10}
          maxLength={10000}
          aria-invalid={!!descriptionError}
          aria-describedby={descriptionError ? 'description-error' : 'description-description'}
        />
        {descriptionError ? (
          <p id="description-error" className="mt-1 text-xs text-red-600">
            {descriptionError}
          </p>
        ) : (
          <p id="description-description" className="mt-1 text-xs text-gray-500">
            {description.length}/10,000 characters (minimum 10)
          </p>
        )}
      </div>

      {/* Priority Field */}
      <div>
        <label htmlFor="priority" className="block text-sm font-medium text-gray-700">
          Priority
        </label>
        <select
          id="priority"
          value={priority}
          onChange={(e) => setPriority(e.target.value)}
          className="mt-1 block w-full rounded-md border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 sm:text-sm"
          disabled={createTicket.isPending}
        >
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
          <option value="Critical">Critical</option>
        </select>
        <p className="mt-1 text-xs text-gray-500">
          Select the urgency level of your issue
        </p>
      </div>

      {/* Category Field */}
      <div>
        <label htmlFor="category" className="block text-sm font-medium text-gray-700">
          Category
        </label>
        <select
          id="category"
          value={categoryId}
          onChange={(e) => setCategoryId(e.target.value)}
          className="mt-1 block w-full rounded-md border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 sm:text-sm"
          disabled={createTicket.isPending}
        >
          <option value="">Select a category (optional)</option>
          {categories?.filter(c => c.isActive).map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </select>
        <p className="mt-1 text-xs text-gray-500">
          Help us route your ticket to the right team
        </p>
      </div>

      {/* Error Display */}
      {createTicket.isError && (
        <div className="rounded-md bg-red-50 p-4">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-red-800">
                Failed to create ticket
              </h3>
              <div className="mt-2 text-sm text-red-700">
                {createTicket.error instanceof Error ? createTicket.error.message : 'An unexpected error occurred'}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Action Buttons */}
      <div className="flex items-center justify-end space-x-3 pt-4 border-t border-gray-200">
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            disabled={createTicket.isPending}
          >
            Cancel
          </button>
        )}
        <button
          type="submit"
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          disabled={!isValid || createTicket.isPending}
        >
          {createTicket.isPending ? (
            <span className="flex items-center">
              <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Creating...
            </span>
          ) : (
            'Create Ticket'
          )}
        </button>
      </div>
    </form>
  );
}
