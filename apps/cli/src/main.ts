#!/usr/bin/env node

import { Command } from 'commander';
import { login, logout, getConfig } from './commands/auth';
import { createTicket, viewTicket, listTickets } from './commands/ticket';

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

// Ticket commands
const ticket = program
  .command('ticket')
  .description('Manage support tickets');

ticket
  .command('create')
  .description('Create a new support ticket interactively')
  .action(createTicket);

ticket
  .command('view <ticket>')
  .description('View details of a specific ticket')
  .action(viewTicket);

ticket
  .command('list')
  .description('List all your tickets')
  .action(listTickets);

program.parse(process.argv);
