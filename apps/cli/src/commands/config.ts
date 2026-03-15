import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';

const CONFIG_DIR = path.join(os.homedir(), '.hickory');
const SETTINGS_FILE = path.join(CONFIG_DIR, 'settings.json');

// ANSI color codes
const RESET = '\x1b[0m';
const BOLD = '\x1b[1m';
const DIM = '\x1b[2m';
const GREEN = '\x1b[32m';
const YELLOW = '\x1b[33m';
const CYAN = '\x1b[36m';

export interface CliSettings {
  apiUrl: string;
  outputFormat: 'text' | 'json';
  colorEnabled: boolean;
}

const DEFAULT_SETTINGS: CliSettings = {
  apiUrl: 'http://localhost:5000',
  outputFormat: 'text',
  colorEnabled: true,
};

const VALID_KEYS = Object.keys(DEFAULT_SETTINGS) as (keyof CliSettings)[];

const SETTING_DESCRIPTIONS: Record<keyof CliSettings, string> = {
  apiUrl: 'Base URL for the Hickory API',
  outputFormat: 'Default output format (text, json)',
  colorEnabled: 'Enable colored output (true, false)',
};

function ensureConfigDir(): void {
  if (!fs.existsSync(CONFIG_DIR)) {
    fs.mkdirSync(CONFIG_DIR, { recursive: true });
  }
}

export function loadSettings(): CliSettings {
  try {
    if (!fs.existsSync(SETTINGS_FILE)) {
      return { ...DEFAULT_SETTINGS };
    }
    const data = fs.readFileSync(SETTINGS_FILE, 'utf-8');
    const parsed = JSON.parse(data);
    return { ...DEFAULT_SETTINGS, ...parsed };
  } catch {
    return { ...DEFAULT_SETTINGS };
  }
}

function saveSettings(settings: CliSettings): void {
  ensureConfigDir();
  fs.writeFileSync(SETTINGS_FILE, JSON.stringify(settings, null, 2), 'utf-8');
}

function isValidKey(key: string): key is keyof CliSettings {
  return VALID_KEYS.includes(key as keyof CliSettings);
}

function validateValue(key: keyof CliSettings, value: string): { valid: boolean; message?: string; parsed?: string | boolean } {
  switch (key) {
    case 'apiUrl': {
      try {
        new URL(value);
        return { valid: true, parsed: value };
      } catch {
        return { valid: false, message: `Invalid URL: "${value}". Please provide a valid URL (e.g., http://localhost:5000).` };
      }
    }
    case 'outputFormat': {
      const allowed = ['text', 'json'];
      if (!allowed.includes(value)) {
        return { valid: false, message: `Invalid output format: "${value}". Allowed values: ${allowed.join(', ')}.` };
      }
      return { valid: true, parsed: value };
    }
    case 'colorEnabled': {
      const truthy = ['true', '1', 'yes'];
      const falsy = ['false', '0', 'no'];
      if (truthy.includes(value.toLowerCase())) {
        return { valid: true, parsed: true };
      }
      if (falsy.includes(value.toLowerCase())) {
        return { valid: true, parsed: false };
      }
      return { valid: false, message: `Invalid boolean value: "${value}". Use true/false, yes/no, or 1/0.` };
    }
    default:
      return { valid: false, message: `Unknown setting: "${key}".` };
  }
}

/**
 * List all configuration settings
 */
export function configList(): void {
  const settings = loadSettings();

  console.log(`\n${BOLD}Hickory CLI Configuration${RESET}`);
  console.log(`${DIM}Settings file: ${SETTINGS_FILE}${RESET}\n`);

  for (const key of VALID_KEYS) {
    const value = settings[key];
    const description = SETTING_DESCRIPTIONS[key];
    const defaultValue = DEFAULT_SETTINGS[key];
    const isDefault = JSON.stringify(value) === JSON.stringify(defaultValue);

    console.log(`${BOLD}${key}${RESET} = ${CYAN}${value}${RESET}${isDefault ? ` ${DIM}(default)${RESET}` : ''}`);
    console.log(`  ${DIM}${description}${RESET}`);
  }

  console.log(`\n${DIM}Use "hickory config set <key> <value>" to change a setting.${RESET}`);
  console.log(`${DIM}Use "hickory config reset" to restore defaults.${RESET}\n`);
}

/**
 * Get a specific configuration value
 */
export function configGet(key: string): void {
  if (!isValidKey(key)) {
    console.error(`Error: Unknown configuration key "${key}".`);
    console.error(`Valid keys: ${VALID_KEYS.join(', ')}`);
    process.exit(1);
  }

  const settings = loadSettings();
  console.log(settings[key]);
}

/**
 * Set a configuration value
 */
export function configSet(key: string, value: string): void {
  if (!isValidKey(key)) {
    console.error(`Error: Unknown configuration key "${key}".`);
    console.error(`Valid keys: ${VALID_KEYS.join(', ')}`);
    process.exit(1);
  }

  const validation = validateValue(key, value);
  if (!validation.valid) {
    console.error(`Error: ${validation.message}`);
    process.exit(1);
  }

  const settings = loadSettings();
  (settings as Record<string, unknown>)[key] = validation.parsed;
  saveSettings(settings);

  console.log(`${GREEN}✓${RESET} Set ${BOLD}${key}${RESET} = ${CYAN}${validation.parsed}${RESET}`);
}

/**
 * Reset all configuration to defaults
 */
export function configReset(): void {
  saveSettings({ ...DEFAULT_SETTINGS });
  console.log(`${GREEN}✓${RESET} Configuration reset to defaults.`);
  console.log('');
  for (const key of VALID_KEYS) {
    console.log(`  ${BOLD}${key}${RESET} = ${CYAN}${DEFAULT_SETTINGS[key]}${RESET}`);
  }
  console.log('');
}
