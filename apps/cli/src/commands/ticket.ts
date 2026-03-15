import axios from 'axios';
import * as readline from 'readline';
import { getConfig } from './auth';
import {
  bold,
  dim,
  cyan,
  success,
  error as errorColor,
  warning,
  info,
  colorizeStatus,
  colorizePriority,
} from '../utils/colors';

const API_BASE_URL = process.env.HICKORY_API_URL || 'http://localhost:5000';

interface TicketDto {
  id: string;
  ticketNumber: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  submitterId: string;
  submitterName: string;
  assignedToId?: string;
  assignedToName?: string;
  createdAt: string;
  updatedAt: string;
  closedAt?: string;
  resolutionNotes?: string;
  commentCount: number;
}

interface CreateTicketRequest {
  title: string;
  description: string;
  priority: string;
}

function getAuthHeader(): string | null {
  const config = getConfig();
  if (!config || !config.accessToken) {
    return null;
  }
  return `Bearer ${config.accessToken}`;
}

function requireAuth(): string {
  const authHeader = getAuthHeader();
  if (!authHeader) {
    console.error(errorColor('Error: Not authenticated. Run "hickory login" first.'));
    process.exit(1);
  }
  return authHeader;
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

function promptMultiline(question: string): Promise<string> {
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
  });

  console.log(question);
  console.log('(Type your description. Press Enter twice to finish)');
  console.log('');

  return new Promise((resolve) => {
    const lines: string[] = [];
    let emptyLineCount = 0;

    rl.on('line', (line) => {
      if (line.trim() === '') {
        emptyLineCount++;
        if (emptyLineCount >= 2) {
          rl.close();
          // Remove the last empty line
          lines.pop();
          resolve(lines.join('\n'));
          return;
        }
      } else {
        emptyLineCount = 0;
      }
      lines.push(line);
    });
  });
}

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
  }).format(date);
}


/**
 * Create a new ticket interactively
 */
export async function createTicket() {
  const authHeader = requireAuth();

  try {
    console.log(bold('Create a New Ticket') + '\n');

    // Get title
    let title = '';
    while (title.length < 5 || title.length > 200) {
      title = await promptInput('Title (5-200 characters): ');
      if (title.length < 5) {
        console.log(warning('Title must be at least 5 characters long.'));
      } else if (title.length > 200) {
        console.log(warning('Title must be no more than 200 characters long.'));
      }
    }

    // Get description
    let description = '';
    while (description.length < 10 || description.length > 10000) {
      description = await promptMultiline('Description (10-10,000 characters):');
      if (description.length < 10) {
        console.log('\n' + warning('Description must be at least 10 characters long. Please try again.') + '\n');
      } else if (description.length > 10000) {
        console.log('\n' + warning('Description must be no more than 10,000 characters long. Please try again.') + '\n');
      }
    }

    // Get priority
    console.log('\n' + bold('Priority:'));
    console.log(`  1. ${colorizePriority('Low')}`);
    console.log(`  2. ${colorizePriority('Medium')}`);
    console.log(`  3. ${colorizePriority('High')}`);
    console.log(`  4. ${colorizePriority('Critical')}`);

    let priorityChoice = '';
    let priority = '';
    while (!priority) {
      priorityChoice = await promptInput('Select priority (1-4): ');
      switch (priorityChoice) {
        case '1':
          priority = 'Low';
          break;
        case '2':
          priority = 'Medium';
          break;
        case '3':
          priority = 'High';
          break;
        case '4':
          priority = 'Critical';
          break;
        default:
          console.log(warning('Invalid choice. Please select 1-4.'));
      }
    }

    // Create the ticket
    console.log('\n' + info('Creating ticket...'));

    const response = await axios.post<{ data: TicketDto }>(
      `${API_BASE_URL}/api/tickets`,
      {
        title,
        description,
        priority,
      } as CreateTicketRequest,
      {
        headers: {
          Authorization: authHeader,
          'Content-Type': 'application/json',
        },
      }
    );

    const ticket = response.data.data;

    console.log('\n' + success(bold('✓ Ticket created successfully!')) + '\n');
    console.log(`Ticket Number: ${bold(ticket.ticketNumber)}`);
    console.log(`Title: ${ticket.title}`);
    console.log(`Priority: ${colorizePriority(ticket.priority)}`);
    console.log(`Status: ${colorizeStatus(ticket.status)}`);
    console.log(`\nView details: ${bold('hickory ticket view ' + ticket.ticketNumber)}`);
  } catch (err: any) {
    if (err.response?.status === 401) {
      console.error(errorColor('Error: Authentication failed. Please login again.'));
      process.exit(1);
    } else if (err.response?.data?.errors) {
      console.error(errorColor('Validation errors:'));
      Object.entries(err.response.data.errors).forEach(([field, messages]) => {
        console.error(`  ${field}: ${(messages as string[]).join(', ')}`);
      });
    } else {
      console.error(errorColor(`Error creating ticket: ${err.response?.data?.message || err.message}`));
    }
    process.exit(1);
  }
}

/**
 * View a single ticket by ticket number or ID
 */
export async function viewTicket(ticketIdentifier: string) {
  const authHeader = requireAuth();

  if (!ticketIdentifier) {
    console.error(errorColor('Error: Ticket number or ID is required.'));
    console.log('Usage: hickory ticket view <ticket-number-or-id>');
    process.exit(1);
  }

  try {
    const response = await axios.get<{ data: TicketDto }>(
      `${API_BASE_URL}/api/tickets/${ticketIdentifier}`,
      {
        headers: {
          Authorization: authHeader,
        },
      }
    );

    const ticket = response.data.data;

    console.log('\n' + dim('='.repeat(80)));
    console.log(bold(`${ticket.ticketNumber}: ${ticket.title}`));
    console.log(dim('='.repeat(80)));
    console.log('');

    console.log(`${bold('Status:')} ${colorizeStatus(ticket.status)}`);
    console.log(`${bold('Priority:')} ${colorizePriority(ticket.priority)}`);
    console.log(`${bold('Submitted by:')} ${ticket.submitterName}`);

    if (ticket.assignedToName) {
      console.log(`${bold('Assigned to:')} ${ticket.assignedToName}`);
    } else {
      console.log(`${bold('Assigned to:')} ${dim('Unassigned')}`);
    }

    console.log(`${bold('Created:')} ${formatDate(ticket.createdAt)}`);
    console.log(`${bold('Last updated:')} ${formatDate(ticket.updatedAt)}`);

    if (ticket.closedAt) {
      console.log(`${bold('Closed:')} ${formatDate(ticket.closedAt)}`);
    }

    console.log(`${bold('Comments:')} ${ticket.commentCount}`);

    console.log('\n' + dim('-'.repeat(80)));
    console.log(bold('Description:'));
    console.log(dim('-'.repeat(80)));
    console.log(ticket.description);

    if (ticket.resolutionNotes) {
      console.log('\n' + dim('-'.repeat(80)));
      console.log(bold('Resolution Notes:'));
      console.log(dim('-'.repeat(80)));
      console.log(ticket.resolutionNotes);
    }

    console.log('\n');
  } catch (err: any) {
    if (err.response?.status === 401) {
      console.error(errorColor('Error: Authentication failed. Please login again.'));
    } else if (err.response?.status === 404) {
      console.error(errorColor(`Error: Ticket "${ticketIdentifier}" not found.`));
    } else if (err.response?.status === 403) {
      console.error(errorColor('Error: You do not have permission to view this ticket.'));
    } else {
      console.error(errorColor(`Error fetching ticket: ${err.response?.data?.message || err.message}`));
    }
    process.exit(1);
  }
}

/**
 * List all tickets for the current user
 */
export async function listTickets() {
  const authHeader = requireAuth();

  try {
    const response = await axios.get<{ data: TicketDto[] }>(
      `${API_BASE_URL}/api/tickets/my`,
      {
        headers: {
          Authorization: authHeader,
        },
      }
    );

    const tickets = response.data.data;
    
    if (tickets.length === 0) {
      console.log('\nNo tickets found.');
      console.log(`Create your first ticket: ${bold('hickory ticket create')}\n`);
      return;
    }

    console.log(`\n${bold('Your Tickets (' + tickets.length + ')')}\n`);
    console.log(dim('-'.repeat(120)));
    console.log(
      bold(`${'NUMBER'.padEnd(12)} ${'TITLE'.padEnd(35)} ${'STATUS'.padEnd(12)} ${'PRIORITY'.padEnd(10)} ${'CREATED'.padEnd(20)} ${'COMMENTS'.padEnd(8)}`)
    );
    console.log(dim('-'.repeat(120)));

    tickets.forEach((ticket) => {
      const number = ticket.ticketNumber.padEnd(12);
      // Title: max 35 chars (truncate to 32 + '...' = 35 if longer)
      const maxTitleLength = 35;
      const truncateAt = 32; // Leave room for '...'
      const title = (ticket.title.length > maxTitleLength
        ? ticket.title.substring(0, truncateAt) + '...'
        : ticket.title
      ).padEnd(maxTitleLength);
      const statusText = ticket.status.padEnd(12);
      const priorityText = ticket.priority.padEnd(10);
      const created = new Date(ticket.createdAt).toISOString().replace('T', ' ').substring(0, 16).padEnd(20);
      const comments = ticket.commentCount.toString().padEnd(8);

      console.log(
        `${number} ${title} ${colorizeStatus(statusText)} ${colorizePriority(priorityText)} ${created} ${comments}`
      );
    });

    console.log(dim('-'.repeat(120)));
    console.log(`\nView details: ${bold('hickory ticket view <ticket-number>')}`);
    console.log(`Create ticket: ${bold('hickory ticket create')}\n`);
  } catch (err: any) {
    if (err.response?.status === 401) {
      console.error(errorColor('Error: Authentication failed. Please login again.'));
    } else {
      console.error(errorColor(`Error fetching tickets: ${err.response?.data?.message || err.message}`));
    }
    process.exit(1);
  }
}
