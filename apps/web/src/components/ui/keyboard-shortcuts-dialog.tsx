'use client';

import { useEffect, useRef } from 'react';
import { formatShortcutKey } from '@/hooks/use-keyboard-shortcuts';

interface ShortcutEntry {
  key: string;
  ctrl?: boolean;
  meta?: boolean;
  shift?: boolean;
  description: string;
  category: string;
}

interface KeyboardShortcutsDialogProps {
  isOpen: boolean;
  onClose: () => void;
  shortcuts: ShortcutEntry[];
}

export function KeyboardShortcutsDialog({ isOpen, onClose, shortcuts }: KeyboardShortcutsDialogProps) {
  const dialogRef = useRef<HTMLDivElement>(null);

  // Focus trap and Escape to close
  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        e.preventDefault();
        onClose();
      }
    };

    document.addEventListener('keydown', handleKeyDown);

    // Focus the dialog on open
    dialogRef.current?.focus();

    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  // Group shortcuts by category
  const grouped = shortcuts.reduce<Record<string, ShortcutEntry[]>>((acc, shortcut) => {
    if (!acc[shortcut.category]) acc[shortcut.category] = [];
    acc[shortcut.category].push(shortcut);
    return acc;
  }, {});

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/50 z-50"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Dialog */}
      <div className="fixed inset-0 z-50 overflow-y-auto">
        <div className="flex min-h-full items-center justify-center p-4">
          <div
            ref={dialogRef}
            role="dialog"
            aria-modal="true"
            aria-label="Keyboard shortcuts"
            tabIndex={-1}
            className="relative bg-white dark:bg-gray-900 rounded-lg shadow-xl max-w-lg w-full outline-none border border-gray-200 dark:border-gray-700"
          >
            {/* Header */}
            <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
                Keyboard Shortcuts
              </h2>
              <button
                onClick={onClose}
                className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                aria-label="Close keyboard shortcuts dialog"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            {/* Content */}
            <div className="px-6 py-4 max-h-96 overflow-y-auto">
              {Object.entries(grouped).map(([category, items]) => (
                <div key={category} className="mb-6 last:mb-0">
                  <h3 className="text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-3">
                    {category}
                  </h3>
                  <ul className="space-y-2">
                    {items.map((shortcut) => (
                      <li
                        key={`${shortcut.key}-${shortcut.ctrl ? 'ctrl' : ''}`}
                        className="flex items-center justify-between"
                      >
                        <span className="text-sm text-gray-700 dark:text-gray-300">
                          {shortcut.description}
                        </span>
                        <kbd className="inline-flex items-center gap-1 px-2 py-1 text-xs font-mono font-medium text-gray-600 dark:text-gray-300 bg-gray-100 dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded">
                          {formatShortcutKey(shortcut)}
                        </kbd>
                      </li>
                    ))}
                  </ul>
                </div>
              ))}
            </div>

            {/* Footer */}
            <div className="px-6 py-3 border-t border-gray-200 dark:border-gray-700">
              <p className="text-xs text-gray-500 dark:text-gray-400 text-center">
                Press <kbd className="px-1 py-0.5 text-xs font-mono bg-gray-100 dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded">Esc</kbd> to close
              </p>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
