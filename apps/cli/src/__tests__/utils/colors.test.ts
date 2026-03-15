const RESET = '\x1b[0m';

describe('CLI Color Utilities', () => {
  const originalEnv = process.env;
  const originalIsTTY = process.stdout.isTTY;

  afterEach(() => {
    process.env = { ...originalEnv };
    Object.defineProperty(process.stdout, 'isTTY', { value: originalIsTTY, writable: true });
    jest.resetModules();
  });

  function loadColors() {
    // eslint-disable-next-line @typescript-eslint/no-require-imports
    return require('../../utils/colors');
  }

  describe('when colors are enabled (TTY)', () => {
    beforeEach(() => {
      delete process.env['NO_COLOR'];
      delete process.env['FORCE_COLOR'];
      Object.defineProperty(process.stdout, 'isTTY', { value: true, writable: true });
    });

    it('should wrap text with ANSI codes for basic colors', () => {
      const colors = loadColors();
      expect(colors.red('hello')).toBe(`\x1b[31mhello${RESET}`);
      expect(colors.green('hello')).toBe(`\x1b[32mhello${RESET}`);
      expect(colors.yellow('hello')).toBe(`\x1b[33mhello${RESET}`);
      expect(colors.blue('hello')).toBe(`\x1b[34mhello${RESET}`);
      expect(colors.cyan('hello')).toBe(`\x1b[36mhello${RESET}`);
      expect(colors.gray('hello')).toBe(`\x1b[90mhello${RESET}`);
      expect(colors.bold('hello')).toBe(`\x1b[1mhello${RESET}`);
      expect(colors.dim('hello')).toBe(`\x1b[2mhello${RESET}`);
    });

    it('should colorize ticket statuses correctly', () => {
      const colors = loadColors();
      expect(colors.colorizeStatus('Open')).toBe(`\x1b[32mOpen${RESET}`);
      expect(colors.colorizeStatus('InProgress')).toBe(`\x1b[33mInProgress${RESET}`);
      expect(colors.colorizeStatus('Resolved')).toBe(`\x1b[34mResolved${RESET}`);
      expect(colors.colorizeStatus('Closed')).toBe(`\x1b[90mClosed${RESET}`);
      expect(colors.colorizeStatus('Cancelled')).toBe(`\x1b[31mCancelled${RESET}`);
      expect(colors.colorizeStatus('Unknown')).toBe('Unknown');
    });

    it('should colorize ticket priorities correctly', () => {
      const colors = loadColors();
      expect(colors.colorizePriority('Critical')).toBe(`\x1b[31mCritical${RESET}`);
      expect(colors.colorizePriority('High')).toBe(`\x1b[35mHigh${RESET}`);
      expect(colors.colorizePriority('Medium')).toBe(`\x1b[33mMedium${RESET}`);
      expect(colors.colorizePriority('Low')).toBe(`\x1b[32mLow${RESET}`);
      expect(colors.colorizePriority('Unknown')).toBe('Unknown');
    });

    it('should provide semantic color helpers', () => {
      const colors = loadColors();
      expect(colors.success('ok')).toBe(colors.green('ok'));
      expect(colors.error('fail')).toBe(colors.red('fail'));
      expect(colors.warning('warn')).toBe(colors.yellow('warn'));
      expect(colors.info('note')).toBe(colors.blue('note'));
    });
  });

  describe('when NO_COLOR is set', () => {
    beforeEach(() => {
      process.env['NO_COLOR'] = '1';
      Object.defineProperty(process.stdout, 'isTTY', { value: true, writable: true });
    });

    it('should return plain text without ANSI codes', () => {
      const colors = loadColors();
      expect(colors.red('hello')).toBe('hello');
      expect(colors.bold('hello')).toBe('hello');
      expect(colors.colorizeStatus('Open')).toBe('Open');
      expect(colors.colorizePriority('Critical')).toBe('Critical');
    });
  });

  describe('when output is not a TTY', () => {
    beforeEach(() => {
      delete process.env['NO_COLOR'];
      delete process.env['FORCE_COLOR'];
      Object.defineProperty(process.stdout, 'isTTY', { value: false, writable: true });
    });

    it('should return plain text without ANSI codes', () => {
      const colors = loadColors();
      expect(colors.red('hello')).toBe('hello');
      expect(colors.bold('hello')).toBe('hello');
    });
  });

  describe('when FORCE_COLOR=1 is set on non-TTY', () => {
    beforeEach(() => {
      delete process.env['NO_COLOR'];
      process.env['FORCE_COLOR'] = '1';
      Object.defineProperty(process.stdout, 'isTTY', { value: false, writable: true });
    });

    it('should enable colors even without TTY', () => {
      const colors = loadColors();
      expect(colors.red('hello')).toBe(`\x1b[31mhello${RESET}`);
    });
  });
});
