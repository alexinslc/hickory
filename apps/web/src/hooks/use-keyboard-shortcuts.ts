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
 *
 * Modifier matching is strict: each modifier flag must match exactly.
 * A `mod` shortcut (ctrl OR meta) is represented by setting both `ctrl`
 * and `meta` to true -- the matcher then accepts either modifier key.
 */
function matchesShortcut(event: KeyboardEvent, shortcut: KeyboardShortcut): boolean {
  // When both ctrl and meta are specified, treat as a "mod" shortcut that
  // accepts either Ctrl or Cmd (Meta). Otherwise enforce each flag exactly.
  const isMod = shortcut.ctrl && shortcut.meta;

  if (isMod) {
    // At least one of Ctrl / Meta must be pressed
    if (!event.ctrlKey && !event.metaKey) return false;
  } else {
    // Enforce ctrl exactly
    if (!!shortcut.ctrl !== event.ctrlKey) return false;
    // Enforce meta exactly
    if (!!shortcut.meta !== event.metaKey) return false;
  }

  // Enforce shift exactly so Ctrl+Shift+K does not trigger a Ctrl+K shortcut
  if (!!shortcut.shift !== event.shiftKey) return false;

  // Reject if Alt is held (unless we add alt support later)
  if (event.altKey) return false;

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
