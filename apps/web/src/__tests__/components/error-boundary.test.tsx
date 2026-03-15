import { render, screen, fireEvent } from '@testing-library/react';
import { ErrorBoundary, ErrorFallback } from '@/components/ErrorBoundary';

// Suppress console.error for expected errors in tests
const originalConsoleError = console.error;
beforeAll(() => {
  console.error = jest.fn();
});
afterAll(() => {
  console.error = originalConsoleError;
});

function ThrowingComponent({ shouldThrow = true }: { shouldThrow?: boolean }) {
  if (shouldThrow) {
    throw new Error('Test error');
  }
  return <div>Child content</div>;
}

describe('ErrorBoundary', () => {
  it('renders children when there is no error', () => {
    render(
      <ErrorBoundary>
        <div>Hello</div>
      </ErrorBoundary>
    );

    expect(screen.getByText('Hello')).toBeTruthy();
  });

  it('renders fallback UI when a child throws', () => {
    render(
      <ErrorBoundary>
        <ThrowingComponent />
      </ErrorBoundary>
    );

    expect(screen.getByRole('alert')).toBeTruthy();
    expect(screen.getByText('Something went wrong')).toBeTruthy();
    expect(screen.getByText('Try again')).toBeTruthy();
    expect(screen.getByText('Go to home page')).toBeTruthy();
  });

  it('renders section name when provided', () => {
    render(
      <ErrorBoundary section="Tickets">
        <ThrowingComponent />
      </ErrorBoundary>
    );

    expect(
      screen.getByText('Something went wrong in Tickets')
    ).toBeTruthy();
  });

  it('renders custom fallback when provided', () => {
    render(
      <ErrorBoundary fallback={<div>Custom fallback</div>}>
        <ThrowingComponent />
      </ErrorBoundary>
    );

    expect(screen.getByText('Custom fallback')).toBeTruthy();
  });

  it('calls onError callback when an error occurs', () => {
    const onError = jest.fn();

    render(
      <ErrorBoundary onError={onError}>
        <ThrowingComponent />
      </ErrorBoundary>
    );

    expect(onError).toHaveBeenCalledTimes(1);
    expect(onError.mock.calls[0][0]).toBeInstanceOf(Error);
    expect(onError.mock.calls[0][0].message).toBe('Test error');
  });

  it('recovers when Try again is clicked and error is resolved', () => {
    let shouldThrow = true;

    function ConditionalThrower() {
      if (shouldThrow) {
        throw new Error('Test error');
      }
      return <div>Recovered content</div>;
    }

    render(
      <ErrorBoundary>
        <ConditionalThrower />
      </ErrorBoundary>
    );

    expect(screen.getByRole('alert')).toBeTruthy();

    // Fix the error condition
    shouldThrow = false;

    // Click Try again
    fireEvent.click(screen.getByText('Try again'));

    expect(screen.getByText('Recovered content')).toBeTruthy();
    expect(screen.queryByRole('alert')).toBeNull();
  });
});

describe('ErrorFallback', () => {
  it('renders with default title', () => {
    render(<ErrorFallback error={null} onReset={jest.fn()} />);

    expect(screen.getByText('Something went wrong')).toBeTruthy();
  });

  it('renders with section-specific title', () => {
    render(
      <ErrorFallback error={null} section="Dashboard" onReset={jest.fn()} />
    );

    expect(
      screen.getByText('Something went wrong in Dashboard')
    ).toBeTruthy();
  });

  it('calls onReset when Try again is clicked', () => {
    const onReset = jest.fn();
    render(<ErrorFallback error={null} onReset={onReset} />);

    fireEvent.click(screen.getByText('Try again'));

    expect(onReset).toHaveBeenCalledTimes(1);
  });

  it('shows error details in development mode', () => {
    const originalEnv = process.env.NODE_ENV;
    Object.defineProperty(process.env, 'NODE_ENV', { value: 'development', configurable: true });

    const error = new Error('Detailed error message');
    render(<ErrorFallback error={error} onReset={jest.fn()} />);

    expect(screen.getByText('Error details')).toBeTruthy();

    Object.defineProperty(process.env, 'NODE_ENV', { value: originalEnv, configurable: true });
  });

  it('has accessible alert role', () => {
    render(<ErrorFallback error={null} onReset={jest.fn()} />);

    expect(screen.getByRole('alert')).toBeTruthy();
  });
});
