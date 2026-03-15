/**
 * Performance Test: Search Operations
 *
 * Tests full-text search performance with varying query complexity.
 */

import axios, { AxiosInstance } from 'axios';

const API_BASE_URL = process.env.API_URL || 'http://localhost:5000';
const SEARCH_THRESHOLD_MS = 1000;
const TEST_ITERATIONS = 20;

interface TestCredentials {
  email: string;
  password: string;
  accessToken?: string;
}

const TEST_USER: TestCredentials = {
  email: process.env.TEST_USER_EMAIL || 'perftest@example.com',
  password: process.env.TEST_USER_PASSWORD || 'TestPassword123!',
};

let apiClient: AxiosInstance;

async function authenticate(): Promise<void> {
  const response = await axios.post(`${API_BASE_URL}/api/v1/auth/login`, {
    email: TEST_USER.email,
    password: TEST_USER.password,
  });
  TEST_USER.accessToken = response.data.accessToken;
  apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: { Authorization: `Bearer ${TEST_USER.accessToken}` },
  });
}

function stats(durations: number[]) {
  const sorted = [...durations].sort((a, b) => a - b);
  const avg = durations.reduce((s, d) => s + d, 0) / durations.length;
  return {
    min: sorted[0],
    max: sorted[sorted.length - 1],
    avg: Math.round(avg),
    p50: sorted[Math.floor(sorted.length * 0.5)],
    p95: sorted[Math.floor(sorted.length * 0.95)],
    p99: sorted[Math.floor(sorted.length * 0.99)],
  };
}

describe('Search Performance', () => {
  beforeAll(async () => {
    await authenticate();
  }, 10000);

  it(`should complete simple search within ${SEARCH_THRESHOLD_MS}ms`, async () => {
    const durations: number[] = [];

    for (let i = 0; i < TEST_ITERATIONS; i++) {
      const start = Date.now();
      await apiClient.get('/api/v1/search/tickets', {
        params: { q: 'password reset', page: 1, pageSize: 20 },
      });
      durations.push(Date.now() - start);
    }

    const s = stats(durations);
    console.log(`Simple search: avg=${s.avg}ms p95=${s.p95}ms p99=${s.p99}ms`);
    expect(s.p95).toBeLessThan(SEARCH_THRESHOLD_MS);
  }, 60000);

  it(`should complete filtered search within ${SEARCH_THRESHOLD_MS}ms`, async () => {
    const durations: number[] = [];

    for (let i = 0; i < TEST_ITERATIONS; i++) {
      const start = Date.now();
      await apiClient.get('/api/v1/search/tickets', {
        params: {
          q: 'login issue',
          status: 'Open',
          priority: 'High',
          page: 1,
          pageSize: 20,
        },
      });
      durations.push(Date.now() - start);
    }

    const s = stats(durations);
    console.log(`Filtered search: avg=${s.avg}ms p95=${s.p95}ms p99=${s.p99}ms`);
    expect(s.p95).toBeLessThan(SEARCH_THRESHOLD_MS);
  }, 60000);

  it('should handle pagination efficiently', async () => {
    const pageSizes = [10, 25, 50, 100];

    for (const pageSize of pageSizes) {
      const durations: number[] = [];

      for (let i = 0; i < 10; i++) {
        const start = Date.now();
        await apiClient.get('/api/v1/search/tickets', {
          params: { q: 'ticket', page: 1, pageSize },
        });
        durations.push(Date.now() - start);
      }

      const s = stats(durations);
      console.log(`Page size ${pageSize}: avg=${s.avg}ms p95=${s.p95}ms`);
      expect(s.p95).toBeLessThan(SEARCH_THRESHOLD_MS * 2);
    }
  }, 120000);
});
