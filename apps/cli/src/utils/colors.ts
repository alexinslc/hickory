/**
 * Centralized color utilities for the Hickory CLI.
 *
 * Respects the NO_COLOR environment variable (https://no-color.org/)
 * and detects non-TTY output to gracefully disable colors.
 */

const isColorEnabled =
  !('NO_COLOR' in process.env) &&
  process.env['FORCE_COLOR'] !== '0' &&
  (process.stdout.isTTY || Number(process.env['FORCE_COLOR']) >= 1);

// ANSI escape codes
const codes = {
  reset: '\x1b[0m',
  bold: '\x1b[1m',
  dim: '\x1b[2m',
  red: '\x1b[31m',
  green: '\x1b[32m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m',
  magenta: '\x1b[35m',
  cyan: '\x1b[36m',
  gray: '\x1b[90m',
} as const;

function wrap(code: string, text: string): string {
  if (!isColorEnabled) return text;
  return `${code}${text}${codes.reset}`;
}

// ── Basic colors ──────────────────────────────────────────────

export const red = (text: string) => wrap(codes.red, text);
export const green = (text: string) => wrap(codes.green, text);
export const yellow = (text: string) => wrap(codes.yellow, text);
export const blue = (text: string) => wrap(codes.blue, text);
export const magenta = (text: string) => wrap(codes.magenta, text);
export const cyan = (text: string) => wrap(codes.cyan, text);
export const gray = (text: string) => wrap(codes.gray, text);
export const bold = (text: string) => wrap(codes.bold, text);
export const dim = (text: string) => wrap(codes.dim, text);

// ── Semantic helpers ──────────────────────────────────────────

export const success = (text: string) => green(text);
export const error = (text: string) => red(text);
export const warning = (text: string) => yellow(text);
export const info = (text: string) => blue(text);

// ── Ticket status colors ──────────────────────────────────────
// Open=green, InProgress=yellow, Resolved=blue, Closed=gray

export function colorizeStatus(status: string): string {
  switch (status.trim().toLowerCase()) {
    case 'open':
      return green(status);
    case 'inprogress':
      return yellow(status);
    case 'resolved':
      return blue(status);
    case 'closed':
      return gray(status);
    case 'cancelled':
      return red(status);
    default:
      return status;
  }
}

// ── Ticket priority colors ────────────────────────────────────
// Critical=red+bold, High=red, Medium=yellow, Low=green

export function colorizePriority(priority: string): string {
  switch (priority.trim().toLowerCase()) {
    case 'critical':
      return bold(red(priority));
    case 'high':
      return red(priority);
    case 'medium':
      return yellow(priority);
    case 'low':
      return green(priority);
    default:
      return priority;
  }
}
