import { render, screen, fireEvent, act } from '@testing-library/react';
import { ThemeProvider, useTheme } from '@/providers/theme-provider';

// Mock matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(), // deprecated
    removeListener: jest.fn(), // deprecated
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
});

// Test component to interact with theme
function TestComponent() {
  const { theme, setTheme, resolvedTheme } = useTheme();
  return (
    <div>
      <span data-testid="theme">{theme}</span>
      <span data-testid="resolved">{resolvedTheme}</span>
      <button onClick={() => setTheme('dark')}>Set Dark</button>
      <button onClick={() => setTheme('light')}>Set Light</button>
      <button onClick={() => setTheme('system')}>Set System</button>
    </div>
  );
}

describe('ThemeProvider', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove('dark');
  });

  it('defaults to system theme', () => {
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );

    expect(screen.getByTestId('theme').textContent).toBe('system');
  });

  it('can switch to dark theme', () => {
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );

    fireEvent.click(screen.getByText('Set Dark'));
    
    expect(screen.getByTestId('theme').textContent).toBe('dark');
    expect(localStorage.getItem('hickory-theme')).toBe('dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });

  it('can switch to light theme', () => {
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );

    fireEvent.click(screen.getByText('Set Light'));
    
    expect(screen.getByTestId('theme').textContent).toBe('light');
    expect(localStorage.getItem('hickory-theme')).toBe('light');
    expect(document.documentElement.classList.contains('dark')).toBe(false);
  });

  it('persists theme preference in localStorage', () => {
    localStorage.setItem('hickory-theme', 'dark');
    
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );

    expect(screen.getByTestId('theme').textContent).toBe('dark');
  });

  it('validates localStorage value and falls back to system for invalid themes', () => {
    localStorage.setItem('hickory-theme', 'invalid-theme');
    
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );

    expect(screen.getByTestId('theme').textContent).toBe('system');
  });

  it('detects system theme changes when theme is set to system', () => {
    let mediaQueryListeners: Array<(event: any) => void> = [];
    
    // Mock matchMedia to track listeners
    const mockMatchMedia = jest.fn().mockImplementation(query => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: jest.fn(),
      removeListener: jest.fn(),
      addEventListener: jest.fn((event, listener) => {
        if (event === 'change') {
          mediaQueryListeners.push(listener);
        }
      }),
      removeEventListener: jest.fn((event, listener) => {
        if (event === 'change') {
          mediaQueryListeners = mediaQueryListeners.filter(l => l !== listener);
        }
      }),
      dispatchEvent: jest.fn(),
    }));
    
    window.matchMedia = mockMatchMedia;
    
    render(
      <ThemeProvider>
        <TestComponent />
      </ThemeProvider>
    );

    // Set theme to system
    fireEvent.click(screen.getByText('Set System'));
    
    // Verify initial state
    expect(screen.getByTestId('theme').textContent).toBe('system');
    expect(screen.getByTestId('resolved').textContent).toBe('light');
    expect(document.documentElement.classList.contains('dark')).toBe(false);
    
    // Simulate system theme change to dark
    mockMatchMedia.mockImplementation(query => ({
      matches: true,
      media: query,
      onchange: null,
      addListener: jest.fn(),
      removeListener: jest.fn(),
      addEventListener: jest.fn((event, listener) => {
        if (event === 'change') {
          mediaQueryListeners.push(listener);
        }
      }),
      removeEventListener: jest.fn((event, listener) => {
        if (event === 'change') {
          mediaQueryListeners = mediaQueryListeners.filter(l => l !== listener);
        }
      }),
      dispatchEvent: jest.fn(),
    }));
    
    // Trigger the media query change event
    act(() => {
      mediaQueryListeners.forEach(listener => listener({ matches: true }));
    });
    
    // Verify theme updated to dark
    expect(screen.getByTestId('resolved').textContent).toBe('dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });
});
