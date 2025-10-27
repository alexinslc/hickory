'use client';

import { useEffect, useState } from 'react';
import { useNotifications } from './NotificationProvider';
import Link from 'next/link';

export function NotificationToast() {
  const { notifications, markAsRead } = useNotifications();
  const [visibleNotifications, setVisibleNotifications] = useState<typeof notifications>([]);

  useEffect(() => {
    // Show only the most recent notification
    if (notifications.length > 0 && notifications[0] !== visibleNotifications[0]) {
      setVisibleNotifications([notifications[0]]);
      
      // Auto-dismiss after 5 seconds
      const timer = setTimeout(() => {
        setVisibleNotifications([]);
        markAsRead();
      }, 5000);
      
      return () => clearTimeout(timer);
    }
  }, [notifications, visibleNotifications, markAsRead]);

  if (visibleNotifications.length === 0) {
    return null;
  }

  const notification = visibleNotifications[0];

  return (
    <div className="fixed bottom-4 right-4 z-50 max-w-sm">
      <div className="bg-white rounded-lg shadow-lg border border-gray-200 p-4 animate-slide-up">
        <div className="flex items-start gap-3">
          {/* Icon based on notification type */}
          <div className="flex-shrink-0">
            {notification.type === 'ticket.created' && (
              <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
                <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
              </div>
            )}
            {notification.type === 'ticket.updated' && (
              <div className="w-10 h-10 bg-yellow-100 rounded-full flex items-center justify-center">
                <svg className="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
              </div>
            )}
            {notification.type === 'ticket.assigned' && (
              <div className="w-10 h-10 bg-green-100 rounded-full flex items-center justify-center">
                <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </div>
            )}
            {notification.type === 'comment.added' && (
              <div className="w-10 h-10 bg-purple-100 rounded-full flex items-center justify-center">
                <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                </svg>
              </div>
            )}
          </div>

          {/* Content */}
          <div className="flex-1 min-w-0">
            <p className="text-sm font-semibold text-gray-900">{notification.title}</p>
            <p className="text-sm text-gray-600 mt-1">{notification.message}</p>
            {notification.ticketNumber && (
              <Link
                href={`/tickets/${notification.ticketNumber}`}
                className="text-sm text-blue-600 hover:text-blue-800 mt-2 inline-block"
              >
                View Ticket →
              </Link>
            )}
          </div>

          {/* Close button */}
          <button
            onClick={() => {
              setVisibleNotifications([]);
              markAsRead();
            }}
            className="flex-shrink-0 text-gray-400 hover:text-gray-600"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>
    </div>
  );
}
