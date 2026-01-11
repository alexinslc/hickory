'use client';

import { Download, Trash2, File, Image as ImageIcon } from 'lucide-react';
import { apiClient, type AttachmentDto } from '@/lib/api-client';
import { useState } from 'react';

interface AttachmentListProps {
  attachments: AttachmentDto[];
  onDelete?: (attachmentId: string) => void;
  canDelete?: boolean;
}

export function AttachmentList({ attachments, onDelete, canDelete = false }: AttachmentListProps) {
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const handleDownload = async (attachment: AttachmentDto) => {
    try {
      await apiClient.downloadAttachment(attachment.id, attachment.fileName);
    } catch (error) {
      console.error('Failed to download attachment:', error);
    }
  };

  const handleDelete = async (attachmentId: string) => {
    if (!confirm('Are you sure you want to delete this attachment?')) {
      return;
    }

    setDeletingId(attachmentId);
    try {
      await apiClient.deleteAttachment(attachmentId);
      onDelete?.(attachmentId);
    } catch (error) {
      console.error('Failed to delete attachment:', error);
      alert('Failed to delete attachment');
    } finally {
      setDeletingId(null);
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  };

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
    }).format(date);
  };

  const getFileIcon = (contentType: string) => {
    if (contentType.startsWith('image/')) {
      return <ImageIcon className="w-5 h-5 text-blue-500" />;
    }
    return <File className="w-5 h-5 text-gray-500" />;
  };

  if (attachments.length === 0) {
    return (
      <div className="text-center py-6 text-gray-500 dark:text-gray-400 text-sm">
        No attachments
      </div>
    );
  }

  return (
    <div className="space-y-2">
      {attachments.map((attachment) => (
        <div
          key={attachment.id}
          className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-800 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
        >
          <div className="flex items-center space-x-3 flex-1 min-w-0">
            {getFileIcon(attachment.contentType)}
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-gray-900 dark:text-gray-100 truncate">
                {attachment.fileName}
              </p>
              <p className="text-xs text-gray-500 dark:text-gray-400">
                {formatFileSize(attachment.fileSizeBytes)} • {attachment.uploadedByName} •{' '}
                {formatDate(attachment.uploadedAt)}
              </p>
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <button
              type="button"
              onClick={() => handleDownload(attachment)}
              className="p-2 text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-gray-100 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-md transition-colors"
              title="Download"
            >
              <Download className="w-4 h-4" />
            </button>
            {canDelete && (
              <button
                type="button"
                onClick={() => handleDelete(attachment.id)}
                disabled={deletingId === attachment.id}
                className="p-2 text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-md transition-colors disabled:opacity-50"
                title="Delete"
              >
                <Trash2 className="w-4 h-4" />
              </button>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
