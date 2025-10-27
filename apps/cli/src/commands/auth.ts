import axios from 'axios';
import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';
import * as readline from 'readline';

const API_BASE_URL = process.env.HICKORY_API_URL || 'http://localhost:5000';
const CONFIG_DIR = path.join(os.homedir(), '.hickory');
const CONFIG_FILE = path.join(CONFIG_DIR, 'config.json');

interface User {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

interface AuthConfig {
  accessToken: string;
  refreshToken: string;
  user: User;
}

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

function ensureConfigDir() {
  if (!fs.existsSync(CONFIG_DIR)) {
    fs.mkdirSync(CONFIG_DIR, { recursive: true });
  }
}

export function getConfig(): AuthConfig | null {
  try {
    if (!fs.existsSync(CONFIG_FILE)) {
      return null;
    }
    const data = fs.readFileSync(CONFIG_FILE, 'utf-8');
    return JSON.parse(data);
  } catch {
    return null;
  }
}

function saveConfig(config: AuthConfig) {
  ensureConfigDir();
  fs.writeFileSync(CONFIG_FILE, JSON.stringify(config, null, 2), 'utf-8');
}

function clearConfig() {
  if (fs.existsSync(CONFIG_FILE)) {
    fs.unlinkSync(CONFIG_FILE);
  }
}

function promptInput(question: string): Promise<string> {
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
  });

  return new Promise((resolve) => {
    rl.question(question, (answer) => {
      rl.close();
      resolve(answer);
    });
  });
}

function promptPassword(question: string): Promise<string> {
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
  });

  return new Promise((resolve) => {
    const stdin = process.stdin;
    
    // Only use raw mode if stdin is a TTY (interactive terminal)
    const isTTY = stdin.isTTY;
    if (isTTY) {
      stdin.setRawMode(true);
    }
    
    let password = '';
    process.stdout.write(question);
    
    // If not a TTY, use regular readline
    if (!isTTY) {
      rl.question('', (answer) => {
        rl.close();
        resolve(answer);
      });
      return;
    }
    
    stdin.on('data', (char: Buffer) => {
      const c = char.toString('utf-8');
      
      switch (c) {
        case '\n':
        case '\r':
        case '\u0004': // Ctrl+D
          stdin.setRawMode(false);
          stdin.pause();
          process.stdout.write('\n');
          rl.close();
          resolve(password);
          break;
        case '\u0003': // Ctrl+C
          process.exit();
          break;
        case '\u007f': // Backspace
          if (password.length > 0) {
            password = password.slice(0, -1);
          }
          break;
        default:
          password += c;
          break;
      }
    });
  });
}

export async function login(options: { email?: string; password?: string }) {
  try {
    let email = options.email;
    let password = options.password;

    // Prompt for missing credentials
    if (!email) {
      email = await promptInput('Email: ');
    }
    
    if (!password) {
      password = await promptPassword('Password: ');
    }

    console.log('Authenticating...');

    const response = await axios.post<LoginResponse>(`${API_BASE_URL}/api/auth/login`, {
      email,
      password,
    });

    const { accessToken, refreshToken, userId, firstName, lastName, role } = response.data;

    const config: AuthConfig = {
      accessToken,
      refreshToken,
      user: {
        userId,
        email: email || '',
        firstName,
        lastName,
        role,
      },
    };

    saveConfig(config);

    console.log('✓ Authentication successful!');
    console.log(`  Welcome, ${firstName} ${lastName}`);
    console.log(`  Role: ${role}`);
    console.log('\nCredentials saved to:', CONFIG_FILE);
  } catch (error: unknown) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 401) {
        console.error('✗ Authentication failed: Invalid email or password');
      } else if (error.response?.status === 400) {
        console.error('✗ Authentication failed:', error.response.data.message || 'Bad request');
      } else {
        console.error('✗ Authentication failed:', error.message);
      }
    } else {
      console.error('✗ An unexpected error occurred:', error instanceof Error ? error.message : 'Unknown error');
    }
    process.exit(1);
  }
}

export function logout() {
  const config = getConfig();
  
  if (!config) {
    console.log('Not currently authenticated.');
    return;
  }

  clearConfig();
  console.log('✓ Logged out successfully.');
  console.log(`  Goodbye, ${config.user.firstName}!`);
}
