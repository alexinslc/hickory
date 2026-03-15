'use client';

import { useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error('[RootError]', error);
  }, [error]);

  return (
    <div className="flex items-center justify-center min-h-[60vh] p-6" role="alert">
      <Card className="max-w-lg w-full">
        <CardHeader>
          <CardTitle className="text-lg text-destructive">
            Something went wrong
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground mb-4">
            An unexpected error occurred. Please try again or return to the home page.
          </p>
          {process.env.NODE_ENV === 'development' && error.message && (
            <details className="text-xs text-muted-foreground">
              <summary className="cursor-pointer mb-2 font-medium">
                Error details
              </summary>
              <pre className="bg-muted p-3 rounded-md overflow-auto max-h-40 whitespace-pre-wrap">
                {error.message}
              </pre>
            </details>
          )}
        </CardContent>
        <CardFooter className="gap-3">
          <Button onClick={reset}>Try again</Button>
          {/* Intentional hard navigation to fully reset app state after an error */}
          <Button variant="outline" asChild>
            <a href="/">Go to home page</a>
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
