'use client';

import { useState, useRef, DragEvent, ChangeEvent } from 'react';
import { Upload, X, File, Image, Loader2 } from 'lucide-react';
import { apiClient, type UploadAttachmentResponse } from '@/lib/api-client';

interface FileUploadProps {
  ticketId: string;
  onUploadComplete?: (attachment: UploadAttachmentResponse) => void;
  onError?: (error: string) => void;
}

interface FileWithProgress {
  file: File;
  progress: number;
  uploading: boolean;
  error?: string;
  completed?: boolean;
}

const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
const ALLOWED_FILE_TYPES = [
  'image/jpeg',
  'image/jpg',
  'image/png',
  'image/gif',
  'image/webp',
  'application/pdf',
  'application/msword',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document', // .docx
  'application/vnd.ms-excel',
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // .xlsx
  'text/plain',
  'text/csv',
  'application/zip',
];

export function FileUpload({ ticketId, onUploadComplete, onError }: FileUploadProps) {
  const [files, setFiles] = useState<FileWithProgress[]>([]);
  const [isDragging, setIsDragging] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const validateFile = (file: File): string | null => {
    if (file.size > MAX_FILE_SIZE) {
      return `File size must be less than 10MB`;
    }

    if (!ALLOWED_FILE_TYPES.includes(file.type) && file.type !== 'application/octet-stream') {
      return `File type ${file.type} is not allowed`;
    }

    return null;
  };

  const handleFiles = (newFiles: FileList | null) => {
    if (!newFiles) return;

    const validFiles: FileWithProgress[] = [];
    
    Array.from(newFiles).forEach((file) => {
      const error = validateFile(file);
      if (error) {
        onError?.(error);
        return;
      }

      validFiles.push({
        file,
        progress: 0,
        uploading: false,
      });
    });

    if (validFiles.length > 0) {
      setFiles((prev) => {
        const newFileList = [...prev, ...validFiles];
        // Start uploading immediately using the new file list
        validFiles.forEach((fileItem) => uploadFile(fileItem, newFileList));
        return newFileList;
      });
    }
  };

  const uploadFile = async (fileItem: FileWithProgress, currentFiles?: FileWithProgress[]) => {
    const fileList = currentFiles || files;
    const fileIndex = fileList.findIndex((f) => f.file === fileItem.file);
    if (fileIndex === -1) return;

    // Update uploading status
    setFiles((prev) => {
      const updated = [...prev];
      const index = updated.findIndex((f) => f.file === fileItem.file);
      if (index !== -1) {
        updated[index] = { ...updated[index], uploading: true };
      }
      return updated;
    });

    try {
      const response = await apiClient.uploadAttachment(
        ticketId,
        fileItem.file,
        (progressEvent: any) => {
          const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
          setFiles((prev) => {
            const updated = [...prev];
            const index = updated.findIndex((f) => f.file === fileItem.file);
            if (index !== -1) {
              updated[index] = { ...updated[index], progress: percentCompleted };
            }
            return updated;
          });
        }
      );

      // Mark as completed
      setFiles((prev) => {
        const updated = [...prev];
        const index = updated.findIndex((f) => f.file === fileItem.file);
        if (index !== -1) {
          updated[index] = { ...updated[index], uploading: false, completed: true };
        }
        return updated;
      });

      onUploadComplete?.(response);

      // Remove from list after a delay
      setTimeout(() => {
        setFiles((prev) => prev.filter((f) => f.file !== fileItem.file));
      }, 2000);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Upload failed';
      setFiles((prev) => {
        const updated = [...prev];
        const index = updated.findIndex((f) => f.file === fileItem.file);
        if (index !== -1) {
          updated[index] = {
            ...updated[index],
            uploading: false,
            error: errorMessage,
          };
        }
        return updated;
      });
      onError?.(errorMessage);
    }
  };

  const removeFile = (file: File) => {
    setFiles((prev) => prev.filter((f) => f.file !== file));
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
    // Reset input so same file can be selected again
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
      return <Image className="w-4 h-4" />;
    }
    return <File className="w-4 h-4" />;
  };

  return (
    <div className="space-y-4">
      <div
        className={`border-2 border-dashed rounded-lg p-6 text-center transition-colors ${
          isDragging
            ? 'border-blue-500 bg-blue-50 dark:bg-blue-950'
            : 'border-gray-300 dark:border-gray-700 hover:border-gray-400 dark:hover:border-gray-600'
        }`}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
      >
        <Upload className="w-8 h-8 mx-auto mb-2 text-gray-400" />
        <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">
          Drag and drop files here, or click to select
        </p>
        <p className="text-xs text-gray-500 dark:text-gray-500">
          Maximum file size: 10MB
        </p>
        <input
          ref={fileInputRef}
          type="file"
          multiple
          className="hidden"
          onChange={handleFileInputChange}
          accept={ALLOWED_FILE_TYPES.join(',')}
        />
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          className="mt-3 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          Select Files
        </button>
      </div>

      {files.length > 0 && (
        <div className="space-y-2">
          {files.map((fileItem, index) => (
            <div
              key={index}
              className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-800 rounded-lg"
            >
              <div className="flex items-center space-x-3 flex-1 min-w-0">
                {getFileIcon(fileItem.file)}
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 dark:text-gray-100 truncate">
                    {fileItem.file.name}
                  </p>
                  <p className="text-xs text-gray-500 dark:text-gray-400">
                    {formatFileSize(fileItem.file.size)}
                  </p>
                  {fileItem.uploading && (
                    <div className="mt-1 w-full bg-gray-200 dark:bg-gray-700 rounded-full h-1.5">
                      <div
                        className="bg-blue-600 h-1.5 rounded-full transition-all duration-300"
                        style={{ width: `${fileItem.progress}%` }}
                      />
                    </div>
                  )}
                  {fileItem.error && (
                    <p className="text-xs text-red-500 mt-1">{fileItem.error}</p>
                  )}
                  {fileItem.completed && (
                    <p className="text-xs text-green-500 mt-1">Upload complete</p>
                  )}
                </div>
              </div>
              <div className="flex items-center space-x-2">
                {fileItem.uploading && (
                  <Loader2 className="w-4 h-4 animate-spin text-blue-500" />
                )}
                {!fileItem.uploading && !fileItem.completed && (
                  <button
                    type="button"
                    onClick={() => removeFile(fileItem.file)}
                    className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                  >
                    <X className="w-4 h-4" />
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
