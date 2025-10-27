#!/usr/bin/env node

import { Command } from 'commander';
import { login, logout, getConfig } from './commands/auth';
import { createTicket, viewTicket, listTickets } from './commands/ticket';
import { agentQueue, assignTicket, closeTicket } from './commands/agent';

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

// Agent commands (requires Agent or Administrator role)
const agent = program
  .command('agent')
  .description('Agent commands for managing support tickets');

agent
  .command('queue')
  .description('View the agent ticket queue')
  .option('-f, --filter <type>', 'Filter tickets: all, unassigned, mine', 'all')
  .action((options) => agentQueue(options));

agent
  .command('assign <ticket>')
  .description('Assign a ticket to yourself')
  .action(assignTicket);

agent
  .command('close <ticket>')
  .description('Close a ticket with resolution notes')
  .action(closeTicket);

program.parse(process.argv);
