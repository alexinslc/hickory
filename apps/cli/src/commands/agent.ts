/* eslint-disable @typescript-eslint/no-explicit-any */
import axios from 'axios';
import * as readline from 'readline';
import { getConfig } from './auth';
import {
  bold,
  dim,
  red,
  green,
  cyan,
  success,
  error as errorColor,
  warning,
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

function requireAgentRole(): void {
  const config = getConfig();
  if (!config || !config.user) {
    console.error(errorColor('Error: Not authenticated. Run "hickory login" first.'));
    process.exit(1);
  }
  if (config.user.role !== 'Agent' && config.user.role !== 'Administrator') {
    console.error(errorColor('Error: This command requires Agent or Administrator role.'));
    process.exit(1);
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


function formatDate(dateString: string): string {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  const diffDays = Math.floor(diffHours / 24);

  if (diffHours < 1) return '< 1h ago';
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;
  
  return date.toLocaleDateString();
}

// T103: Agent queue command
export async function agentQueue(options: { filter?: string }): Promise<void> {
  requireAgentRole();
  const authHeader = requireAuth();

  try {
    const response = await axios.get<TicketDto[]>(`${API_BASE_URL}/api/tickets/queue`, {
      headers: { Authorization: authHeader },
    });

    const tickets = response.data;
    const config = getConfig();
    const userId = config?.user?.userId;

    // Apply filter
    let filteredTickets = tickets;
    if (options.filter === 'unassigned') {
      filteredTickets = tickets.filter((t) => !t.assignedToId);
    } else if (options.filter === 'mine') {
      filteredTickets = tickets.filter((t) => t.assignedToId === userId);
    }

    if (filteredTickets.length === 0) {
      console.log('\n' + warning('No tickets found in the queue') + '\n');
      return;
    }

    // Display statistics
    const unassignedCount = tickets.filter((t) => !t.assignedToId).length;
    const myTicketsCount = tickets.filter((t) => t.assignedToId === userId).length;

    console.log('\n' + bold(cyan('=== Agent Queue ===')));
    console.log(`Total: ${tickets.length} | Unassigned: ${red(String(unassignedCount))} | Mine: ${green(String(myTicketsCount))}\n`);

    // Table header
    console.log(
      dim(`${'Ticket'.padEnd(12)} ${'Title'.padEnd(40)} ${'Status'.padEnd(12)} ${'Priority'.padEnd(10)} ${'Assigned'.padEnd(20)} ${'Age'.padEnd(10)}`)
    );
    console.log(dim('-'.repeat(120)));

    // Display tickets
    filteredTickets.forEach((ticket) => {
      const number = ticket.ticketNumber.padEnd(12);
      const title = ticket.title.length > 40 ? ticket.title.substring(0, 37) + '...' : ticket.title.padEnd(40);
      const statusText = ticket.status.padEnd(12);
      const priorityText = ticket.priority.padEnd(10);
      const assigned = (ticket.assignedToName || 'Unassigned').padEnd(20);
      const age = formatDate(ticket.createdAt).padEnd(10);

      console.log(
        `${number} ${title} ${colorizeStatus(statusText)} ${colorizePriority(priorityText)} ${!ticket.assignedToName ? red(assigned) : assigned} ${age}`
      );
    });

    console.log(dim('-'.repeat(120)));
    console.log(`\n${bold('Commands:')}`);
    console.log(`  View ticket:    ${cyan('hickory agent view <ticket-number>')}`);
    console.log(`  Assign to me:   ${cyan('hickory agent assign <ticket-number>')}`);
    console.log(`  Close ticket:   ${cyan('hickory agent close <ticket-number>')}`);
    console.log(`\n${bold('Filters:')}`);
    console.log(`  Unassigned:     ${cyan('hickory agent queue --filter unassigned')}`);
    console.log(`  My tickets:     ${cyan('hickory agent queue --filter mine')}\n`);
  } catch (error: unknown) {
    const err = error as any;
    if (err.response?.status === 401) {
      console.error(errorColor('Error: Authentication failed. Please login again.'));
    } else if (err.response?.status === 403) {
      console.error(errorColor('Error: Access denied. This command requires Agent or Administrator role.'));
    } else {
      console.error(errorColor(`Error fetching agent queue: ${err.response?.data?.message || err.message}`));
    }
    process.exit(1);
  }
}

// T104: Assign ticket command
export async function assignTicket(ticketNumber: string): Promise<void> {
  requireAgentRole();
  const authHeader = requireAuth();
  const config = getConfig();
  const userId = config?.user?.userId;

  if (!ticketNumber) {
    console.error(errorColor('Error: Ticket number is required'));
    console.log(`Usage: ${cyan('hickory agent assign <ticket-number>')}`);
    process.exit(1);
  }

  try {
    // First, get the ticket to find its ID
    const listResponse = await axios.get<TicketDto[]>(`${API_BASE_URL}/api/tickets/queue`, {
      headers: { Authorization: authHeader },
    });

    const ticket = listResponse.data.find(
      (t) => t.ticketNumber.toLowerCase() === ticketNumber.toLowerCase()
    );

    if (!ticket) {
      console.error(errorColor(`Error: Ticket ${ticketNumber} not found in the queue`));
      process.exit(1);
    }

    if (ticket.assignedToId) {
      console.log('\n' + warning(`Warning: Ticket is already assigned to ${ticket.assignedToName}`));
      const confirm = await promptInput('Do you want to reassign it to yourself? (y/n): ');
      if (confirm.toLowerCase() !== 'y' && confirm.toLowerCase() !== 'yes') {
        console.log('Assignment cancelled.');
        return;
      }
    }

    // Assign the ticket
    await axios.put(
      `${API_BASE_URL}/api/tickets/${ticket.id}/assign`,
      { agentId: userId },
      { headers: { Authorization: authHeader } }
    );

    console.log(`\n${success('✓')} Ticket ${bold(ticket.ticketNumber)} assigned to you successfully`);
    console.log(`  Status: ${colorizeStatus(ticket.status)}`);
    console.log(`  Priority: ${colorizePriority(ticket.priority)}`);
    console.log(`  Title: ${ticket.title}`);
    console.log(`\nView details: ${cyan('hickory agent view ' + ticket.ticketNumber)}\n`);
  } catch (error: unknown) {
    const err = error as any;
    if (err.response?.status === 401) {
      console.error(errorColor('Error: Authentication failed. Please login again.'));
    } else if (err.response?.status === 403) {
      console.error(errorColor('Error: Access denied. This command requires Agent or Administrator role.'));
    } else if (err.response?.status === 404) {
      console.error(errorColor(`Error: Ticket ${ticketNumber} not found`));
    } else {
      console.error(errorColor(`Error assigning ticket: ${err.response?.data?.title || err.message}`));
    }
    process.exit(1);
  }
}

// T105: Close ticket command
export async function closeTicket(ticketNumber: string): Promise<void> {
  requireAgentRole();
  const authHeader = requireAuth();

  if (!ticketNumber) {
    console.error(errorColor('Error: Ticket number is required'));
    console.log(`Usage: ${cyan('hickory agent close <ticket-number>')}`);
    process.exit(1);
  }

  try {
    // First, get the ticket to find its ID and validate
    const listResponse = await axios.get<TicketDto[]>(`${API_BASE_URL}/api/tickets/queue`, {
      headers: { Authorization: authHeader },
    });

    const ticket = listResponse.data.find(
      (t) => t.ticketNumber.toLowerCase() === ticketNumber.toLowerCase()
    );

    if (!ticket) {
      console.error(errorColor(`Error: Ticket ${ticketNumber} not found in the queue`));
      process.exit(1);
    }

    if (ticket.status === 'Closed') {
      console.error(errorColor(`Error: Ticket ${ticketNumber} is already closed`));
      process.exit(1);
    }

    // Display ticket info
    console.log('\n' + bold(`Closing Ticket: ${ticket.ticketNumber}`));
    console.log(`Title: ${ticket.title}`);
    console.log(`Status: ${colorizeStatus(ticket.status)}`);
    console.log(`Priority: ${colorizePriority(ticket.priority)}\n`);

    // Prompt for resolution notes
    console.log(bold('Resolution Notes') + ' (minimum 10 characters):');
    console.log(dim('Describe how the issue was resolved. Press Enter twice when done.') + '\n');

    let resolutionNotes = '';
    let line = '';
    const rl = readline.createInterface({
      input: process.stdin,
      output: process.stdout,
    });

    // Read multi-line input
    for await (const input of rl) {
      if (input === '' && line === '') {
        // Two consecutive empty lines - done
        break;
      }
      if (input === '') {
        line = '';
      } else {
        resolutionNotes += (resolutionNotes ? '\n' : '') + input;
        line = input;
      }
    }
    rl.close();

    resolutionNotes = resolutionNotes.trim();

    if (resolutionNotes.length < 10) {
      console.error('\n' + errorColor('Error: Resolution notes must be at least 10 characters long'));
      process.exit(1);
    }

    // Close the ticket
    await axios.post(
      `${API_BASE_URL}/api/tickets/${ticket.id}/close`,
      { resolutionNotes },
      { headers: { Authorization: authHeader } }
    );

    console.log(`\n${success('✓')} Ticket ${bold(ticket.ticketNumber)} closed successfully`);
    console.log('\nResolution notes saved:');
    console.log(dim(resolutionNotes) + '\n');
  } catch (error: unknown) {
    const err = error as any;
    if (err.response?.status === 401) {
      console.error('\n' + errorColor('Error: Authentication failed. Please login again.'));
    } else if (err.response?.status === 403) {
      console.error('\n' + errorColor('Error: Access denied. This command requires Agent or Administrator role.'));
    } else if (err.response?.status === 404) {
      console.error('\n' + errorColor(`Error: Ticket ${ticketNumber} not found`));
    } else if (err.response?.status === 400) {
      const errors = err.response?.data?.errors;
      if (errors && errors.ResolutionNotes) {
        console.error('\n' + errorColor(`Error: ${errors.ResolutionNotes.join(', ')}`));
      } else {
        console.error('\n' + errorColor(`Error: ${err.response?.data?.title || err.message}`));
      }
    } else {
      console.error('\n' + errorColor(`Error closing ticket: ${err.response?.data?.title || err.message}`));
    }
    process.exit(1);
  }
}
