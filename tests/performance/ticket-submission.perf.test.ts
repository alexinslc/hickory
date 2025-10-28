/**
 * Performance Test: Ticket Submission (T075)
 * 
 * Success Criterion SC-003: Users receive confirmation and ticket ID within 2 seconds of submission
 * 
 * This test validates that ticket creation completes within the performance SLA.
 */

import axios, { AxiosInstance } from 'axios';

const API_BASE_URL = process.env.API_URL || 'http://localhost:5000';
const PERFORMANCE_THRESHOLD_MS = 2000; // 2 seconds per SC-003
const TEST_ITERATIONS = 50; // Run multiple times for statistical significance

interface PerformanceResult {
  iteration: number;
  durationMs: number;
  success: boolean;
  ticketId?: string;
  error?: string;
}

interface TestCredentials {
  email: string;
  password: string;
  accessToken?: string;
}

/**
 * Test credentials - In production tests, these would be created/cleaned up as part of test setup
 */
const TEST_USER: TestCredentials = {
  email: process.env.TEST_USER_EMAIL || 'perftest@example.com',
  password: process.env.TEST_USER_PASSWORD || 'TestPassword123!',
};

let apiClient: AxiosInstance;

/**
 * Setup: Authenticate and get access token
 */
async function setupAuth(): Promise<void> {
  try {
    const response = await axios.post(`${API_BASE_URL}/api/v1/auth/login`, {
      email: TEST_USER.email,
      password: TEST_USER.password,
    });
    
    TEST_USER.accessToken = response.data.accessToken;
    
    apiClient = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Authorization': `Bearer ${TEST_USER.accessToken}`,
        'Content-Type': 'application/json',
      },
      timeout: 5000, // 5 second timeout (should never reach this for 2s SLA)
    });
    
    console.log('✓ Authentication successful');
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : 'Unknown error';
    console.error('✗ Authentication failed:', errorMessage);
    throw new Error('Cannot proceed without authentication');
  }
}

/**
 * Run a single ticket submission and measure performance
 */
async function measureTicketSubmission(iteration: number): Promise<PerformanceResult> {
  const startTime = performance.now();
  
  try {
    const response = await apiClient.post('/api/v1/tickets', {
      title: `Performance Test Ticket ${iteration} - ${Date.now()}`,
      description: `This is a performance test ticket created to validate SC-003 success criterion. Iteration: ${iteration}`,
      priority: 'Medium',
    });
    
    const endTime = performance.now();
    const durationMs = endTime - startTime;
    
    return {
      iteration,
      durationMs,
      success: true,
      ticketId: response.data.id,
    };
  } catch (error) {
    const endTime = performance.now();
    const durationMs = endTime - startTime;
    const errorMessage = error instanceof Error ? error.message : 'Unknown error';
    
    return {
      iteration,
      durationMs,
      success: false,
      error: errorMessage,
    };
  }
}

/**
 * Calculate performance statistics
 */
function calculateStats(results: PerformanceResult[]): {
  total: number;
  successful: number;
  failed: number;
  averageMs: number;
  minMs: number;
  maxMs: number;
  p50Ms: number;
  p95Ms: number;
  p99Ms: number;
  withinSLA: number;
  slaCompliancePercent: number;
} {
  const successfulResults = results.filter(r => r.success);
  const durations = successfulResults.map(r => r.durationMs).sort((a, b) => a - b);
  
  const total = results.length;
  const successful = successfulResults.length;
  const failed = total - successful;
  
  const averageMs = durations.reduce((sum, d) => sum + d, 0) / durations.length;
  const minMs = durations[0] || 0;
  const maxMs = durations[durations.length - 1] || 0;
  
  const p50Index = Math.floor(durations.length * 0.5);
  const p95Index = Math.floor(durations.length * 0.95);
  const p99Index = Math.floor(durations.length * 0.99);
  
  const p50Ms = durations[p50Index] || 0;
  const p95Ms = durations[p95Index] || 0;
  const p99Ms = durations[p99Index] || 0;
  
  const withinSLA = durations.filter(d => d <= PERFORMANCE_THRESHOLD_MS).length;
  const slaCompliancePercent = (withinSLA / successful) * 100;
  
  return {
    total,
    successful,
    failed,
    averageMs,
    minMs,
    maxMs,
    p50Ms,
    p95Ms,
    p99Ms,
    withinSLA,
    slaCompliancePercent,
  };
}

/**
 * Print test results in a formatted table
 */
function printResults(stats: ReturnType<typeof calculateStats>): void {
  console.log('\n' + '='.repeat(70));
  console.log('PERFORMANCE TEST RESULTS: Ticket Submission (T075 / SC-003)');
  console.log('='.repeat(70));
  console.log(`\nSLA Target: ${PERFORMANCE_THRESHOLD_MS}ms (2 seconds)`);
  console.log(`Test Iterations: ${stats.total}`);
  console.log(`\nSuccess Rate:`);
  console.log(`  Successful: ${stats.successful}/${stats.total} (${((stats.successful/stats.total)*100).toFixed(1)}%)`);
  console.log(`  Failed: ${stats.failed}/${stats.total}`);
  
  console.log(`\nPerformance Metrics:`);
  console.log(`  Average:  ${stats.averageMs.toFixed(2)}ms`);
  console.log(`  Minimum:  ${stats.minMs.toFixed(2)}ms`);
  console.log(`  Maximum:  ${stats.maxMs.toFixed(2)}ms`);
  console.log(`  P50:      ${stats.p50Ms.toFixed(2)}ms`);
  console.log(`  P95:      ${stats.p95Ms.toFixed(2)}ms`);
  console.log(`  P99:      ${stats.p99Ms.toFixed(2)}ms`);
  
  console.log(`\nSLA Compliance:`);
  console.log(`  Within ${PERFORMANCE_THRESHOLD_MS}ms: ${stats.withinSLA}/${stats.successful} (${stats.slaCompliancePercent.toFixed(1)}%)`);
  
  if (stats.slaCompliancePercent >= 95) {
    console.log(`\n✅ PASS: ${stats.slaCompliancePercent.toFixed(1)}% of requests met the ${PERFORMANCE_THRESHOLD_MS}ms SLA`);
  } else {
    console.log(`\n❌ FAIL: Only ${stats.slaCompliancePercent.toFixed(1)}% of requests met the ${PERFORMANCE_THRESHOLD_MS}ms SLA (target: ≥95%)`);
  }
  
  console.log('='.repeat(70) + '\n');
}

/**
 * Main test execution
 */
async function runPerformanceTest(): Promise<void> {
  console.log('Starting Ticket Submission Performance Test (T075)...\n');
  
  // Setup
  console.log('Step 1: Authenticating test user...');
  await setupAuth();
  
  console.log(`\nStep 2: Running ${TEST_ITERATIONS} ticket submission tests...`);
  process.stdout.write('Progress: ');
  
  const results: PerformanceResult[] = [];
  
  // Run tests with some delay between to avoid overwhelming the server
  for (let i = 1; i <= TEST_ITERATIONS; i++) {
    const result = await measureTicketSubmission(i);
    results.push(result);
    
    // Progress indicator
    if (i % 10 === 0) {
      process.stdout.write(`${i}...`);
    }
    
    // Small delay to simulate realistic load (50ms between requests)
    await new Promise(resolve => setTimeout(resolve, 50));
  }
  
  console.log(' Done!\n');
  
  // Calculate and print results
  console.log('Step 3: Analyzing results...');
  const stats = calculateStats(results);
  printResults(stats);
  
  // Exit with appropriate code
  if (stats.slaCompliancePercent >= 95 && stats.failed === 0) {
    console.log('✅ All tests passed!');
    process.exit(0);
  } else {
    console.log('❌ Some tests failed or did not meet SLA requirements');
    process.exit(1);
  }
}

/**
 * Error handling
 */
process.on('unhandledRejection', (error) => {
  console.error('Unhandled rejection:', error);
  process.exit(1);
});

// Run the test
runPerformanceTest().catch((error) => {
  console.error('Fatal error:', error);
  process.exit(1);
});
