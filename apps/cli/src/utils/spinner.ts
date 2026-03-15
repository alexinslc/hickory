/**
 * A lightweight CLI spinner for showing progress during async operations.
 * No external dependencies required.
 */

const SPINNER_FRAMES = ['⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏'];
const FRAME_INTERVAL = 80;

const GREEN = '\x1b[32m';
const RED = '\x1b[31m';
const RESET = '\x1b[0m';

export interface Spinner {
  /** Update the spinner message while it is running */
  update(message: string): void;
  /** Stop with a success message (green checkmark) */
  succeed(message?: string): void;
  /** Stop with a failure message (red cross) */
  fail(message?: string): void;
  /** Stop the spinner without a status icon */
  stop(): void;
}

/**
 * Start a spinner with the given message.
 *
 * Usage:
 *   const spinner = startSpinner('Loading tickets...');
 *   try {
 *     const data = await fetchTickets();
 *     spinner.succeed('Tickets loaded');
 *   } catch (err) {
 *     spinner.fail('Failed to load tickets');
 *   }
 */
export function startSpinner(message: string): Spinner {
  let frameIndex = 0;
  let currentMessage = message;
  let stopped = false;

  // Hide cursor
  process.stderr.write('\x1b[?25l');

  const interval = setInterval(() => {
    const frame = SPINNER_FRAMES[frameIndex % SPINNER_FRAMES.length];
    process.stderr.write(`\r\x1b[K${frame} ${currentMessage}`);
    frameIndex++;
  }, FRAME_INTERVAL);

  function clearLine() {
    process.stderr.write('\r\x1b[K');
  }

  function stopSpinner() {
    if (stopped) return;
    stopped = true;
    clearInterval(interval);
    clearLine();
    // Show cursor
    process.stderr.write('\x1b[?25h');
  }

  return {
    update(msg: string) {
      currentMessage = msg;
    },
    succeed(msg?: string) {
      stopSpinner();
      console.log(`${GREEN}\u2714${RESET} ${msg ?? currentMessage}`);
    },
    fail(msg?: string) {
      stopSpinner();
      console.log(`${RED}\u2718${RESET} ${msg ?? currentMessage}`);
    },
    stop() {
      stopSpinner();
    },
  };
}
