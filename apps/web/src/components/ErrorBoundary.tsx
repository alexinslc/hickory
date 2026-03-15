'use client';

import { Component, type ErrorInfo, type ReactNode } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
  onError?: (error: Error, errorInfo: ErrorInfo) => void;
  /** Section name shown in error UI, e.g. "Tickets" */
  section?: string;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidUpdate(prevProps: ErrorBoundaryProps): void {
    if (this.state.hasError && prevProps.children !== this.props.children) {
      this.setState({ hasError: false, error: null });
    }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    console.error('[ErrorBoundary] Caught error:', error, errorInfo);
    this.props.onError?.(error, errorInfo);
  }

  handleReset = (): void => {
    this.setState({ hasError: false, error: null });
  };

  render(): ReactNode {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <ErrorFallback
          error={this.state.error}
          section={this.props.section}
          onReset={this.handleReset}
        />
      );
    }

    return this.props.children;
  }
}

interface ErrorFallbackProps {
  error: Error | null;
  section?: string;
  onReset: () => void;
}

export function ErrorFallback({ error, section, onReset }: ErrorFallbackProps) {
  const title = section
    ? `Something went wrong in ${section}`
    : 'Something went wrong';

  return (
    <div className="flex items-center justify-center min-h-[200px] p-6" role="alert">
      <Card className="max-w-lg w-full">
        <CardHeader>
          <CardTitle className="text-lg text-destructive">{title}</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground mb-4">
            An unexpected error occurred. You can try again or return to the home page.
          </p>
          {error && process.env.NODE_ENV === 'development' && (
            <details className="text-xs text-muted-foreground">
              <summary className="cursor-pointer mb-2 font-medium">
                Error details
              </summary>
              <pre className="bg-muted p-3 rounded-md overflow-auto max-h-40 whitespace-pre-wrap">
                {error.message}
                {error.stack && `\n\n${error.stack}`}
              </pre>
            </details>
          )}
        </CardContent>
        <CardFooter className="gap-3">
          <Button onClick={onReset}>Try again</Button>
          {/* Intentional hard navigation to fully reset app state after an error */}
          <Button variant="outline" asChild>
            <a href="/">Go to home page</a>
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
