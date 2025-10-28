# Quick Start: Performance Testing (T075)

## What was created

1. **Performance Test**: `tests/performance/ticket-submission.perf.test.ts`
   - Tests ticket submission performance (SC-003)
   - Runs 50 iterations
   - Measures response times and calculates statistics
   - Validates ≥95% of requests complete within 2 seconds

2. **Setup Script**: `tests/performance/setup.sh`
   - Creates test user automatically
   - Validates API is running
   - Bash script for easy setup

3. **Documentation**: `tests/performance/README.md`
   - Complete guide to performance testing
   - Environment variable documentation
   - CI/CD integration examples

4. **NPM Scripts**: Added to `package.json`
   - `npm run test:performance` - Run all performance tests
   - `npm run test:perf:ticket-submission` - Run specific test

## How to run

### Step 1: Start the API

```bash
cd apps/api
dotnet run
```

Wait for the message: "Now listening on: http://localhost:5000"

### Step 2: Setup test user (one-time)

```bash
cd ../../
./tests/performance/setup.sh
```

This creates the test user: `perftest@example.com` with password `TestPassword123!`

### Step 3: Run the performance test

```bash
npm run test:performance
```

## Expected Output

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

✅ All tests passed!
```

## Troubleshooting

### API not running
```
❌ ERROR: API is not running at http://localhost:5000
```
**Solution**: Start the API server with `cd apps/api && dotnet run`

### Authentication failed
```
✗ Authentication failed: Request failed with status code 401
```
**Solution**: Run the setup script to create the test user: `./tests/performance/setup.sh`

### Database connection error
```
Cannot connect to database
```
**Solution**: Ensure PostgreSQL is running and the connection string in `apps/api/appsettings.json` is correct

## Custom Configuration

### Use different API URL (staging/production)

```bash
API_URL=https://staging.example.com npm run test:performance
```

### Use different test credentials

```bash
TEST_USER_EMAIL=mytest@example.com \
TEST_USER_PASSWORD=MyPassword123! \
npm run test:performance
```

## Success Criteria

T075 is considered **PASSED** when:
- ✅ Test runs without errors
- ✅ ≥95% of requests complete within 2000ms
- ✅ Zero failed requests
- ✅ Exit code is 0

The test will automatically fail and exit with code 1 if any criterion is not met.
