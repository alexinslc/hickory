/* eslint-disable @typescript-eslint/no-explicit-any */
import axios from 'axios';
import * as readline from 'readline';
import { getConfig } from './auth';

const API_BASE_URL = process.env.HICKORY_API_URL || 'http://localhost:5000';

// ANSI color codes
const RESET = '\x1b[0m';
const BOLD = '\x1b[1m';
const DIM = '\x1b[2m';
const RED = '\x1b[31m';
const GREEN = '\x1b[32m';
const YELLOW = '\x1b[33m';
const BLUE = '\x1b[34m';
const MAGENTA = '\x1b[35m';
const CYAN = '\x1b[36m';
const GRAY = '\x1b[90m';

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
    console.error('Error: Not authenticated. Run "hickory login" first.');
    process.exit(1);
  }
  return authHeader;
}

function requireAgentRole(): void {
  const config = getConfig();
  if (!config || !config.user) {
    console.error('Error: Not authenticated. Run "hickory login" first.');
    process.exit(1);
  }
  if (config.user.role !== 'Agent' && config.user.role !== 'Administrator') {
    console.error('Error: This command requires Agent or Administrator role.');
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

function getStatusColor(status: string): string {
  switch (status.toLowerCase()) {
    case 'open':
      return BLUE;
    case 'inprogress':
      return YELLOW;
    case 'resolved':
      return GREEN;
    case 'closed':
      return GRAY;
    case 'cancelled':
      return RED;
    default:
      return RESET;
  }
}

function getPriorityColor(priority: string): string {
  switch (priority.toLowerCase()) {
    case 'critical':
      return RED;
    case 'high':
      return MAGENTA;
    case 'medium':
      return YELLOW;
    case 'low':
      return CYAN;
    default:
      return RESET;
  }
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
      console.log(`\n${YELLOW}No tickets found in the queue${RESET}\n`);
      return;
    }

    // Display statistics
    const unassignedCount = tickets.filter((t) => !t.assignedToId).length;
    const myTicketsCount = tickets.filter((t) => t.assignedToId === userId).length;

    console.log(`\n${BOLD}${CYAN}=== Agent Queue ===${RESET}`);
    console.log(`Total: ${tickets.length} | Unassigned: ${RED}${unassignedCount}${RESET} | Mine: ${GREEN}${myTicketsCount}${RESET}\n`);

    // Table header
    console.log(
      `${DIM}${'Ticket'.padEnd(12)} ${'Title'.padEnd(40)} ${'Status'.padEnd(12)} ${'Priority'.padEnd(10)} ${'Assigned'.padEnd(20)} ${'Age'.padEnd(10)}${RESET}`
    );
    console.log('-'.repeat(120));

    // Display tickets
    filteredTickets.forEach((ticket) => {
      const number = ticket.ticketNumber.padEnd(12);
      const title = ticket.title.length > 40 ? ticket.title.substring(0, 37) + '...' : ticket.title.padEnd(40);
      const status = ticket.status.padEnd(12);
      const priority = ticket.priority.padEnd(10);
      const assigned = (ticket.assignedToName || 'Unassigned').padEnd(20);
      const age = formatDate(ticket.createdAt).padEnd(10);

      console.log(
        `${number} ${title} ${getStatusColor(ticket.status)}${status}${RESET} ${getPriorityColor(ticket.priority)}${priority}${RESET} ${!ticket.assignedToName ? RED : RESET}${assigned}${RESET} ${age}`
      );
    });

    console.log('-'.repeat(120));
    console.log(`\n${BOLD}Commands:${RESET}`);
    console.log(`  View ticket:    ${CYAN}hickory agent view <ticket-number>${RESET}`);
    console.log(`  Assign to me:   ${CYAN}hickory agent assign <ticket-number>${RESET}`);
    console.log(`  Close ticket:   ${CYAN}hickory agent close <ticket-number>${RESET}`);
    console.log(`\n${BOLD}Filters:${RESET}`);
    console.log(`  Unassigned:     ${CYAN}hickory agent queue --filter unassigned${RESET}`);
    console.log(`  My tickets:     ${CYAN}hickory agent queue --filter mine${RESET}\n`);
  } catch (error: unknown) {
    const err = error as any;
    if (err.response?.status === 401) {
      console.error('Error: Authentication failed. Please login again.');
    } else if (err.response?.status === 403) {
      console.error('Error: Access denied. This command requires Agent or Administrator role.');
    } else {
      console.error(`Error fetching agent queue: ${err.response?.data?.message || err.message}`);
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
    console.error('Error: Ticket number is required');
    console.log(`Usage: ${CYAN}hickory agent assign <ticket-number>${RESET}`);
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
      console.error(`Error: Ticket ${ticketNumber} not found in the queue`);
      process.exit(1);
    }

    if (ticket.assignedToId) {
      console.log(`\n${YELLOW}Warning: Ticket is already assigned to ${ticket.assignedToName}${RESET}`);
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

    console.log(`\n${GREEN}✓${RESET} Ticket ${BOLD}${ticket.ticketNumber}${RESET} assigned to you successfully`);
    console.log(`  Status: ${getStatusColor(ticket.status)}${ticket.status}${RESET}`);
    console.log(`  Priority: ${getPriorityColor(ticket.priority)}${ticket.priority}${RESET}`);
    console.log(`  Title: ${ticket.title}`);
    console.log(`\nView details: ${CYAN}hickory agent view ${ticket.ticketNumber}${RESET}\n`);
  } catch (error: unknown) {
    const err = error as any;
    if (err.response?.status === 401) {
      console.error('Error: Authentication failed. Please login again.');
    } else if (err.response?.status === 403) {
      console.error('Error: Access denied. This command requires Agent or Administrator role.');
    } else if (err.response?.status === 404) {
      console.error(`Error: Ticket ${ticketNumber} not found`);
    } else {
      console.error(`Error assigning ticket: ${err.response?.data?.title || err.message}`);
    }
    process.exit(1);
  }
}

// T105: Close ticket command
export async function closeTicket(ticketNumber: string): Promise<void> {
  requireAgentRole();
  const authHeader = requireAuth();

  if (!ticketNumber) {
    console.error('Error: Ticket number is required');
    console.log(`Usage: ${CYAN}hickory agent close <ticket-number>${RESET}`);
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
      console.error(`Error: Ticket ${ticketNumber} not found in the queue`);
      process.exit(1);
    }

    if (ticket.status === 'Closed') {
      console.error(`Error: Ticket ${ticketNumber} is already closed`);
      process.exit(1);
    }

    // Display ticket info
    console.log(`\n${BOLD}Closing Ticket: ${ticket.ticketNumber}${RESET}`);
    console.log(`Title: ${ticket.title}`);
    console.log(`Status: ${getStatusColor(ticket.status)}${ticket.status}${RESET}`);
    console.log(`Priority: ${getPriorityColor(ticket.priority)}${ticket.priority}${RESET}\n`);

    // Prompt for resolution notes
    console.log(`${BOLD}Resolution Notes${RESET} (minimum 10 characters):`);
    console.log(`${DIM}Describe how the issue was resolved. Press Enter twice when done.${RESET}\n`);

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
      console.error('\nError: Resolution notes must be at least 10 characters long');
      process.exit(1);
    }

    // Close the ticket
    await axios.post(
      `${API_BASE_URL}/api/tickets/${ticket.id}/close`,
      { resolutionNotes },
      { headers: { Authorization: authHeader } }
    );

    console.log(`\n${GREEN}✓${RESET} Ticket ${BOLD}${ticket.ticketNumber}${RESET} closed successfully`);
    console.log(`\nResolution notes saved:`);
    console.log(`${DIM}${resolutionNotes}${RESET}\n`);
  } catch (error: unknown) {
    const err = error as any;
    if (err.response?.status === 401) {
      console.error('\nError: Authentication failed. Please login again.');
    } else if (err.response?.status === 403) {
      console.error('\nError: Access denied. This command requires Agent or Administrator role.');
    } else if (err.response?.status === 404) {
      console.error(`\nError: Ticket ${ticketNumber} not found`);
    } else if (err.response?.status === 400) {
      const errors = err.response?.data?.errors;
      if (errors && errors.ResolutionNotes) {
        console.error(`\nError: ${errors.ResolutionNotes.join(', ')}`);
      } else {
        console.error(`\nError: ${err.response?.data?.title || err.message}`);
      }
    } else {
      console.error(`\nError closing ticket: ${err.response?.data?.title || err.message}`);
    }
    process.exit(1);
  }
}
