import * as React from 'react';
import { cn } from '@/lib/utils';
import { AlertCircle } from 'lucide-react';

interface FormFieldProps {
  /** Unique identifier for the field, used for htmlFor/id/aria attributes */
  id: string;
  /** Label text for the field */
  label: string;
  /** Whether the field is required */
  required?: boolean;
  /** Validation error message to display */
  error?: string | null;
  /** Helper text shown when there is no error */
  hint?: string;
  /** The form control to render */
  children: React.ReactElement<{
    id?: string;
    'aria-invalid'?: boolean;
    'aria-describedby'?: string;
    'aria-required'?: boolean;
  }>;
  /** Additional class name for the wrapper */
  className?: string;
  /** Whether to use sr-only for the label (visually hidden) */
  srOnlyLabel?: boolean;
}

/**
 * A reusable form field wrapper that handles:
 * - Accessible labels with required indicators
 * - Inline validation error messages with error icons
 * - ARIA attributes (aria-invalid, aria-describedby, aria-required)
 * - Helper text / hints
 * - Consistent styling
 */
export function FormField({
  id,
  label,
  required = false,
  error,
  hint,
  children,
  className,
  srOnlyLabel = false,
}: FormFieldProps) {
  const errorId = `${id}-error`;
  const hintId = `${id}-hint`;
  const hasError = !!error;
  const describedBy = hasError ? errorId : hint ? hintId : undefined;

  // Clone the child element to inject accessibility props
  const enhancedChild = React.cloneElement(children, {
    id,
    'aria-invalid': hasError || undefined,
    'aria-describedby': describedBy,
    'aria-required': required || undefined,
  });

  return (
    <div className={cn('space-y-1.5', className)}>
      <label
        htmlFor={id}
        className={cn(
          'block text-sm font-medium text-gray-700 dark:text-gray-300',
          srOnlyLabel && 'sr-only'
        )}
      >
        {label}
        {required && (
          <span className="text-destructive ml-0.5" aria-hidden="true">
            *
          </span>
        )}
      </label>

      {enhancedChild}

      {hasError && (
        <p
          id={errorId}
          className="flex items-center gap-1 text-xs text-destructive"
          role="alert"
        >
          <AlertCircle className="h-3 w-3 flex-shrink-0" aria-hidden="true" />
          {error}
        </p>
      )}

      {!hasError && hint && (
        <p id={hintId} className="text-xs text-muted-foreground">
          {hint}
        </p>
      )}
    </div>
  );
}
