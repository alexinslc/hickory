import axios from 'axios';
import * as readline from 'readline';
import { getConfig } from './auth';

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
    console.error('Error: Not authenticated. Run "hickory login" first.');
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

function getStatusColor(status: string): string {
  const colors: Record<string, string> = {
    open: '\x1b[34m', // Blue
    inprogress: '\x1b[33m', // Yellow
    resolved: '\x1b[32m', // Green
    closed: '\x1b[90m', // Gray
    cancelled: '\x1b[31m', // Red
  };
  return colors[status.toLowerCase()] || '\x1b[0m';
}

function getPriorityColor(priority: string): string {
  const colors: Record<string, string> = {
    critical: '\x1b[31m', // Red
    high: '\x1b[33m', // Yellow
    medium: '\x1b[36m', // Cyan
    low: '\x1b[32m', // Green
  };
  return colors[priority.toLowerCase()] || '\x1b[0m';
}

const RESET = '\x1b[0m';
const BOLD = '\x1b[1m';

/**
 * Create a new ticket interactively
 */
export async function createTicket() {
  const authHeader = requireAuth();

  try {
    console.log(`${BOLD}Create a New Ticket${RESET}\n`);

    // Get title
    let title = '';
    while (title.length < 5 || title.length > 200) {
      title = await promptInput('Title (5-200 characters): ');
      if (title.length < 5) {
        console.log('Title must be at least 5 characters long.');
      } else if (title.length > 200) {
        console.log('Title must be no more than 200 characters long.');
      }
    }

    // Get description
    let description = '';
    while (description.length < 10 || description.length > 10000) {
      description = await promptMultiline('Description (10-10,000 characters):');
      if (description.length < 10) {
        console.log('\nDescription must be at least 10 characters long. Please try again.\n');
      } else if (description.length > 10000) {
        console.log('\nDescription must be no more than 10,000 characters long. Please try again.\n');
      }
    }

    // Get priority
    console.log('\nPriority:');
    console.log('  1. Low');
    console.log('  2. Medium');
    console.log('  3. High');
    console.log('  4. Critical');
    
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
          console.log('Invalid choice. Please select 1-4.');
      }
    }

    // Create the ticket
    console.log('\nCreating ticket...');
    
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
    
    console.log(`\n${BOLD}✓ Ticket created successfully!${RESET}\n`);
    console.log(`Ticket Number: ${BOLD}${ticket.ticketNumber}${RESET}`);
    console.log(`Title: ${ticket.title}`);
    console.log(`Priority: ${getPriorityColor(ticket.priority)}${ticket.priority}${RESET}`);
    console.log(`Status: ${getStatusColor(ticket.status)}${ticket.status}${RESET}`);
    console.log(`\nView details: ${BOLD}hickory ticket view ${ticket.ticketNumber}${RESET}`);
  } catch (error: any) {
    if (error.response?.status === 401) {
      console.error('Error: Authentication failed. Please login again.');
      process.exit(1);
    } else if (error.response?.data?.errors) {
      console.error('Validation errors:');
      Object.entries(error.response.data.errors).forEach(([field, messages]) => {
        console.error(`  ${field}: ${(messages as string[]).join(', ')}`);
      });
    } else {
      console.error(`Error creating ticket: ${error.response?.data?.message || error.message}`);
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
    console.error('Error: Ticket number or ID is required.');
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
    
    console.log('\n' + '='.repeat(80));
    console.log(`${BOLD}${ticket.ticketNumber}: ${ticket.title}${RESET}`);
    console.log('='.repeat(80));
    console.log('');
    
    console.log(`${BOLD}Status:${RESET} ${getStatusColor(ticket.status)}${ticket.status}${RESET}`);
    console.log(`${BOLD}Priority:${RESET} ${getPriorityColor(ticket.priority)}${ticket.priority}${RESET}`);
    console.log(`${BOLD}Submitted by:${RESET} ${ticket.submitterName}`);
    
    if (ticket.assignedToName) {
      console.log(`${BOLD}Assigned to:${RESET} ${ticket.assignedToName}`);
    } else {
      console.log(`${BOLD}Assigned to:${RESET} Unassigned`);
    }
    
    console.log(`${BOLD}Created:${RESET} ${formatDate(ticket.createdAt)}`);
    console.log(`${BOLD}Last updated:${RESET} ${formatDate(ticket.updatedAt)}`);
    
    if (ticket.closedAt) {
      console.log(`${BOLD}Closed:${RESET} ${formatDate(ticket.closedAt)}`);
    }
    
    console.log(`${BOLD}Comments:${RESET} ${ticket.commentCount}`);
    
    console.log('\n' + '-'.repeat(80));
    console.log(`${BOLD}Description:${RESET}`);
    console.log('-'.repeat(80));
    console.log(ticket.description);
    
    if (ticket.resolutionNotes) {
      console.log('\n' + '-'.repeat(80));
      console.log(`${BOLD}Resolution Notes:${RESET}`);
      console.log('-'.repeat(80));
      console.log(ticket.resolutionNotes);
    }
    
    console.log('\n');
  } catch (error: any) {
    if (error.response?.status === 401) {
      console.error('Error: Authentication failed. Please login again.');
    } else if (error.response?.status === 404) {
      console.error(`Error: Ticket "${ticketIdentifier}" not found.`);
    } else if (error.response?.status === 403) {
      console.error('Error: You do not have permission to view this ticket.');
    } else {
      console.error(`Error fetching ticket: ${error.response?.data?.message || error.message}`);
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
      console.log(`Create your first ticket: ${BOLD}hickory ticket create${RESET}\n`);
      return;
    }

    console.log(`\n${BOLD}Your Tickets (${tickets.length})${RESET}\n`);
    console.log('-'.repeat(120));
    console.log(
      `${BOLD}${'NUMBER'.padEnd(12)} ${'TITLE'.padEnd(35)} ${'STATUS'.padEnd(12)} ${'PRIORITY'.padEnd(10)} ${'CREATED'.padEnd(20)} ${'COMMENTS'.padEnd(8)}${RESET}`
    );
    console.log('-'.repeat(120));

    tickets.forEach((ticket) => {
      const number = ticket.ticketNumber.padEnd(12);
      const title = (ticket.title.length > 35 ? ticket.title.substring(0, 32) + '...' : ticket.title).padEnd(35);
      const status = ticket.status.padEnd(12);
      const priority = ticket.priority.padEnd(10);
      const created = new Date(ticket.createdAt).toISOString().replace('T', ' ').substring(0, 16).padEnd(20);
      const comments = ticket.commentCount.toString().padEnd(8);

      console.log(
        `${number} ${title} ${getStatusColor(ticket.status)}${status}${RESET} ${getPriorityColor(ticket.priority)}${priority}${RESET} ${created} ${comments}`
      );
    });

    console.log('-'.repeat(120));
    console.log(`\nView details: ${BOLD}hickory ticket view <ticket-number>${RESET}`);
    console.log(`Create ticket: ${BOLD}hickory ticket create${RESET}\n`);
  } catch (error: any) {
    if (error.response?.status === 401) {
      console.error('Error: Authentication failed. Please login again.');
    } else {
      console.error(`Error fetching tickets: ${error.response?.data?.message || error.message}`);
    }
    process.exit(1);
  }
}
