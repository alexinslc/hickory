/**
 * Performance Test: Concurrent Operations
 *
 * Tests system behavior under concurrent load.
 */

import axios, { AxiosInstance } from 'axios';

const API_BASE_URL = process.env.API_URL || 'http://localhost:5000';

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
    timeout: 10000,
  });
}

describe('Concurrent Operations Performance', () => {
  beforeAll(async () => {
    await authenticate();
  }, 10000);

  it('should handle 10 concurrent ticket creations', async () => {
    const concurrency = 10;
    const start = Date.now();

    const promises = Array.from({ length: concurrency }, (_, i) =>
      apiClient.post('/api/v1/tickets', {
        title: `Concurrent Test Ticket ${i}`,
        description: `Created during concurrent performance test iteration ${i}`,
        priority: 'Medium',
      }).then(() => ({ success: true, error: null }))
        .catch((err) => ({ success: false, error: err.message }))
    );

    const results = await Promise.all(promises);
    const totalMs = Date.now() - start;
    const successCount = results.filter((r) => r.success).length;

    console.log(`${concurrency} concurrent creates: ${totalMs}ms total, ${successCount}/${concurrency} succeeded`);
    expect(successCount).toBeGreaterThanOrEqual(concurrency * 0.9);
    expect(totalMs).toBeLessThan(10000);
  }, 30000);

  it('should handle 20 concurrent reads', async () => {
    const concurrency = 20;
    const start = Date.now();

    const promises = Array.from({ length: concurrency }, () =>
      apiClient.get('/api/v1/tickets', { params: { page: 1, pageSize: 10 } })
        .then(() => ({ success: true }))
        .catch(() => ({ success: false }))
    );

    const results = await Promise.all(promises);
    const totalMs = Date.now() - start;
    const successCount = results.filter((r) => r.success).length;

    console.log(`${concurrency} concurrent reads: ${totalMs}ms total, ${successCount}/${concurrency} succeeded`);
    expect(successCount).toBe(concurrency);
    expect(totalMs).toBeLessThan(5000);
  }, 30000);

  it('should handle mixed read/write workload', async () => {
    const reads = 15;
    const writes = 5;
    const start = Date.now();

    const readPromises = Array.from({ length: reads }, () =>
      apiClient.get('/api/v1/tickets', { params: { page: 1, pageSize: 10 } })
        .then(() => ({ type: 'read', success: true }))
        .catch(() => ({ type: 'read', success: false }))
    );

    const writePromises = Array.from({ length: writes }, (_, i) =>
      apiClient.post('/api/v1/tickets', {
        title: `Mixed Workload Ticket ${i}`,
        description: 'Created during mixed workload test',
        priority: 'Low',
      }).then(() => ({ type: 'write', success: true }))
        .catch(() => ({ type: 'write', success: false }))
    );

    const results = await Promise.all([...readPromises, ...writePromises]);
    const totalMs = Date.now() - start;
    const readSuccess = results.filter((r) => r.type === 'read' && r.success).length;
    const writeSuccess = results.filter((r) => r.type === 'write' && r.success).length;

    console.log(`Mixed workload: ${totalMs}ms, reads=${readSuccess}/${reads}, writes=${writeSuccess}/${writes}`);
    expect(readSuccess).toBe(reads);
    expect(writeSuccess).toBeGreaterThanOrEqual(writes * 0.8);
  }, 30000);
});
