'use client';

import { createContext, useContext, useState, useCallback, useMemo } from 'react';
import { useRouter } from 'next/navigation';
import { useKeyboardShortcuts, KeyboardShortcut } from '@/hooks/use-keyboard-shortcuts';
import { KeyboardShortcutsDialog } from '@/components/ui/keyboard-shortcuts-dialog';

interface KeyboardShortcutsContextType {
  showHelp: () => void;
  hideHelp: () => void;
  isHelpOpen: boolean;
  shortcuts: KeyboardShortcut[];
}

const KeyboardShortcutsContext = createContext<KeyboardShortcutsContextType | undefined>(undefined);

export function KeyboardShortcutsProvider({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [isHelpOpen, setIsHelpOpen] = useState(false);

  const showHelp = useCallback(() => setIsHelpOpen(true), []);
  const hideHelp = useCallback(() => setIsHelpOpen(false), []);

  const shortcuts: KeyboardShortcut[] = useMemo(() => [
    {
      key: 'k',
      ctrl: true,
      meta: true,
      description: 'Focus search',
      category: 'Navigation',
      action: () => {
        const searchInput = document.querySelector<HTMLInputElement>('input[aria-label="Search for tickets"]');
        if (searchInput) {
          searchInput.focus();
          searchInput.select();
        } else {
          router.push('/search');
        }
      },
    },
    {
      key: 'n',
      ctrl: true,
      meta: true,
      description: 'New ticket',
      category: 'Navigation',
      action: () => {
        router.push('/tickets/new');
      },
    },
    {
      key: '/',
      ctrl: true,
      meta: true,
      description: 'Show keyboard shortcuts',
      category: 'General',
      action: () => {
        setIsHelpOpen((prev) => !prev);
      },
    },
    {
      key: '?',
      description: 'Show keyboard shortcuts',
      category: 'General',
      action: () => {
        setIsHelpOpen((prev) => !prev);
      },
    },
    {
      key: 'g',
      description: 'Go to dashboard',
      category: 'Navigation',
      action: () => {
        router.push('/dashboard');
      },
    },
    {
      key: 't',
      description: 'Go to tickets',
      category: 'Navigation',
      action: () => {
        router.push('/tickets');
      },
    },
  ], [router]);

  useKeyboardShortcuts(shortcuts);

  const contextValue = useMemo(
    () => ({ showHelp, hideHelp, isHelpOpen, shortcuts }),
    [showHelp, hideHelp, isHelpOpen, shortcuts]
  );

  return (
    <KeyboardShortcutsContext.Provider value={contextValue}>
      {children}
      <KeyboardShortcutsDialog
        isOpen={isHelpOpen}
        onClose={hideHelp}
        shortcuts={shortcuts}
      />
    </KeyboardShortcutsContext.Provider>
  );
}

export function useKeyboardShortcutsContext() {
  const context = useContext(KeyboardShortcutsContext);
  if (!context) {
    throw new Error('useKeyboardShortcutsContext must be used within a KeyboardShortcutsProvider');
  }
  return context;
}
