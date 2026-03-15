import { renderHook } from '@testing-library/react';
import { useKeyboardShortcuts, formatShortcutKey } from '../../hooks/use-keyboard-shortcuts';

describe('useKeyboardShortcuts', () => {
  it('calls action when matching key is pressed', () => {
    const action = jest.fn();
    renderHook(() =>
      useKeyboardShortcuts([
        { key: '?', description: 'Help', category: 'General', action },
      ])
    );

    const event = new KeyboardEvent('keydown', { key: '?', bubbles: true });
    document.dispatchEvent(event);

    expect(action).toHaveBeenCalledTimes(1);
  });

  it('calls action for modifier shortcuts (Ctrl)', () => {
    const action = jest.fn();
    renderHook(() =>
      useKeyboardShortcuts([
        { key: 'k', ctrl: true, meta: true, description: 'Search', category: 'Nav', action },
      ])
    );

    const event = new KeyboardEvent('keydown', { key: 'k', ctrlKey: true, bubbles: true });
    document.dispatchEvent(event);

    expect(action).toHaveBeenCalledTimes(1);
  });

  it('calls action for modifier shortcuts (Meta/Cmd)', () => {
    const action = jest.fn();
    renderHook(() =>
      useKeyboardShortcuts([
        { key: 'n', ctrl: true, meta: true, description: 'New', category: 'Nav', action },
      ])
    );

    const event = new KeyboardEvent('keydown', { key: 'n', metaKey: true, bubbles: true });
    document.dispatchEvent(event);

    expect(action).toHaveBeenCalledTimes(1);
  });

  it('does not fire non-modifier shortcut when inside an input', () => {
    const action = jest.fn();
    renderHook(() =>
      useKeyboardShortcuts([
        { key: 't', description: 'Tickets', category: 'Nav', action },
      ])
    );

    // Simulate an input being focused
    const input = document.createElement('input');
    document.body.appendChild(input);
    input.focus();

    const event = new KeyboardEvent('keydown', { key: 't', bubbles: true });
    document.dispatchEvent(event);

    expect(action).not.toHaveBeenCalled();

    document.body.removeChild(input);
  });

  it('still fires modifier shortcut when inside an input', () => {
    const action = jest.fn();
    renderHook(() =>
      useKeyboardShortcuts([
        { key: 'k', ctrl: true, meta: true, description: 'Search', category: 'Nav', action },
      ])
    );

    const input = document.createElement('input');
    document.body.appendChild(input);
    input.focus();

    const event = new KeyboardEvent('keydown', { key: 'k', ctrlKey: true, bubbles: true });
    document.dispatchEvent(event);

    expect(action).toHaveBeenCalledTimes(1);

    document.body.removeChild(input);
  });

  it('does not fire when Ctrl is held but shortcut has no modifier', () => {
    const action = jest.fn();
    renderHook(() =>
      useKeyboardShortcuts([
        { key: 't', description: 'Tickets', category: 'Nav', action },
      ])
    );

    const event = new KeyboardEvent('keydown', { key: 't', ctrlKey: true, bubbles: true });
    document.dispatchEvent(event);

    expect(action).not.toHaveBeenCalled();
  });

  it('cleans up listener on unmount', () => {
    const action = jest.fn();
    const { unmount } = renderHook(() =>
      useKeyboardShortcuts([
        { key: '?', description: 'Help', category: 'General', action },
      ])
    );

    unmount();

    const event = new KeyboardEvent('keydown', { key: '?', bubbles: true });
    document.dispatchEvent(event);

    expect(action).not.toHaveBeenCalled();
  });
});

describe('formatShortcutKey', () => {
  it('formats a plain key', () => {
    const result = formatShortcutKey({ key: '?' });
    expect(result).toBe('?');
  });

  it('formats Escape key', () => {
    const result = formatShortcutKey({ key: 'Escape' });
    expect(result).toBe('Esc');
  });

  it('formats a modifier shortcut', () => {
    // jsdom userAgent does not contain "Mac", so we get Ctrl variant
    const result = formatShortcutKey({ key: 'k', ctrl: true, meta: true });
    expect(result).toBe('Ctrl + K');
  });

  it('formats shift + modifier shortcut', () => {
    const result = formatShortcutKey({ key: 'n', ctrl: true, meta: true, shift: true });
    expect(result).toBe('Ctrl + Shift + N');
  });
});
