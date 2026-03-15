'use client';

import { useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';

export default function AuthError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error('[AuthError]', error);
  }, [error]);

  return (
    <div className="flex items-center justify-center min-h-[60vh] p-6" role="alert">
      <Card className="max-w-lg w-full">
        <CardHeader>
          <CardTitle className="text-lg text-destructive">
            Authentication error
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground mb-4">
            There was a problem with authentication. Please try signing in
            again.
          </p>
        </CardContent>
        <CardFooter className="gap-3">
          <Button onClick={reset}>Try again</Button>
          {/* Intentional hard navigation to fully reset app state after an auth error */}
          <Button variant="outline" asChild>
            <a href="/auth/login">Go to login</a>
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
