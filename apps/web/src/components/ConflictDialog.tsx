'use client';

interface ConflictDialogProps {
  isOpen: boolean;
  message: string;
  onRetry: () => void;
  onCancel: () => void;
}

export function ConflictDialog({ isOpen, message, onRetry, onCancel }: ConflictDialogProps) {
  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-50"
        onClick={onCancel}
        aria-hidden="true"
      />

      {/* Dialog */}
      <div className="fixed inset-0 z-50 overflow-y-auto">
        <div className="flex min-h-full items-center justify-center p-4">
          <div className="relative bg-white rounded-lg shadow-xl max-w-md w-full">
            {/* Icon */}
            <div className="flex items-center justify-center w-12 h-12 mx-auto mt-6 bg-yellow-100 rounded-full">
              <svg className="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
              </svg>
            </div>

            {/* Content */}
            <div className="px-6 py-4 text-center">
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Update Conflict Detected
              </h3>
              <p className="text-sm text-gray-600 mb-4">
                {message}
              </p>
              <div className="bg-blue-50 border border-blue-200 rounded-md p-3 mb-4">
                <p className="text-xs text-blue-800">
                  <strong>What happened?</strong> Someone else modified this item while you were working on it. 
                  Click "Refresh & Retry" to load the latest version and try again.
                </p>
              </div>
            </div>

            {/* Actions */}
            <div className="px-6 py-4 border-t border-gray-200 flex justify-end space-x-3">
              <button
                type="button"
                onClick={onCancel}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={onRetry}
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                <svg className="inline-block w-4 h-4 mr-1 -mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                Refresh & Retry
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
