import * as signalR from '@microsoft/signalr';

export interface NotificationMessage {
  type: string; // "ticket.created", "ticket.updated", etc.
  title: string;
  message: string;
  ticketNumber: string;
  ticketId?: string;
  timestamp: string;
  data?: Record<string, unknown>;
}

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;
  private reconnectDelay = 1000; // Start with 1 second

  async connect(accessToken: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      console.log('SignalR already connected');
      return;
    }

    const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';
    
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiUrl}/hubs/notifications`, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 1s, 2s, 4s, 8s, 16s, 32s, max 60s
          const delay = Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 60000);
          console.log(`SignalR reconnecting in ${delay}ms (attempt ${retryContext.previousRetryCount + 1})`);
          return delay;
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.onreconnecting((error) => {
      console.warn('SignalR reconnecting...', error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
      this.reconnectAttempts = 0;
    });

    this.connection.onclose(async (error) => {
      console.error('SignalR connection closed:', error);
      
      if (this.reconnectAttempts < this.maxReconnectAttempts) {
        const delay = this.reconnectDelay * Math.pow(2, this.reconnectAttempts);
        this.reconnectAttempts++;
        
        console.log(`Attempting manual reconnect in ${delay}ms (attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts})`);
        
        await new Promise(resolve => setTimeout(resolve, delay));
        
        try {
          await this.connect(accessToken);
        } catch (err) {
          console.error('Manual reconnect failed:', err);
        }
      } else {
        console.error('Max reconnect attempts reached');
      }
    });

    try {
      await this.connection.start();
      console.log('SignalR connected');
      this.reconnectAttempts = 0;
    } catch (err) {
      console.error('SignalR connection failed:', err);
      throw err;
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      console.log('SignalR disconnected');
    }
  }

  onNotification(callback: (notification: NotificationMessage) => void): void {
    if (!this.connection) {
      console.warn('SignalR not connected, cannot register notification handler');
      return;
    }

    this.connection.on('notification', callback);
  }

  offNotification(callback: (notification: NotificationMessage) => void): void {
    if (this.connection) {
      this.connection.off('notification', callback);
    }
  }

  getConnectionState(): signalR.HubConnectionState | null {
    return this.connection?.state ?? null;
  }
}

export const signalRService = new SignalRService();
