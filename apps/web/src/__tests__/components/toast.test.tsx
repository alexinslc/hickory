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
});
