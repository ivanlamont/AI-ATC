/**
 * Basic Load Test for AI-ATC API
 * Tests core endpoints with concurrent users
 *
 * Run: k6 run k6/load-test-basic.js
 * Run with custom settings: k6 run k6/load-test-basic.js --vus 100 --duration 30s
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter, Gauge } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const successRate = new Rate('success');
const apiDuration = new Trend('api_duration');
const activeUsers = new Gauge('vus');
const requestCounter = new Counter('http_requests_total');

// Configuration
export const options = {
  stages: [
    { duration: '2m', target: 10 },    // Ramp-up to 10 users
    { duration: '3m', target: 50 },    // Ramp-up to 50 users
    { duration: '2m', target: 100 },   // Ramp-up to 100 users
    { duration: '5m', target: 100 },   // Stay at 100 users
    { duration: '3m', target: 50 },    // Ramp-down to 50 users
    { duration: '2m', target: 0 },     // Ramp-down to 0 users
  ],
  thresholds: {
    // API endpoint latency SLAs
    'api_duration{endpoint:login}': ['p(95)<500'],      // 95% of logins < 500ms
    'api_duration{endpoint:getScenario}': ['p(95)<300'], // 95% of scenario gets < 300ms
    'api_duration{endpoint:submitScore}': ['p(95)<200'], // 95% of score submissions < 200ms

    // Error rate SLAs
    'errors': ['rate<0.1'],              // Error rate < 10%
    'http_requests_total': ['rate>0'],  // Some requests succeed
  },
};

// Test data
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const TEST_EMAIL = 'loadtest@example.com';
const TEST_PASSWORD = 'TestPassword123!';

let authToken = '';
let userId = '';
let scenarioId = '';

/**
 * Setup: Initialize test data
 */
export function setup() {
  console.log(`Starting load test against ${BASE_URL}`);

  // Test connectivity
  const res = http.get(`${BASE_URL}/health`, {
    timeout: '5s',
  });

  if (res.status !== 200) {
    throw new Error(`API not healthy: ${res.status}`);
  }

  console.log('API is healthy, starting load test');

  return {
    baseUrl: BASE_URL,
    email: TEST_EMAIL,
    password: TEST_PASSWORD,
  };
}

/**
 * VU Lifecycle: Prepare user for testing
 */
export function setup_user() {
  // Register/login user
  const payload = JSON.stringify({
    email: `${TEST_EMAIL}${__VU}`,
    password: TEST_PASSWORD,
    username: `loadtestuser${__VU}`,
  });

  const res = http.post(`${BASE_URL}/api/auth/register`, payload, {
    headers: {
      'Content-Type': 'application/json',
    },
    timeout: '10s',
  });

  if (res.status === 201 || res.status === 400) {
    // User created or already exists, try login
    return login(`${TEST_EMAIL}${__VU}`, TEST_PASSWORD);
  }

  throw new Error(`Registration failed: ${res.status}`);
}

/**
 * Login and get auth token
 */
function login(email, password) {
  const payload = JSON.stringify({
    email: email,
    password: password,
  });

  const res = http.post(`${BASE_URL}/api/auth/login`, payload, {
    headers: {
      'Content-Type': 'application/json',
    },
    timeout: '10s',
  });

  const success = check(res, {
    'login status is 200': (r) => r.status === 200,
    'login response has token': (r) => r.json('token') !== null,
  });

  if (!success) {
    errorRate.add(1);
    throw new Error(`Login failed: ${res.status}`);
  }

  successRate.add(1);
  return {
    token: res.json('token'),
    userId: res.json('userId'),
  };
}

/**
 * Main load test scenarios
 */
export default function (data) {
  activeUsers.set(__VU);

  // Scenario 1: Get leaderboard
  test_leaderboard();
  sleep(1);

  // Scenario 2: Get scenario list
  test_scenario_list();
  sleep(1);

  // Scenario 3: Start and play scenario
  test_play_scenario();
  sleep(2);

  // Scenario 4: Submit score
  test_submit_score();
  sleep(1);

  // Scenario 5: Get user profile
  test_user_profile();
  sleep(1);

  // Scenario 6: Update settings
  test_update_settings();
  sleep(1);
}

/**
 * Test: Get Leaderboard
 */
function test_leaderboard() {
  const res = http.get(`${BASE_URL}/api/leaderboard?difficulty=intermediate&limit=50`, {
    timeout: '5s',
  });

  const success = check(res, {
    'leaderboard status 200': (r) => r.status === 200,
    'leaderboard has entries': (r) => r.json('entries').length > 0,
  });

  errorRate.add(!success ? 1 : 0);
  successRate.add(success ? 1 : 0);
  apiDuration.add(res.timings.duration, { endpoint: 'leaderboard' });
  requestCounter.add(1);
}

/**
 * Test: Get Scenario List
 */
function test_scenario_list() {
  const res = http.get(`${BASE_URL}/api/scenarios?difficulty=beginner`, {
    timeout: '5s',
  });

  const success = check(res, {
    'scenario list status 200': (r) => r.status === 200,
    'scenario list has items': (r) => r.json('scenarios').length > 0,
  });

  errorRate.add(!success ? 1 : 0);
  successRate.add(success ? 1 : 0);
  apiDuration.add(res.timings.duration, { endpoint: 'getScenario' });
  requestCounter.add(1);
}

/**
 * Test: Play Scenario
 */
function test_play_scenario() {
  // Get scenarios first
  const listRes = http.get(`${BASE_URL}/api/scenarios?difficulty=beginner`, {
    timeout: '5s',
  });

  if (listRes.status !== 200) {
    return;
  }

  const scenarios = listRes.json('scenarios');
  if (scenarios.length === 0) {
    return;
  }

  const scenarioId = scenarios[0].id;

  // Start scenario
  const startRes = http.post(`${BASE_URL}/api/scenarios/${scenarioId}/start`, '', {
    headers: {
      'Authorization': `Bearer ${authToken}`,
    },
    timeout: '10s',
  });

  const success = check(startRes, {
    'start scenario status 200': (r) => r.status === 200,
    'scenario has session': (r) => r.json('sessionId') !== null,
  });

  errorRate.add(!success ? 1 : 0);
  successRate.add(success ? 1 : 0);
  apiDuration.add(startRes.timings.duration, { endpoint: 'startScenario' });
  requestCounter.add(1);
}

/**
 * Test: Submit Score
 */
function test_submit_score() {
  const payload = JSON.stringify({
    scenarioId: scenarioId,
    score: Math.random() * 100,
    duration: Math.random() * 1000,
    aircraftLanded: Math.floor(Math.random() * 5),
    separationViolations: Math.floor(Math.random() * 2),
  });

  const res = http.post(`${BASE_URL}/api/scores`, payload, {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${authToken}`,
    },
    timeout: '10s',
  });

  const success = check(res, {
    'submit score status 201': (r) => r.status === 201,
  });

  errorRate.add(!success ? 1 : 0);
  successRate.add(success ? 1 : 0);
  apiDuration.add(res.timings.duration, { endpoint: 'submitScore' });
  requestCounter.add(1);
}

/**
 * Test: Get User Profile
 */
function test_user_profile() {
  const res = http.get(`${BASE_URL}/api/users/profile`, {
    headers: {
      'Authorization': `Bearer ${authToken}`,
    },
    timeout: '5s',
  });

  const success = check(res, {
    'profile status 200': (r) => r.status === 200,
    'profile has email': (r) => r.json('email') !== null,
  });

  errorRate.add(!success ? 1 : 0);
  successRate.add(success ? 1 : 0);
  apiDuration.add(res.timings.duration, { endpoint: 'getProfile' });
  requestCounter.add(1);
}

/**
 * Test: Update Settings
 */
function test_update_settings() {
  const payload = JSON.stringify({
    difficulty: 'intermediate',
    soundEnabled: true,
    notifications: false,
  });

  const res = http.patch(`${BASE_URL}/api/users/settings`, payload, {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${authToken}`,
    },
    timeout: '5s',
  });

  const success = check(res, {
    'settings update status 200': (r) => r.status === 200,
  });

  errorRate.add(!success ? 1 : 0);
  successRate.add(success ? 1 : 0);
  apiDuration.add(res.timings.duration, { endpoint: 'updateSettings' });
  requestCounter.add(1);
}

/**
 * Teardown: Clean up
 */
export function teardown() {
  console.log('Load test completed');
  console.log(`Total VUs: ${__VU}`);
  console.log(`Test completed at: ${new Date().toISOString()}`);
}
