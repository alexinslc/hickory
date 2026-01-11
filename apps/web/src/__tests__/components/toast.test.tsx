import { render, screen, fireEvent, act } from '@testing-library/react';
import { ToastProvider, useToast } from '@/components/ui/toast';

// Mock matchMedia for theme provider
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
});

// Test component to trigger toasts
function TestComponent() {
  const { success, error, info, warning } = useToast();
  return (
    <div>
      <button onClick={() => success('Success message')}>Show Success</button>
      <button onClick={() => error('Error message')}>Show Error</button>
      <button onClick={() => info('Info message')}>Show Info</button>
      <button onClick={() => warning('Warning message')}>Show Warning</button>
    </div>
  );
}

describe('Toast', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  it('shows success toast when triggered', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    fireEvent.click(screen.getByText('Show Success'));
    
    expect(screen.getByText('Success message')).toBeTruthy();
    expect(screen.getByRole('alert')).toBeTruthy();
  });

  it('shows error toast when triggered', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    fireEvent.click(screen.getByText('Show Error'));
    
    expect(screen.getByText('Error message')).toBeTruthy();
  });

  it('auto-dismisses toast after duration', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    fireEvent.click(screen.getByText('Show Success'));
    expect(screen.getByText('Success message')).toBeTruthy();

    // Fast-forward past the default 5 second duration
    act(() => {
      jest.advanceTimersByTime(5000);
    });

    expect(screen.queryByText('Success message')).toBeNull();
  });

  it('can dismiss toast manually', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    fireEvent.click(screen.getByText('Show Success'));
    expect(screen.getByText('Success message')).toBeTruthy();

    // Click dismiss button
    fireEvent.click(screen.getByLabelText('Dismiss notification'));

    expect(screen.queryByText('Success message')).toBeNull();
  });

  it('cleans up timer when toast is manually dismissed before auto-dismiss', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    // Show a toast with 5 second duration
    fireEvent.click(screen.getByText('Show Success'));
    expect(screen.getByText('Success message')).toBeTruthy();

    // Manually dismiss after 2 seconds
    act(() => {
      jest.advanceTimersByTime(2000);
    });
    fireEvent.click(screen.getByLabelText('Dismiss notification'));
    expect(screen.queryByText('Success message')).toBeNull();

    // Advance past the original 5 second duration
    // If timer wasn't cleaned up, this might cause issues
    act(() => {
      jest.advanceTimersByTime(4000);
    });

    // Toast should still be gone and no errors should occur
    expect(screen.queryByText('Success message')).toBeNull();
  });

  it('can show multiple toasts', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    fireEvent.click(screen.getByText('Show Success'));
    fireEvent.click(screen.getByText('Show Error'));

    expect(screen.getByText('Success message')).toBeTruthy();
    expect(screen.getByText('Error message')).toBeTruthy();
  });

  it('can dismiss toast with Escape key', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    fireEvent.click(screen.getByText('Show Success'));
    const toast = screen.getByRole('alert');
    expect(screen.getByText('Success message')).toBeTruthy();

    // Press Escape key
    fireEvent.keyDown(toast, { key: 'Escape' });

    expect(screen.queryByText('Success message')).toBeNull();
  });

  it('can dismiss toast with Enter key', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    fireEvent.click(screen.getByText('Show Info'));
    const toast = screen.getByRole('alert');
    expect(screen.getByText('Info message')).toBeTruthy();

    // Press Enter key
    fireEvent.keyDown(toast, { key: 'Enter' });

    expect(screen.queryByText('Info message')).toBeNull();
  });

  it('uses assertive aria-live for error toasts', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    // Initially no toasts, no container
    expect(screen.queryByRole('region')).toBeNull();

    // Show success toast - should use polite
    fireEvent.click(screen.getByText('Show Success'));
    let container = screen.getByRole('region');
    expect(container.getAttribute('aria-live')).toBe('polite');

    // Show error toast - should switch to assertive
    fireEvent.click(screen.getByText('Show Error'));
    container = screen.getByRole('region');
    expect(container.getAttribute('aria-live')).toBe('assertive');
  });
});
