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

export type ConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

type ConnectionStateListener = (state: ConnectionState) => void;

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;
  private reconnectDelay = 1000;
  private isReconnecting = false;
  private accessToken: string | null = null;

  // Reactive connection state
  private _connectionState: ConnectionState = 'disconnected';
  private stateListeners: Set<ConnectionStateListener> = new Set();

  // Offline message queue
  private messageQueue: NotificationMessage[] = [];
  private maxQueueSize = 100;

  // Missed message tracking
  private lastReceivedTimestamp: string | null = null;

  // Connection metrics
  private connectStartTime = 0;
  private totalReconnects = 0;
  private lastConnectedAt: Date | null = null;

  get connectionState(): ConnectionState {
    return this._connectionState;
  }

  private setConnectionState(state: ConnectionState): void {
    if (this._connectionState !== state) {
      this._connectionState = state;
      this.stateListeners.forEach((listener) => listener(state));
    }
  }

  onConnectionStateChange(listener: ConnectionStateListener): () => void {
    this.stateListeners.add(listener);
    // Immediately notify with current state
    listener(this._connectionState);
    return () => this.stateListeners.delete(listener);
  }

  async connect(accessToken: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.accessToken = accessToken;
    this.setConnectionState('connecting');
    this.connectStartTime = Date.now();

    const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiUrl}/hubs/notifications`, {
        accessTokenFactory: () => this.accessToken || '',
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          const delay = Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 60000);
          console.log(`SignalR reconnecting in ${delay}ms (attempt ${retryContext.previousRetryCount + 1})`);
          return delay;
        },
      })
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.onreconnecting(() => {
      this.setConnectionState('reconnecting');
    });

    this.connection.onreconnected(() => {
      this.reconnectAttempts = 0;
      this.isReconnecting = false;
      this.totalReconnects++;
      this.lastConnectedAt = new Date();
      this.setConnectionState('connected');
      this.replayMissedMessages();
    });

    this.connection.onclose(async (error) => {
      this.setConnectionState('disconnected');

      if (this.isReconnecting) return;

      if (this.reconnectAttempts < this.maxReconnectAttempts) {
        this.isReconnecting = true;
        const delay = Math.min(this.reconnectDelay * Math.pow(2, this.reconnectAttempts), 60000);
        this.reconnectAttempts++;

        console.log(`Manual reconnect in ${delay}ms (attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts})`);

        await new Promise((resolve) => setTimeout(resolve, delay));

        try {
          await this.connect(this.accessToken!);
        } catch (err) {
          console.error('Manual reconnect failed:', err);
          this.isReconnecting = false;
        }
      } else {
        console.error('Max reconnect attempts reached');
        this.isReconnecting = false;
      }
    });

    try {
      await this.connection.start();
      this.reconnectAttempts = 0;
      this.isReconnecting = false;
      this.lastConnectedAt = new Date();
      this.setConnectionState('connected');

      const connectDuration = Date.now() - this.connectStartTime;
      console.log(`SignalR connected in ${connectDuration}ms`);
    } catch (err) {
      this.isReconnecting = false;
      this.setConnectionState('disconnected');
      throw err;
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.setConnectionState('disconnected');
    }
  }

  /** Update the access token for long-lived connections. */
  updateAccessToken(newToken: string): void {
    this.accessToken = newToken;
  }

  onNotification(callback: (notification: NotificationMessage) => void): void {
    if (!this.connection) {
      console.warn('SignalR not connected, cannot register notification handler');
      return;
    }

    this.connection.on('notification', (msg: NotificationMessage) => {
      this.lastReceivedTimestamp = msg.timestamp;
      callback(msg);
    });
  }

  offNotification(callback: (notification: NotificationMessage) => void): void {
    if (this.connection) {
      this.connection.off('notification', callback);
    }
  }

  /** Queue a message for delivery when reconnected. */
  queueMessage(message: NotificationMessage): void {
    if (this.messageQueue.length >= this.maxQueueSize) {
      this.messageQueue.shift(); // Drop oldest
    }
    this.messageQueue.push(message);
  }

  /** Flush queued messages to a callback. */
  flushQueue(callback: (message: NotificationMessage) => void): void {
    const messages = [...this.messageQueue];
    this.messageQueue = [];
    messages.forEach(callback);
  }

  /** Request missed messages since last received timestamp. */
  private async replayMissedMessages(): Promise<void> {
    if (!this.connection || !this.lastReceivedTimestamp) return;

    try {
      await this.connection.invoke('RequestMissedNotifications', this.lastReceivedTimestamp);
    } catch {
      // Server may not support this method yet — that's OK
    }
  }

  getConnectionState(): signalR.HubConnectionState | null {
    return this.connection?.state ?? null;
  }

  getMetrics() {
    return {
      state: this._connectionState,
      totalReconnects: this.totalReconnects,
      lastConnectedAt: this.lastConnectedAt,
      queuedMessages: this.messageQueue.length,
      lastReceivedTimestamp: this.lastReceivedTimestamp,
    };
  }
}

export const signalRService = new SignalRService();
