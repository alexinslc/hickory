import { render, screen, fireEvent } from '@testing-library/react';
import { KeyboardShortcutsDialog } from '@/components/ui/keyboard-shortcuts-dialog';

const mockShortcuts = [
  { key: 'k', ctrl: true, meta: true, description: 'Focus search', category: 'Navigation' },
  { key: 'n', ctrl: true, meta: true, description: 'New ticket', category: 'Navigation' },
  { key: '?', description: 'Show shortcuts', category: 'General' },
  { key: 'g', description: 'Go to dashboard', category: 'Navigation' },
];

describe('KeyboardShortcutsDialog', () => {
  it('renders nothing when closed', () => {
    render(
      <KeyboardShortcutsDialog isOpen={false} onClose={jest.fn()} shortcuts={mockShortcuts} />
    );

    expect(screen.queryByRole('dialog')).toBeNull();
  });

  it('renders dialog when open', () => {
    render(
      <KeyboardShortcutsDialog isOpen={true} onClose={jest.fn()} shortcuts={mockShortcuts} />
    );

    expect(screen.getByRole('dialog')).toBeTruthy();
    expect(screen.getByText('Keyboard Shortcuts')).toBeTruthy();
  });

  it('displays all shortcuts grouped by category', () => {
    render(
      <KeyboardShortcutsDialog isOpen={true} onClose={jest.fn()} shortcuts={mockShortcuts} />
    );

    expect(screen.getByText('Navigation')).toBeTruthy();
    expect(screen.getByText('General')).toBeTruthy();
    expect(screen.getByText('Focus search')).toBeTruthy();
    expect(screen.getByText('New ticket')).toBeTruthy();
    expect(screen.getByText('Show shortcuts')).toBeTruthy();
    expect(screen.getByText('Go to dashboard')).toBeTruthy();
  });

  it('calls onClose when close button is clicked', () => {
    const onClose = jest.fn();
    render(
      <KeyboardShortcutsDialog isOpen={true} onClose={onClose} shortcuts={mockShortcuts} />
    );

    fireEvent.click(screen.getByLabelText('Close keyboard shortcuts dialog'));

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('calls onClose when backdrop is clicked', () => {
    const onClose = jest.fn();
    const { container } = render(
      <KeyboardShortcutsDialog isOpen={true} onClose={onClose} shortcuts={mockShortcuts} />
    );

    // Click the backdrop (first child with aria-hidden)
    const backdrop = container.querySelector('[aria-hidden="true"]');
    if (backdrop) fireEvent.click(backdrop);

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('calls onClose when Escape is pressed', () => {
    const onClose = jest.fn();
    render(
      <KeyboardShortcutsDialog isOpen={true} onClose={onClose} shortcuts={mockShortcuts} />
    );

    fireEvent.keyDown(document, { key: 'Escape' });

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('has proper aria attributes', () => {
    render(
      <KeyboardShortcutsDialog isOpen={true} onClose={jest.fn()} shortcuts={mockShortcuts} />
    );

    const dialog = screen.getByRole('dialog');
    expect(dialog.getAttribute('aria-modal')).toBe('true');
    expect(dialog.getAttribute('aria-label')).toBe('Keyboard shortcuts');
  });
});
