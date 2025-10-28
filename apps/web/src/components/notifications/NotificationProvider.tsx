'use client';

import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import { signalRService, NotificationMessage } from '@/lib/signalr/signalr-service';
import { useAuthStore } from '@/store/auth-store';

interface NotificationContextType {
  notifications: NotificationMessage[];
  unreadCount: number;
  addNotification: (notification: NotificationMessage) => void;
  markAsRead: () => void;
  markAllAsRead: () => void;
  clearNotifications: () => void;
  isConnected: boolean;
  connectionError: string | null;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export function NotificationProvider({ children }: { children: React.ReactNode }) {
  const [notifications, setNotifications] = useState<NotificationMessage[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [isConnected, setIsConnected] = useState(false);
  const [connectionError, setConnectionError] = useState<string | null>(null);
  const { accessToken, isAuthenticated } = useAuthStore();

  const addNotification = useCallback((notification: NotificationMessage) => {
    setNotifications(prev => [notification, ...prev].slice(0, 50)); // Keep last 50
    setUnreadCount(prev => prev + 1);
  }, []);

  const markAsRead = useCallback(() => {
    // In a full implementation, you would track read status per notification
    // For now, just decrement the unread count when one is marked as read
    setUnreadCount(prev => Math.max(0, prev - 1));
  }, []);

  const markAllAsRead = useCallback(() => {
    setUnreadCount(0);
  }, []);

  const clearNotifications = useCallback(() => {
    setNotifications([]);
    setUnreadCount(0);
  }, []);

  useEffect(() => {
    // Don't connect if not authenticated or no token
    if (!isAuthenticated || !accessToken) {
      console.log('Not authenticated or no access token, skipping SignalR connection');
      setIsConnected(false);
      setConnectionError(null);
      return;
    }

    let mounted = true;

    const handleNotification = (notification: NotificationMessage) => {
      if (mounted) {
        addNotification(notification);
      }
    };

    const connectSignalR = async () => {
      try {
        setConnectionError(null);
        await signalRService.connect(accessToken);
        if (mounted) {
          setIsConnected(true);
          signalRService.onNotification(handleNotification);
          console.log('SignalR notifications ready');
        }
      } catch (error) {
        console.error('Failed to connect to SignalR:', error);
        if (mounted) {
          setIsConnected(false);
          setConnectionError(error instanceof Error ? error.message : 'Connection failed');
        }
      }
    };

    connectSignalR();

    return () => {
      mounted = false;
      signalRService.offNotification(handleNotification);
      signalRService.disconnect();
    };
  }, [accessToken, isAuthenticated, addNotification]);

  const value: NotificationContextType = {
    notifications,
    unreadCount,
    addNotification,
    markAsRead,
    markAllAsRead,
    clearNotifications,
    isConnected,
    connectionError,
  };

  return (
    <NotificationContext.Provider value={value}>
      {children}
    </NotificationContext.Provider>
  );
}

export function useNotifications() {
  const context = useContext(NotificationContext);
  if (context === undefined) {
    throw new Error('useNotifications must be used within a NotificationProvider');
  }
  return context;
}
