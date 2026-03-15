'use client';

import { useState, useCallback, useMemo } from 'react';
import { useLogin } from '@/hooks/use-auth';
import { getFieldErrors } from '@/lib/api-client';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { TicketIcon, LogIn, AlertCircle, Loader2 } from 'lucide-react';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [touched, setTouched] = useState({ email: false, password: false });
  const login = useLogin();

  const handleBlur = useCallback((field: 'email' | 'password') => {
    setTouched((prev) => ({ ...prev, [field]: true }));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setTouched({ email: true, password: true });

    if (!isValid) return;

    login.mutate({ email, password });
  };

  // Client-side validation
  const emailError = useMemo(() => {
    if (!touched.email) return null;
    if (email.length === 0) return 'Email is required';
    if (!email.includes('@')) return 'Please enter a valid email address';
    return null;
  }, [email, touched.email]);

  const passwordError = useMemo(() => {
    if (!touched.password) return null;
    if (password.length === 0) return 'Password is required';
    return null;
  }, [password, touched.password]);

  const isValid = email.includes('@') && password.length > 0;

  // Server-side field errors
  const serverFieldErrors = login.isError ? getFieldErrors(login.error) : null;
  const serverEmailError = serverFieldErrors?.email?.[0] ?? null;
  const serverPasswordError = serverFieldErrors?.password?.[0] ?? null;

  // Combine client + server errors (client errors take priority when field is being edited)
  const displayEmailError = emailError || serverEmailError;
  const displayPasswordError = passwordError || serverPasswordError;

  // General server error (not field-specific)
  const hasGeneralError = login.isError && !serverFieldErrors;

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 via-blue-50 to-blue-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-md">
        {/* Logo and Title */}
        <div className="text-center mb-8">
          <div className="flex justify-center mb-4">
            <div className="bg-primary text-primary-foreground p-3 rounded-2xl shadow-lg" aria-hidden="true">
              <TicketIcon className="h-10 w-10" />
            </div>
          </div>
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 mb-2">
            Hickory
          </h1>
          <p className="text-gray-600">
            Your Modern Help Desk Solution
          </p>
        </div>

        {/* Login Card */}
        <Card className="shadow-xl">
          <CardHeader className="space-y-1">
            <CardTitle className="text-2xl">Welcome back</CardTitle>
            <CardDescription>
              Enter your credentials to access your account
            </CardDescription>
          </CardHeader>

          <form onSubmit={handleSubmit} aria-label="Login form" noValidate>
            <CardContent className="space-y-4">
              <div className="space-y-1.5">
                <Label htmlFor="email">
                  Email <span className="text-destructive" aria-hidden="true">*</span>
                </Label>
                <Input
                  id="email"
                  name="email"
                  type="email"
                  placeholder="you@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  onBlur={() => handleBlur('email')}
                  disabled={login.isPending}
                  required
                  autoComplete="email"
                  aria-required="true"
                  aria-invalid={!!displayEmailError || undefined}
                  aria-describedby={displayEmailError ? 'email-error' : undefined}
                  className={displayEmailError ? 'border-destructive focus-visible:ring-destructive' : ''}
                />
                {displayEmailError && (
                  <p id="email-error" className="flex items-center gap-1 text-xs text-destructive" role="alert">
                    <AlertCircle className="h-3 w-3 flex-shrink-0" aria-hidden="true" />
                    {displayEmailError}
                  </p>
                )}
              </div>

              <div className="space-y-1.5">
                <div className="flex items-center justify-between">
                  <Label htmlFor="password">
                    Password <span className="text-destructive" aria-hidden="true">*</span>
                  </Label>
                  <Link
                    href="/auth/forgot-password"
                    className="text-xs text-primary hover:underline"
                  >
                    Forgot password?
                  </Link>
                </div>
                <Input
                  id="password"
                  name="password"
                  type="password"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  onBlur={() => handleBlur('password')}
                  disabled={login.isPending}
                  required
                  autoComplete="current-password"
                  aria-required="true"
                  aria-invalid={!!displayPasswordError || undefined}
                  aria-describedby={displayPasswordError ? 'password-error' : undefined}
                  className={displayPasswordError ? 'border-destructive focus-visible:ring-destructive' : ''}
                />
                {displayPasswordError && (
                  <p id="password-error" className="flex items-center gap-1 text-xs text-destructive" role="alert">
                    <AlertCircle className="h-3 w-3 flex-shrink-0" aria-hidden="true" />
                    {displayPasswordError}
                  </p>
                )}
              </div>

              {hasGeneralError && (
                <div className="flex items-start gap-2 p-3 rounded-md bg-destructive/10 text-destructive border border-destructive/20" role="alert">
                  <AlertCircle className="h-5 w-5 mt-0.5 flex-shrink-0" aria-hidden="true" />
                  <div className="text-sm">
                    <p className="font-medium">Authentication failed</p>
                    <p className="text-xs mt-1">
                      {login.error?.message || 'Please check your credentials and try again'}
                    </p>
                  </div>
                </div>
              )}
            </CardContent>

            <CardFooter className="flex flex-col space-y-4">
              <Button
                type="submit"
                className="w-full"
                size="lg"
                disabled={login.isPending}
              >
                {login.isPending ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" aria-hidden="true" />
                    Signing in...
                  </>
                ) : (
                  <>
                    <LogIn className="mr-2 h-4 w-4" aria-hidden="true" />
                    Sign In
                  </>
                )}
              </Button>

              <div className="text-sm text-center text-muted-foreground">
                Don't have an account?{' '}
                <Link
                  href="/auth/register"
                  className="text-primary font-medium hover:underline"
                >
                  Create account
                </Link>
              </div>
            </CardFooter>
          </form>
        </Card>

        {/* Footer */}
        <p className="text-center text-xs text-gray-500 mt-8">
          &copy; 2025 Hickory Help Desk. All rights reserved.
        </p>
      </div>
    </div>
  );
}
