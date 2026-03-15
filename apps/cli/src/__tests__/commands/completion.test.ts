import { generateCompletion } from '../../commands/completion';

describe('CLI Completion Commands', () => {
  let stdoutWriteSpy: jest.SpyInstance;
  let stderrSpy: jest.SpyInstance;
  let exitSpy: jest.SpyInstance;

  beforeEach(() => {
    stdoutWriteSpy = jest.spyOn(process.stdout, 'write').mockImplementation(() => true);
    stderrSpy = jest.spyOn(console, 'error').mockImplementation(() => undefined);
    exitSpy = jest.spyOn(process, 'exit').mockImplementation(() => undefined as never);
  });

  afterEach(() => {
    stdoutWriteSpy.mockRestore();
    stderrSpy.mockRestore();
    exitSpy.mockRestore();
  });

  describe('generateCompletion', () => {
    it('should output bash completion script', () => {
      generateCompletion('bash');

      expect(stdoutWriteSpy).toHaveBeenCalledTimes(1);
      const output = stdoutWriteSpy.mock.calls[0][0] as string;
      expect(output).toContain('_hickory_completions');
      expect(output).toContain('complete -F _hickory_completions hickory');
      expect(output).toContain('login logout whoami ticket agent completion help');
    });

    it('should output zsh completion script', () => {
      generateCompletion('zsh');

      expect(stdoutWriteSpy).toHaveBeenCalledTimes(1);
      const output = stdoutWriteSpy.mock.calls[0][0] as string;
      expect(output).toContain('#compdef hickory');
      expect(output).toContain('_hickory');
      expect(output).toContain('login:Authenticate with the Hickory API');
    });

    it('should include all top-level commands in bash script', () => {
      generateCompletion('bash');

      const output = stdoutWriteSpy.mock.calls[0][0] as string;
      expect(output).toContain('login');
      expect(output).toContain('logout');
      expect(output).toContain('whoami');
      expect(output).toContain('ticket');
      expect(output).toContain('agent');
      expect(output).toContain('completion');
    });

    it('should include subcommands in bash script', () => {
      generateCompletion('bash');

      const output = stdoutWriteSpy.mock.calls[0][0] as string;
      expect(output).toContain('create view list');
      expect(output).toContain('queue assign close');
    });

    it('should include subcommands in zsh script', () => {
      generateCompletion('zsh');

      const output = stdoutWriteSpy.mock.calls[0][0] as string;
      expect(output).toContain("'create:Create a new support ticket interactively'");
      expect(output).toContain("'view:View details of a specific ticket'");
      expect(output).toContain("'list:List all your tickets'");
      expect(output).toContain("'queue:View the agent ticket queue'");
      expect(output).toContain("'assign:Assign a ticket to yourself'");
      expect(output).toContain("'close:Close a ticket with resolution notes'");
    });

    it('should include agent queue filter options in bash script', () => {
      generateCompletion('bash');

      const output = stdoutWriteSpy.mock.calls[0][0] as string;
      expect(output).toContain('all unassigned mine');
      expect(output).toContain('-f --filter');
    });

    it('should include agent queue filter options in zsh script', () => {
      generateCompletion('zsh');

      const output = stdoutWriteSpy.mock.calls[0][0] as string;
      expect(output).toContain("'all:Show all tickets'");
      expect(output).toContain("'unassigned:Show only unassigned tickets'");
      expect(output).toContain("'mine:Show only tickets assigned to me'");
    });

    it('should error for unsupported shell', () => {
      generateCompletion('fish');

      expect(stderrSpy).toHaveBeenCalledWith('Unsupported shell: fish');
      expect(stderrSpy).toHaveBeenCalledWith('Supported shells: bash, zsh');
      expect(exitSpy).toHaveBeenCalledWith(1);
    });
  });
});
