/**
 * Performance Test: Agent Queue Operations
 *
 * Tests agent queue performance under load.
 */

import axios, { AxiosInstance } from 'axios';

const API_BASE_URL = process.env.API_URL || 'http://localhost:5000';
const QUEUE_THRESHOLD_MS = 1500;
const TEST_ITERATIONS = 20;

let apiClient: AxiosInstance;

async function authenticate(): Promise<void> {
  const response = await axios.post(`${API_BASE_URL}/api/v1/auth/login`, {
    email: process.env.TEST_AGENT_EMAIL || 'agent@example.com',
    password: process.env.TEST_AGENT_PASSWORD || 'TestPassword123!',
  });
  apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: { Authorization: `Bearer ${response.data.accessToken}` },
  });
}

function stats(durations: number[]) {
  const sorted = [...durations].sort((a, b) => a - b);
  const avg = durations.reduce((s, d) => s + d, 0) / durations.length;
  return {
    avg: Math.round(avg),
    p95: sorted[Math.floor(sorted.length * 0.95)],
    p99: sorted[Math.floor(sorted.length * 0.99)],
  };
}

describe('Agent Queue Performance', () => {
  beforeAll(async () => {
    await authenticate();
  }, 10000);

  it(`should load agent queue within ${QUEUE_THRESHOLD_MS}ms`, async () => {
    const durations: number[] = [];

    for (let i = 0; i < TEST_ITERATIONS; i++) {
      const start = Date.now();
      await apiClient.get('/api/v1/tickets/queue');
      durations.push(Date.now() - start);
    }

    const s = stats(durations);
    console.log(`Agent queue: avg=${s.avg}ms p95=${s.p95}ms p99=${s.p99}ms`);
    expect(s.p95).toBeLessThan(QUEUE_THRESHOLD_MS);
  }, 60000);

  it('should handle queue with filters efficiently', async () => {
    const filters = ['all', 'unassigned', 'mine'];

    for (const filter of filters) {
      const durations: number[] = [];

      for (let i = 0; i < 10; i++) {
        const start = Date.now();
        await apiClient.get('/api/v1/tickets/queue', { params: { filter } });
        durations.push(Date.now() - start);
      }

      const s = stats(durations);
      console.log(`Queue filter=${filter}: avg=${s.avg}ms p95=${s.p95}ms`);
      expect(s.p95).toBeLessThan(QUEUE_THRESHOLD_MS);
    }
  }, 60000);
});
