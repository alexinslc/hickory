'use client';

import { useEffect, useCallback, useRef } from 'react';

export interface KeyboardShortcut {
  key: string;
  ctrl?: boolean;
  meta?: boolean;
  shift?: boolean;
  description: string;
  category: string;
  action: () => void;
}

/**
 * Check if the active element is an input field where shortcuts should be suppressed.
 */
function isEditableElement(): boolean {
  const el = document.activeElement;
  if (!el) return false;
  const tagName = el.tagName.toLowerCase();
  if (tagName === 'input' || tagName === 'textarea' || tagName === 'select') return true;
  if ((el as HTMLElement).isContentEditable) return true;
  return false;
}

/**
 * Check if a keyboard event matches a shortcut definition.
 */
function matchesShortcut(event: KeyboardEvent, shortcut: KeyboardShortcut): boolean {
  const requiresMod = shortcut.ctrl || shortcut.meta;

  if (requiresMod) {
    // For modifier shortcuts, accept either Ctrl or Meta (Cmd on Mac)
    const modPressed = event.ctrlKey || event.metaKey;
    if (!modPressed) return false;
  } else {
    // For non-modifier shortcuts, reject if Ctrl/Meta/Alt is held
    if (event.ctrlKey || event.metaKey || event.altKey) return false;
  }

  if (shortcut.shift && !event.shiftKey) return false;

  return event.key.toLowerCase() === shortcut.key.toLowerCase();
}

/**
 * Hook that registers global keyboard shortcuts.
 *
 * Shortcuts that use a modifier key (ctrl/meta) fire even inside editable fields.
 * Shortcuts without a modifier are suppressed when an input/textarea is focused.
 */
export function useKeyboardShortcuts(shortcuts: KeyboardShortcut[]) {
  const shortcutsRef = useRef(shortcuts);
  shortcutsRef.current = shortcuts;

  const handleKeyDown = useCallback((event: KeyboardEvent) => {
    for (const shortcut of shortcutsRef.current) {
      if (!matchesShortcut(event, shortcut)) continue;

      const requiresMod = shortcut.ctrl || shortcut.meta;

      // Skip non-modifier shortcuts when inside editable elements
      if (!requiresMod && isEditableElement()) continue;

      event.preventDefault();
      shortcut.action();
      return;
    }
  }, []);

  useEffect(() => {
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown]);
}

/**
 * Returns a human-readable label for a shortcut, adapting to the current OS.
 */
export function formatShortcutKey(shortcut: Pick<KeyboardShortcut, 'key' | 'ctrl' | 'meta' | 'shift'>): string {
  const isMac = typeof navigator !== 'undefined' && /Mac|iPhone|iPad/.test(navigator.userAgent);
  const parts: string[] = [];

  if (shortcut.ctrl || shortcut.meta) {
    parts.push(isMac ? '\u2318' : 'Ctrl');
  }
  if (shortcut.shift) {
    parts.push(isMac ? '\u21E7' : 'Shift');
  }

  const keyLabels: Record<string, string> = {
    '/': '/',
    escape: 'Esc',
    '?': '?',
  };

  const label = keyLabels[shortcut.key.toLowerCase()] ?? shortcut.key.toUpperCase();
  parts.push(label);

  return parts.join(isMac ? '' : ' + ');
}
