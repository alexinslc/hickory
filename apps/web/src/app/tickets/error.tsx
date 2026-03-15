'use client';

import { useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';

export default function TicketsError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error('[TicketsError]', error);
  }, [error]);

  return (
    <div className="flex items-center justify-center min-h-[60vh] p-6" role="alert">
      <Card className="max-w-lg w-full">
        <CardHeader>
          <CardTitle className="text-lg text-destructive">
            Unable to load tickets
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground mb-4">
            There was a problem loading the tickets section. This could be a
            temporary issue with the server.
          </p>
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
