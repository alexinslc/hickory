# Performance Tests

This directory contains performance tests for the Hickory Help Desk system.

## Overview

Performance tests validate that the system meets the performance requirements specified in the success criteria (SC-XXX).

## Tests

### Ticket Submission Performance (T075)

**File**: `ticket-submission.perf.test.ts`

**Success Criterion**: SC-003 - Users receive confirmation and ticket ID within 2 seconds of submission

**What it tests**:
- Creates 50 tickets sequentially
- Measures response time for each ticket creation
- Calculates performance metrics (avg, min, max, P50, P95, P99)
- Validates that ≥95% of requests complete within 2 seconds

**Pass Criteria**:
- 95% or more of ticket submissions complete within 2000ms
- Zero failed requests

## Running the Tests

### Prerequisites

1. **API server must be running**:
   ```bash
   cd apps/api
   dotnet run
   ```

2. **Database must be seeded with test user**:
   - Email: `perftest@example.com`
   - Password: `TestPassword123!`
   
   Or set environment variables:
   ```bash
   export TEST_USER_EMAIL=your-test-user@example.com
   export TEST_USER_PASSWORD=YourPassword123!
   ```

### Run Performance Tests

```bash
# From the repository root
npm run test:performance

# Or run a specific test
npm run test:perf:ticket-submission

# With custom API URL
API_URL=https://staging.hickory.example.com npm run test:performance
```

### Environment Variables

- `API_URL` - Base URL of the API (default: `http://localhost:5000`)
- `TEST_USER_EMAIL` - Email of the test user (default: `perftest@example.com`)
- `TEST_USER_PASSWORD` - Password of the test user (default: `TestPassword123!`)

## Interpreting Results

The test outputs:
- **Success Rate**: Percentage of successful API calls
- **Average/Min/Max**: Basic response time statistics
- **P50/P95/P99**: Percentile response times (50th, 95th, 99th percentile)
- **SLA Compliance**: Percentage of requests meeting the 2-second threshold

### Example Output

```
======================================================================
PERFORMANCE TEST RESULTS: Ticket Submission (T075 / SC-003)
======================================================================

SLA Target: 2000ms (2 seconds)
Test Iterations: 50

Success Rate:
  Successful: 50/50 (100.0%)
  Failed: 0/50

Performance Metrics:
  Average:  453.24ms
  Minimum:  312.15ms
  Maximum:  1247.89ms
  P50:      421.33ms
  P95:      876.42ms
  P99:      1180.21ms

SLA Compliance:
  Within 2000ms: 50/50 (100.0%)

✅ PASS: 100.0% of requests met the 2000ms SLA
======================================================================
```

## Adding New Performance Tests

1. Create a new test file: `your-feature.perf.test.ts`
2. Follow the pattern in `ticket-submission.perf.test.ts`:
   - Setup authentication
   - Run multiple iterations
   - Measure performance
   - Calculate statistics
   - Validate against SLA threshold
3. Add npm script to `package.json`
4. Document the test in this README

## CI/CD Integration

Performance tests can be run in CI/CD pipelines:

```yaml
# Example GitHub Actions workflow
- name: Run Performance Tests
  env:
    API_URL: ${{ secrets.STAGING_API_URL }}
    TEST_USER_EMAIL: ${{ secrets.TEST_USER_EMAIL }}
    TEST_USER_PASSWORD: ${{ secrets.TEST_USER_PASSWORD }}
  run: npm run test:performance
```

## Notes

- Performance tests are **not** run as part of the regular test suite (`npm test`)
- They require a running API server and database
- They create real data (tickets) in the database
- Consider using a dedicated test environment or cleanup scripts
- Results may vary based on hardware, network, and database load
