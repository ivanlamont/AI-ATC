/**
 * Soak Test for AI-ATC API
 * Tests system stability over extended period with constant load
 *
 * Run: k6 run k6/load-test-soak.js
 * Run for 1 hour: k6 run k6/load-test-soak.js --duration 1h
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate, Counter, Gauge } from 'k6/metrics';

const apiDuration = new Trend('soak_api_duration');
const errorRate = new Rate('soak_errors');
const memoryUsage = new Gauge('memory_usage');
const requestCounter = new Counter('soak_requests_total');
const userSessionsActive = new Gauge('user_sessions_active');

export const options = {
  stages: [
    { duration: '5m', target: 50 },    // Ramp up to 50 users
    { duration: '8h', target: 50 },    // Stay at 50 for 8 hours
    { duration: '5m', target: 0 },     // Ramp down
  ],
  thresholds: {
    // Strict thresholds for soak test - should be stable
    'soak_errors': ['rate<0.01'],       // Less than 1% error rate
    'soak_api_duration{endpoint:api}': ['p(95)<500', 'p(99)<1000'], // 95% under 500ms, 99% under 1s
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
  // Simulate normal user activity over time
  test_normal_user_flow();

  // Track memory periodically
  if (__ITER % 100 === 0) {
    track_system_metrics();
  }

  userSessionsActive.set(__VU);
}

function test_normal_user_flow() {
  // 1. Check leaderboard (5% of traffic)
  if (Math.random() < 0.05) {
    test_endpoint('/api/leaderboard?limit=50', 'leaderboard');
  }

  // 2. Get scenarios (10% of traffic)
  if (Math.random() < 0.10) {
    test_endpoint('/api/scenarios?difficulty=intermediate', 'scenarios');
  }

  // 3. Get stats (15% of traffic)
  if (Math.random() < 0.15) {
    test_endpoint('/api/users/stats', 'stats');
  }

  // 4. Get user profile (20% of traffic)
  if (Math.random() < 0.20) {
    test_endpoint('/api/users/profile', 'profile');
  }

  // 5. Simulate scenario interaction (50% of traffic)
  if (Math.random() < 0.50) {
    test_scenario_interaction();
  }

  sleep(1);
}

function test_endpoint(path, name) {
  const res = http.get(`${BASE_URL}${path}`, {
    timeout: '10s',
    tags: { endpoint: name },
  });

  const success = check(res, {
    [`${name} status 200`]: (r) => r.status === 200,
  });

  if (!success) {
    errorRate.add(1);
  }

  apiDuration.add(res.timings.duration, { endpoint: name });
  requestCounter.add(1);
}

function test_scenario_interaction() {
  // Simulate starting a scenario
  const startRes = http.post(`${BASE_URL}/api/scenarios/123/start`, '', {
    timeout: '10s',
    tags: { endpoint: 'start_scenario' },
  });

  if (startRes.status === 200) {
    // Simulate playing for a bit
    sleep(Math.random() * 5);

    // Submit score
    const score = Math.random() * 100;
    const payload = JSON.stringify({
      score: score,
      duration: Math.random() * 600,
      aircraftLanded: Math.floor(Math.random() * 3),
    });

    const submitRes = http.post(`${BASE_URL}/api/scores`, payload, {
      headers: { 'Content-Type': 'application/json' },
      timeout: '10s',
      tags: { endpoint: 'submit_score' },
    });

    apiDuration.add(submitRes.timings.duration, { endpoint: 'submit_score' });
  }
}

function track_system_metrics() {
  // Try to fetch system metrics endpoint if available
  const res = http.get(`${BASE_URL}/api/admin/metrics`, {
    timeout: '5s',
  });

  if (res.status === 200) {
    try {
      const data = res.json();
      if (data.memory) {
        memoryUsage.set(data.memory);
      }
    } catch (e) {
      // Metrics endpoint may not be available
    }
  }
}
