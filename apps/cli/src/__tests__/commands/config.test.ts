import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';

// Mock dependencies BEFORE importing the module under test
jest.mock('fs');
jest.mock('os', () => ({
  homedir: jest.fn(() => '/mock/home'),
}));

// Now import the module under test AFTER mocks are set up
import { configList, configGet, configSet, configReset, loadSettings } from '../../commands/config';

const mockedFs = fs as jest.Mocked<typeof fs>;
const mockedOs = os as jest.Mocked<typeof os>;

describe('CLI Config Commands', () => {
  const mockHomedir = '/mock/home';
  const mockConfigDir = path.join(mockHomedir, '.hickory');
  const mockSettingsFile = path.join(mockConfigDir, 'settings.json');

  let consoleLogSpy: jest.SpyInstance;
  let consoleErrorSpy: jest.SpyInstance;
  let processExitSpy: jest.SpyInstance;

  beforeEach(() => {
    jest.clearAllMocks();

    consoleLogSpy = jest.spyOn(console, 'log').mockImplementation();
    consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation();
    processExitSpy = jest.spyOn(process, 'exit').mockImplementation(() => undefined as never);

    mockedOs.homedir.mockReturnValue(mockHomedir);
    mockedFs.existsSync.mockReturnValue(false);
    mockedFs.mkdirSync.mockImplementation();
    mockedFs.writeFileSync.mockImplementation();
    mockedFs.readFileSync.mockImplementation();
  });

  afterEach(() => {
    consoleLogSpy.mockRestore();
    consoleErrorSpy.mockRestore();
    processExitSpy.mockRestore();
  });

  describe('loadSettings', () => {
    it('should return default settings when no file exists', () => {
      mockedFs.existsSync.mockReturnValue(false);

      const settings = loadSettings();

      expect(settings).toEqual({
        apiUrl: 'http://localhost:5000',
        outputFormat: 'text',
        colorEnabled: true,
      });
    });

    it('should return saved settings when file exists', () => {
      const saved = {
        apiUrl: 'https://api.example.com',
        outputFormat: 'json',
        colorEnabled: false,
      };

      mockedFs.existsSync.mockReturnValue(true);
      mockedFs.readFileSync.mockReturnValue(JSON.stringify(saved));

      const settings = loadSettings();

      expect(settings).toEqual(saved);
    });

    it('should merge saved settings with defaults for missing keys', () => {
      const partial = { apiUrl: 'https://api.example.com' };

      mockedFs.existsSync.mockReturnValue(true);
      mockedFs.readFileSync.mockReturnValue(JSON.stringify(partial));

      const settings = loadSettings();

      expect(settings.apiUrl).toBe('https://api.example.com');
      expect(settings.outputFormat).toBe('text');
      expect(settings.colorEnabled).toBe(true);
    });

    it('should return defaults when file is corrupted', () => {
      mockedFs.existsSync.mockReturnValue(true);
      mockedFs.readFileSync.mockReturnValue('invalid json');

      const settings = loadSettings();

      expect(settings).toEqual({
        apiUrl: 'http://localhost:5000',
        outputFormat: 'text',
        colorEnabled: true,
      });
    });
  });

  describe('configList', () => {
    it('should display all settings', () => {
      mockedFs.existsSync.mockReturnValue(false);

      configList();

      expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringContaining('Hickory CLI Configuration'));
      expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringContaining('apiUrl'));
      expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringContaining('outputFormat'));
      expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringContaining('colorEnabled'));
    });
  });

  describe('configGet', () => {
    it('should display the value for a valid key', () => {
      mockedFs.existsSync.mockReturnValue(false);

      configGet('apiUrl');

      expect(consoleLogSpy).toHaveBeenCalledWith('http://localhost:5000');
    });

    it('should display a saved value', () => {
      const saved = { apiUrl: 'https://api.example.com' };
      mockedFs.existsSync.mockReturnValue(true);
      mockedFs.readFileSync.mockReturnValue(JSON.stringify(saved));

      configGet('apiUrl');

      expect(consoleLogSpy).toHaveBeenCalledWith('https://api.example.com');
    });

    it('should exit with error for invalid key', () => {
      configGet('invalidKey');

      expect(consoleErrorSpy).toHaveBeenCalledWith(expect.stringContaining('Unknown configuration key'));
      expect(processExitSpy).toHaveBeenCalledWith(1);
    });
  });

  describe('configSet', () => {
    it('should save a valid apiUrl', () => {
      mockedFs.existsSync.mockReturnValue(false);

      configSet('apiUrl', 'https://api.example.com');

      expect(mockedFs.writeFileSync).toHaveBeenCalledWith(
        mockSettingsFile,
        expect.stringContaining('"apiUrl": "https://api.example.com"'),
        'utf-8'
      );
      expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringContaining('apiUrl'));
    });

    it('should save a valid outputFormat', () => {
      mockedFs.existsSync.mockReturnValue(false);

      configSet('outputFormat', 'json');

      expect(mockedFs.writeFileSync).toHaveBeenCalledWith(
        mockSettingsFile,
        expect.stringContaining('"outputFormat": "json"'),
        'utf-8'
      );
    });

    it('should save colorEnabled as boolean true', () => {
      mockedFs.existsSync.mockReturnValue(false);

      configSet('colorEnabled', 'true');

      expect(mockedFs.writeFileSync).toHaveBeenCalledWith(
        mockSettingsFile,
        expect.stringContaining('"colorEnabled": true'),
        'utf-8'
      );
    });

    it('should save colorEnabled as boolean false', () => {
      mockedFs.existsSync.mockReturnValue(false);

      configSet('colorEnabled', 'false');

      expect(mockedFs.writeFileSync).toHaveBeenCalledWith(
        mockSettingsFile,
        expect.stringContaining('"colorEnabled": false'),
        'utf-8'
      );
    });

    it('should accept yes/no for colorEnabled', () => {
      mockedFs.existsSync.mockReturnValue(false);

      configSet('colorEnabled', 'yes');

      expect(mockedFs.writeFileSync).toHaveBeenCalledWith(
        mockSettingsFile,
        expect.stringContaining('"colorEnabled": true'),
        'utf-8'
      );
    });

    it('should reject invalid apiUrl', () => {
      configSet('apiUrl', 'not-a-url');

      expect(consoleErrorSpy).toHaveBeenCalledWith(expect.stringContaining('Invalid URL'));
      expect(processExitSpy).toHaveBeenCalledWith(1);
    });

    it('should reject invalid outputFormat', () => {
      configSet('outputFormat', 'xml');

      expect(consoleErrorSpy).toHaveBeenCalledWith(expect.stringContaining('Invalid output format'));
      expect(processExitSpy).toHaveBeenCalledWith(1);
    });

    it('should reject invalid colorEnabled value', () => {
      configSet('colorEnabled', 'maybe');

      expect(consoleErrorSpy).toHaveBeenCalledWith(expect.stringContaining('Invalid boolean value'));
      expect(processExitSpy).toHaveBeenCalledWith(1);
    });

    it('should reject unknown key', () => {
      configSet('unknownKey', 'value');

      expect(consoleErrorSpy).toHaveBeenCalledWith(expect.stringContaining('Unknown configuration key'));
      expect(processExitSpy).toHaveBeenCalledWith(1);
    });

    it('should create config directory if it does not exist', () => {
      mockedFs.existsSync.mockReturnValue(false);

      configSet('apiUrl', 'https://api.example.com');

      expect(mockedFs.mkdirSync).toHaveBeenCalledWith(mockConfigDir, { recursive: true });
    });
  });

  describe('configReset', () => {
    it('should reset settings to defaults', () => {
      mockedFs.existsSync.mockReturnValue(false);

      configReset();

      expect(mockedFs.writeFileSync).toHaveBeenCalledWith(
        mockSettingsFile,
        expect.stringContaining('"apiUrl": "http://localhost:5000"'),
        'utf-8'
      );
      expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringContaining('reset to defaults'));
    });
  });
});
