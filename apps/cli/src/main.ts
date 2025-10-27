#!/usr/bin/env node

import { Command } from 'commander';
import { login, logout, getConfig } from './commands/auth';

const program = new Command();

program
  .name('hickory')
  .description('Hickory Help Desk CLI')
  .version('1.0.0');

// Auth commands
program
  .command('login')
  .description('Authenticate with the Hickory API')
  .option('-e, --email <email>', 'User email')
  .option('-p, --password <password>', 'User password')
  .action(login);

program
  .command('logout')
  .description('Clear authentication credentials')
  .action(logout);

program
  .command('whoami')
  .description('Show current authenticated user')
  .action(() => {
    const config = getConfig();
    if (!config || !config.user) {
      console.log('Not authenticated. Run "hickory login" to authenticate.');
      return;
    }
    console.log('Authenticated as:');
    console.log(`  Name: ${config.user.firstName} ${config.user.lastName}`);
    console.log(`  Email: ${config.user.email}`);
    console.log(`  Role: ${config.user.role}`);
    console.log(`  User ID: ${config.user.userId}`);
  });

program.parse(process.argv);
