'use client';

import { useState, useRef, DragEvent, ChangeEvent } from 'react';
import { useCreateTicketWithAttachments } from '@/hooks/use-tickets';
import { AuthGuard } from '@/components/auth-guard';
import Link from 'next/link';
import { Upload, X, File, Image } from 'lucide-react';

const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
const ALLOWED_FILE_TYPES = [
  'image/jpeg',
  'image/jpg',
  'image/png',
  'image/gif',
  'image/webp',
  'application/pdf',
  'application/msword',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  'application/vnd.ms-excel',
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  'text/plain',
  'text/csv',
  'application/zip',
];

export default function NewTicketPage() {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState('Medium');
  const [touched, setTouched] = useState({ title: false, description: false });
  const [pendingFiles, setPendingFiles] = useState<File[]>([]);
  const [isDragging, setIsDragging] = useState(false);
  const [fileError, setFileError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  
  const createTicket = useCreateTicketWithAttachments();

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
        files: pendingFiles,
      });
    }
  };

  const titleValid = title.length >= 5 && title.length <= 200;
  const descriptionValid = description.length >= 10 && description.length <= 10000;
  const isValid = titleValid && descriptionValid;

  // File handling functions
  const validateFile = (file: File): string | null => {
    if (file.size > MAX_FILE_SIZE) {
      return `File "${file.name}" exceeds 10MB limit`;
    }
    if (!ALLOWED_FILE_TYPES.includes(file.type)) {
      return `File type "${file.type}" is not allowed`;
    }
    return null;
  };

  const handleFiles = (files: FileList | null) => {
    if (!files) return;
    setFileError(null);

    const validFiles: File[] = [];
    const errors: string[] = [];
    for (const file of Array.from(files)) {
      const error = validateFile(file);
      if (error) {
        errors.push(error);
        continue;
      }
      // Avoid duplicates
      if (!pendingFiles.some((f) => f.name === file.name && f.size === file.size)) {
        validFiles.push(file);
      }
    }

    if (errors.length > 0) {
      if (errors.length === 1) {
        setFileError(errors[0]);
      } else {
        setFileError(`${errors.length} files failed validation. Please check file types and sizes.`);
      }
    }

    if (validFiles.length > 0) {
      setPendingFiles((prev) => [...prev, ...validFiles]);
    }
  };

  const removeFile = (file: File) => {
    setPendingFiles((prev) => prev.filter((f) => f !== file));
    setFileError(null);
  };

  const handleDragOver = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(false);
    handleFiles(e.dataTransfer.files);
  };

  const handleFileInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    handleFiles(e.target.files);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  };

  const getFileIcon = (file: File) => {
    if (file.type.startsWith('image/')) {
      return <Image className="w-4 h-4 text-gray-500" />;
    }
    return <File className="w-4 h-4 text-gray-500" />;
  };

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
        <nav className="bg-white shadow-sm" aria-label="Page header">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between h-16">
              <div className="flex items-center">
                <Link href="/dashboard" className="text-2xl font-bold text-gray-900" aria-label="Go to dashboard">
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
                <fieldset disabled={createTicket.isPending}>
                  <legend className="sr-only">New Ticket Form</legend>
                  <div className="space-y-6">
                    <div>
                      <label htmlFor="title" className="block text-sm font-medium text-gray-700">
                        Title *
                      </label>
                      <input
                        type="text"
                        id="title"
                        name="title"
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        onBlur={() => setTouched(prev => ({ ...prev, title: true }))}
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
                        <p id="title-error" className="mt-1 text-xs text-red-600" role="alert">
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
                        name="description"
                        rows={8}
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        onBlur={() => setTouched(prev => ({ ...prev, description: true }))}
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
                        <p id="description-error" className="mt-1 text-xs text-red-600" role="alert">
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
                        name="priority"
                        value={priority}
                        onChange={(e) => setPriority(e.target.value)}
                        className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-blue-500 sm:text-sm"
                        disabled={createTicket.isPending}
                        aria-describedby="priority-description"
                      >
                        <option value="Low">Low</option>
                        <option value="Medium">Medium</option>
                        <option value="High">High</option>
                        <option value="Critical">Critical</option>
                      </select>
                      <p id="priority-description" className="mt-1 text-xs text-gray-500">
                        Select the urgency of your issue
                      </p>
                    </div>

                    {/* Attachments Section */}
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Attachments (optional)
                      </label>
                      <div
                        className={`border-2 border-dashed rounded-lg p-6 text-center transition-colors ${
                          isDragging
                            ? 'border-blue-500 bg-blue-50'
                            : 'border-gray-300 hover:border-gray-400'
                        }`}
                        onDragOver={handleDragOver}
                        onDragLeave={handleDragLeave}
                        onDrop={handleDrop}
                      >
                        <Upload className="w-8 h-8 mx-auto mb-2 text-gray-400" />
                        <p className="text-sm text-gray-600 mb-1">
                          Drag and drop files here, or click to select
                        </p>
                        <p className="text-xs text-gray-500">
                          Maximum file size: 10MB • Supported: images, PDFs, documents
                        </p>
                        <input
                          ref={fileInputRef}
                          type="file"
                          multiple
                          className="hidden"
                          onChange={handleFileInputChange}
                          accept={ALLOWED_FILE_TYPES.join(',')}
                          disabled={createTicket.isPending}
                        />
                        <button
                          type="button"
                          onClick={() => fileInputRef.current?.click()}
                          disabled={createTicket.isPending}
                          className="mt-3 px-4 py-2 text-sm font-medium text-blue-600 bg-blue-50 rounded-md hover:bg-blue-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
                        >
                          Select Files
                        </button>
                      </div>

                      {fileError && (
                        <p className="mt-2 text-sm text-red-600">{fileError}</p>
                      )}

                      {/* Pending Files List */}
                      {pendingFiles.length > 0 && (
                        <div className="mt-4 space-y-2">
                          <p className="text-sm font-medium text-gray-700">
                            Files to upload ({pendingFiles.length})
                          </p>
                          {pendingFiles.map((file, index) => (
                            <div
                              key={`${file.name}-${index}`}
                              className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                            >
                              <div className="flex items-center space-x-3">
                                {getFileIcon(file)}
                                <div>
                                  <p className="text-sm font-medium text-gray-900 truncate max-w-xs">
                                    {file.name}
                                  </p>
                                  <p className="text-xs text-gray-500">
                                    {formatFileSize(file.size)}
                                  </p>
                                </div>
                              </div>
                              <button
                                type="button"
                                onClick={() => removeFile(file)}
                                disabled={createTicket.isPending}
                                className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
                                aria-label={`Remove ${file.name}`}
                              >
                                <X className="w-4 h-4" />
                              </button>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>

                    {createTicket.isError && (
                      <div className="rounded-md bg-red-50 border border-red-200 p-4" role="alert">
                        <div className="flex">
                          <div className="flex-shrink-0">
                            <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
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
                        aria-label="Cancel ticket creation"
                      >
                        Cancel
                      </Link>
                      <button
                        type="submit"
                        disabled={createTicket.isPending || !isValid}
                        className="inline-flex justify-center rounded-md border border-transparent bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
                        aria-label={createTicket.isPending ? 'Creating ticket' : 'Submit new ticket'}
                      >
                        {createTicket.isPending ? 'Creating...' : 'Create Ticket'}
                      </button>
                    </div>
                  </div>
                </fieldset>
              </form>
            </div>
          </div>
        </main>
      </div>
    </AuthGuard>
  );
}
