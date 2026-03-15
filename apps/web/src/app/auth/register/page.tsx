'use client';

import { useState, useCallback, useMemo } from 'react';
import { useRegister } from '@/hooks/use-auth';
import { getFieldErrors } from '@/lib/api-client';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { TicketIcon, UserPlus, AlertCircle, Loader2 } from 'lucide-react';

export default function RegisterPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [touched, setTouched] = useState({
    email: false,
    password: false,
    firstName: false,
    lastName: false,
  });
  const register = useRegister();

  const handleBlur = useCallback((field: keyof typeof touched) => {
    setTouched((prev) => ({ ...prev, [field]: true }));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setTouched({ email: true, password: true, firstName: true, lastName: true });

    if (!isValid) return;

    register.mutate({ email, password, firstName, lastName });
  };

  // Client-side validation
  const firstNameError = useMemo(() => {
    if (!touched.firstName) return null;
    if (firstName.length === 0 || firstName.trim().length < 1) return 'First name is required';
    if (firstName.length > 100) return 'First name must be no more than 100 characters';
    return null;
  }, [firstName, touched.firstName]);

  const lastNameError = useMemo(() => {
    if (!touched.lastName) return null;
    if (lastName.length === 0 || lastName.trim().length < 1) return 'Last name is required';
    if (lastName.length > 100) return 'Last name must be no more than 100 characters';
    return null;
  }, [lastName, touched.lastName]);

  const emailError = useMemo(() => {
    if (!touched.email) return null;
    if (email.length === 0) return 'Email is required';
    if (!email.includes('@')) return 'Please enter a valid email address';
    return null;
  }, [email, touched.email]);

  const passwordError = useMemo(() => {
    if (!touched.password) return null;
    if (password.length === 0) return 'Password is required';
    if (password.length < 8) return 'Password must be at least 8 characters';
    if (!/[A-Z]/.test(password)) return 'Password must contain an uppercase letter';
    if (!/[a-z]/.test(password)) return 'Password must contain a lowercase letter';
    if (!/[0-9]/.test(password)) return 'Password must contain a number';
    if (!/[^A-Za-z0-9]/.test(password)) return 'Password must contain a special character';
    return null;
  }, [password, touched.password]);

  const isValid =
    firstName.trim().length >= 1 &&
    lastName.trim().length >= 1 &&
    email.includes('@') &&
    password.length >= 8 &&
    /[A-Z]/.test(password) &&
    /[a-z]/.test(password) &&
    /[0-9]/.test(password) &&
    /[^A-Za-z0-9]/.test(password);

  // Server-side field errors
  const serverFieldErrors = register.isError ? getFieldErrors(register.error) : null;
  const serverFirstNameError = serverFieldErrors?.firstName?.[0] ?? null;
  const serverLastNameError = serverFieldErrors?.lastName?.[0] ?? null;
  const serverEmailError = serverFieldErrors?.email?.[0] ?? null;
  const serverPasswordError = serverFieldErrors?.password?.[0] ?? null;

  // Combine client + server errors
  const displayFirstNameError = firstNameError || serverFirstNameError;
  const displayLastNameError = lastNameError || serverLastNameError;
  const displayEmailError = emailError || serverEmailError;
  const displayPasswordError = passwordError || serverPasswordError;

  // General server error (not field-specific)
  const hasGeneralError = register.isError && !serverFieldErrors;

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

        {/* Register Card */}
        <Card className="shadow-xl">
          <CardHeader className="space-y-1">
            <CardTitle className="text-2xl">Create your account</CardTitle>
            <CardDescription>
              Enter your details to get started
            </CardDescription>
          </CardHeader>

          <form onSubmit={handleSubmit} aria-label="Registration form" noValidate>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <Label htmlFor="first-name">
                    First name <span className="text-destructive" aria-hidden="true">*</span>
                  </Label>
                  <Input
                    id="first-name"
                    name="firstName"
                    type="text"
                    placeholder="First name"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    onBlur={() => handleBlur('firstName')}
                    disabled={register.isPending}
                    required
                    autoComplete="given-name"
                    aria-required="true"
                    aria-invalid={!!displayFirstNameError || undefined}
                    aria-describedby={displayFirstNameError ? 'first-name-error' : undefined}
                    className={displayFirstNameError ? 'border-destructive focus-visible:ring-destructive' : ''}
                  />
                  {displayFirstNameError && (
                    <p id="first-name-error" className="flex items-center gap-1 text-xs text-destructive" role="alert">
                      <AlertCircle className="h-3 w-3 flex-shrink-0" aria-hidden="true" />
                      {displayFirstNameError}
                    </p>
                  )}
                </div>

                <div className="space-y-1.5">
                  <Label htmlFor="last-name">
                    Last name <span className="text-destructive" aria-hidden="true">*</span>
                  </Label>
                  <Input
                    id="last-name"
                    name="lastName"
                    type="text"
                    placeholder="Last name"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    onBlur={() => handleBlur('lastName')}
                    disabled={register.isPending}
                    required
                    autoComplete="family-name"
                    aria-required="true"
                    aria-invalid={!!displayLastNameError || undefined}
                    aria-describedby={displayLastNameError ? 'last-name-error' : undefined}
                    className={displayLastNameError ? 'border-destructive focus-visible:ring-destructive' : ''}
                  />
                  {displayLastNameError && (
                    <p id="last-name-error" className="flex items-center gap-1 text-xs text-destructive" role="alert">
                      <AlertCircle className="h-3 w-3 flex-shrink-0" aria-hidden="true" />
                      {displayLastNameError}
                    </p>
                  )}
                </div>
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="email-address">
                  Email <span className="text-destructive" aria-hidden="true">*</span>
                </Label>
                <Input
                  id="email-address"
                  name="email"
                  type="email"
                  placeholder="you@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  onBlur={() => handleBlur('email')}
                  disabled={register.isPending}
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
                <Label htmlFor="password">
                  Password <span className="text-destructive" aria-hidden="true">*</span>
                </Label>
                <Input
                  id="password"
                  name="password"
                  type="password"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  onBlur={() => handleBlur('password')}
                  disabled={register.isPending}
                  required
                  autoComplete="new-password"
                  aria-required="true"
                  aria-invalid={!!displayPasswordError || undefined}
                  aria-describedby={displayPasswordError ? 'password-error' : 'password-hint'}
                  className={displayPasswordError ? 'border-destructive focus-visible:ring-destructive' : ''}
                />
                {displayPasswordError ? (
                  <p id="password-error" className="flex items-center gap-1 text-xs text-destructive" role="alert">
                    <AlertCircle className="h-3 w-3 flex-shrink-0" aria-hidden="true" />
                    {displayPasswordError}
                  </p>
                ) : (
                  <p id="password-hint" className="text-xs text-muted-foreground">
                    Must contain uppercase, lowercase, number, and special character (min 8 chars)
                  </p>
                )}
              </div>

              {hasGeneralError && (
                <div className="flex items-start gap-2 p-3 rounded-md bg-destructive/10 text-destructive border border-destructive/20" role="alert">
                  <AlertCircle className="h-5 w-5 mt-0.5 flex-shrink-0" aria-hidden="true" />
                  <div className="text-sm">
                    <p className="font-medium">Registration failed</p>
                    <p className="text-xs mt-1">
                      {register.error?.message || 'Please check your details and try again'}
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
                disabled={register.isPending}
              >
                {register.isPending ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" aria-hidden="true" />
                    Creating account...
                  </>
                ) : (
                  <>
                    <UserPlus className="mr-2 h-4 w-4" aria-hidden="true" />
                    Create account
                  </>
                )}
              </Button>

              <div className="text-sm text-center text-muted-foreground">
                Already have an account?{' '}
                <Link
                  href="/auth/login"
                  className="text-primary font-medium hover:underline"
                >
                  Sign in
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
