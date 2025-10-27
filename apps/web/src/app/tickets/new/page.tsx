'use client';

import { useState } from 'react';
import { useCreateTicket } from '@/hooks/use-tickets';
import { AuthGuard } from '@/components/auth-guard';
import Link from 'next/link';

export default function NewTicketPage() {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState('Medium');
  const [touched, setTouched] = useState({ title: false, description: false });
  
  const createTicket = useCreateTicket();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Mark all fields as touched
    setTouched({ title: true, description: true });
    
    // Only submit if valid
    if (isValid) {
      createTicket.mutate({
        title,
        description,
        priority,
      });
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
    <AuthGuard>
      <div className="min-h-screen bg-gray-50">
        <nav className="bg-white shadow-sm">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between h-16">
              <div className="flex items-center">
                <Link href="/dashboard" className="text-2xl font-bold text-gray-900">
                  Hickory Help Desk
                </Link>
              </div>
              <div className="flex items-center space-x-4">
                <Link
                  href="/tickets"
                  className="text-gray-600 hover:text-gray-900"
                >
                  My Tickets
                </Link>
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

        <main className="max-w-3xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="px-4 py-6 sm:px-0">
            <div className="mb-6">
              <h1 className="text-3xl font-bold text-gray-900">Create New Ticket</h1>
              <p className="mt-2 text-sm text-gray-600">
                Describe your issue and we'll help you resolve it.
              </p>
            </div>

            <div className="bg-white shadow rounded-lg p-6">
              <form onSubmit={handleSubmit}>
                <div className="space-y-6">
                  <div>
                    <label htmlFor="title" className="block text-sm font-medium text-gray-700">
                      Title *
                    </label>
                    <input
                      type="text"
                      id="title"
                      value={title}
                      onChange={(e) => setTitle(e.target.value)}
                      onBlur={() => setTouched({ ...touched, title: true })}
                      className={`mt-1 block w-full rounded-md border px-3 py-2 shadow-sm focus:outline-none sm:text-sm ${
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

                  <div>
                    <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                      Description *
                    </label>
                    <textarea
                      id="description"
                      rows={8}
                      value={description}
                      onChange={(e) => setDescription(e.target.value)}
                      onBlur={() => setTouched({ ...touched, description: true })}
                      className={`mt-1 block w-full rounded-md border px-3 py-2 shadow-sm focus:outline-none sm:text-sm ${
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

                  <div>
                    <label htmlFor="priority" className="block text-sm font-medium text-gray-700">
                      Priority
                    </label>
                    <select
                      id="priority"
                      value={priority}
                      onChange={(e) => setPriority(e.target.value)}
                      className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-blue-500 sm:text-sm"
                      disabled={createTicket.isPending}
                    >
                      <option value="Low">Low</option>
                      <option value="Medium">Medium</option>
                      <option value="High">High</option>
                      <option value="Critical">Critical</option>
                    </select>
                    <p className="mt-1 text-xs text-gray-500">
                      Select the urgency of your issue
                    </p>
                  </div>

                  {createTicket.isError && (
                    <div className="rounded-md bg-red-50 border border-red-200 p-4">
                      <div className="flex">
                        <div className="flex-shrink-0">
                          <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                          </svg>
                        </div>
                        <div className="ml-3">
                          <h3 className="text-sm font-medium text-red-800">
                            Error creating ticket
                          </h3>
                          <div className="mt-2 text-sm text-red-700">
                            <p>{createTicket.error?.message || 'An unexpected error occurred. Please try again.'}</p>
                          </div>
                        </div>
                      </div>
                    </div>
                  )}

                  <div className="flex justify-end space-x-3">
                    <Link
                      href="/tickets"
                      className="inline-flex justify-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
                    >
                      Cancel
                    </Link>
                    <button
                      type="submit"
                      disabled={createTicket.isPending || !isValid}
                      className="inline-flex justify-center rounded-md border border-transparent bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {createTicket.isPending ? 'Creating...' : 'Create Ticket'}
                    </button>
                  </div>
                </div>
              </form>
            </div>
          </div>
        </main>
      </div>
    </AuthGuard>
  );
}
