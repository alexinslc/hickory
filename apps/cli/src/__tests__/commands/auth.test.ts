import axios from 'axios';
import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';

// Mock dependencies BEFORE importing the module under test
jest.mock('axios');
jest.mock('fs');
jest.mock('os', () => ({
  homedir: jest.fn(() => '/mock/home'),
}));
jest.mock('readline', () => ({
  createInterface: jest.fn(() => ({
    question: jest.fn((prompt: string, callback: (answer: string) => void) => {
      callback('test-input');
    }),
    close: jest.fn(),
  })),
}));

// Now import the module under test AFTER mocks are set up
import { login, logout, getConfig } from '../../commands/auth';

const mockedAxios = axios as jest.Mocked<typeof axios>;
const mockedFs = fs as jest.Mocked<typeof fs>;
const mockedOs = os as jest.Mocked<typeof os>;

describe('CLI Auth Commands', () => {
  const mockHomedir = '/mock/home';
  const mockConfigDir = path.join(mockHomedir, '.hickory');
  const mockConfigFile = path.join(mockConfigDir, 'config.json');
  const mockUser = {
    accessToken: 'mock-access-token',
    refreshToken: 'mock-refresh-token',
    userId: 'user-123',
    email: 'test@example.com',
    firstName: 'Test',
    lastName: 'User',
    role: 'User',
  };

  let consoleLogSpy: jest.SpyInstance;
  let consoleErrorSpy: jest.SpyInstance;
  let processExitSpy: jest.SpyInstance;

  beforeEach(() => {
    jest.clearAllMocks();
    
    // Mock console methods
    consoleLogSpy = jest.spyOn(console, 'log').mockImplementation();
    consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation();
    processExitSpy = jest.spyOn(process, 'exit').mockImplementation(() => undefined as never);
    
    // Setup default mocks
    mockedOs.homedir.mockReturnValue(mockHomedir);
    mockedFs.existsSync.mockReturnValue(false);
    mockedFs.mkdirSync.mockImplementation();
    mockedFs.writeFileSync.mockImplementation();
    mockedFs.readFileSync.mockImplementation();
    mockedFs.unlinkSync.mockImplementation();
    mockedAxios.isAxiosError.mockReturnValue(false);
  });

  afterEach(() => {
    consoleLogSpy.mockRestore();
    consoleErrorSpy.mockRestore();
    processExitSpy.mockRestore();
  });

  describe('login command', () => {
    it('should store credentials on successful login', async () => {
      const mockResponse = {
        data: mockUser,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any,
      };

      mockedAxios.post.mockResolvedValue(mockResponse);

      await login({ email: 'test@example.com', password: 'password123' });

      expect(mockedAxios.post).toHaveBeenCalledWith(
        expect.stringContaining('/api/auth/login'),
        {
          email: 'test@example.com',
          password: 'password123',
        }
      );

      expect(mockedFs.writeFileSync).toHaveBeenCalledWith(
        mockConfigFile,
        expect.stringContaining('"accessToken": "mock-access-token"'),
        'utf-8'
      );

      expect(consoleLogSpy).toHaveBeenCalledWith('✓ Authentication successful!');
      expect(consoleLogSpy).toHaveBeenCalledWith('  Welcome, Test User');
    });

    it('should display error message on 401 authentication failure', async () => {
      const mockError = {
        response: {
          status: 401,
          data: {},
        },
        isAxiosError: true,
      };

      mockedAxios.post.mockRejectedValue(mockError);
      mockedAxios.isAxiosError.mockReturnValue(true);

      await login({ email: 'wrong@example.com', password: 'wrongpass' });

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        '✗ Authentication failed: Invalid email or password'
      );
      expect(processExitSpy).toHaveBeenCalledWith(1);
    });

    it('should display error message on 400 bad request', async () => {
      const mockError = {
        response: {
          status: 400,
          data: { message: 'Email is required' },
        },
        isAxiosError: true,
      };

      mockedAxios.post.mockRejectedValue(mockError);
      mockedAxios.isAxiosError.mockReturnValue(true);

      await login({ email: '', password: 'password123' });

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        '✗ Authentication failed:',
        'Email is required'
      );
      expect(processExitSpy).toHaveBeenCalledWith(1);
    });

    it('should handle network errors', async () => {
      const mockError = {
        message: 'Network Error',
        isAxiosError: true,
      };

      mockedAxios.post.mockRejectedValue(mockError);
      mockedAxios.isAxiosError.mockReturnValue(true);

      await login({ email: 'test@example.com', password: 'password123' });

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        '✗ Authentication failed:',
        'Network Error'
      );
      expect(processExitSpy).toHaveBeenCalledWith(1);
    });

    it('should create config directory if it does not exist', async () => {
      const mockResponse = {
        data: mockUser,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any,
      };

      mockedAxios.post.mockResolvedValue(mockResponse);
      mockedFs.existsSync.mockReturnValue(false);

      await login({ email: 'test@example.com', password: 'password123' });

      expect(mockedFs.mkdirSync).toHaveBeenCalledWith(mockConfigDir, { recursive: true });
    });
  });

  describe('logout command', () => {
    it('should clear stored credentials', () => {
      mockedFs.existsSync.mockReturnValue(true);
      mockedFs.readFileSync.mockReturnValue(
        JSON.stringify({
          accessToken: 'token',
          refreshToken: 'refresh',
          user: mockUser,
        })
      );

      logout();

      expect(mockedFs.unlinkSync).toHaveBeenCalledWith(mockConfigFile);
      expect(consoleLogSpy).toHaveBeenCalledWith('✓ Logged out successfully.');
      expect(consoleLogSpy).toHaveBeenCalledWith('  Goodbye, Test!');
    });

    it('should display message when not logged in', () => {
      mockedFs.existsSync.mockReturnValue(false);

      logout();

      expect(mockedFs.unlinkSync).not.toHaveBeenCalled();
      expect(consoleLogSpy).toHaveBeenCalledWith('Not currently authenticated.');
    });
  });

  describe('getConfig function', () => {
    it('should return config when file exists', () => {
      const mockConfig = {
        accessToken: 'token',
        refreshToken: 'refresh',
        user: mockUser,
      };

      mockedFs.existsSync.mockReturnValue(true);
      mockedFs.readFileSync.mockReturnValue(JSON.stringify(mockConfig));

      const config = getConfig();

      expect(config).toEqual(mockConfig);
      expect(mockedFs.readFileSync).toHaveBeenCalledWith(mockConfigFile, 'utf-8');
    });

    it('should return null when file does not exist', () => {
      mockedFs.existsSync.mockReturnValue(false);

      const config = getConfig();

      expect(config).toBeNull();
    });

    it('should return null when file is corrupted', () => {
      mockedFs.existsSync.mockReturnValue(true);
      mockedFs.readFileSync.mockReturnValue('invalid json');

      const config = getConfig();

      expect(config).toBeNull();
    });
  });
});
