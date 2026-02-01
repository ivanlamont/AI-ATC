/**
 * Stress Test for AI-ATC API
 * Tests system limits and breaking points
 *
 * Run: k6 run k6/load-test-stress.js
 * Run with custom VUs: k6 run k6/load-test-stress.js --vus 500 --duration 2m
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate, Counter } from 'k6/metrics';

const apiDuration = new Trend('stress_api_duration');
const errorRate = new Rate('stress_errors');
const dropoutRate = new Rate('stress_dropouts');
const successRate = new Rate('stress_success');
const requestCounter = new Counter('stress_requests');

export const options = {
  stages: [
    { duration: '1m', target: 100 },   // 100 users
    { duration: '2m', target: 250 },   // Ramp to 250 users
    { duration: '3m', target: 500 },   // Ramp to 500 users (stress level)
    { duration: '2m', target: 750 },   // Ramp to 750 users (breaking point)
    { duration: '3m', target: 750 },   // Hold at breaking point
    { duration: '2m', target: 100 },   // Ramp down
    { duration: '1m', target: 0 },     // Back to zero
  ],
  thresholds: {
    // More lenient thresholds for stress test
    'stress_errors': ['rate<0.5'],       // Allow up to 50% errors
    'stress_api_duration{endpoint:api}': ['p(99)<5000'], // 99% under 5 seconds
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

/**
 * Stress test: Hammer the API with requests
 */
export default function () {
  const endpoints = [
    {
      method: 'GET',
      path: '/api/leaderboard?difficulty=beginner&limit=100',
      name: 'leaderboard',
    },
    {
      method: 'GET',
      path: '/api/scenarios?difficulty=intermediate&limit=20',
      name: 'scenarios',
    },
    {
      method: 'GET',
      path: '/api/users/stats',
      name: 'stats',
    },
  ];

  // Pick random endpoint
  const endpoint = endpoints[Math.floor(Math.random() * endpoints.length)];

  let res;
  try {
    if (endpoint.method === 'GET') {
      res = http.get(`${BASE_URL}${endpoint.path}`, {
        timeout: '30s',
        tags: { endpoint: endpoint.name },
      });
    }

    const success = res.status === 200;

    check(res, {
      'status is 200': () => success,
    });

    if (success) {
      successRate.add(1);
    } else {
      errorRate.add(1);
      if (res.status >= 500) {
        dropoutRate.add(1);
      }
    }

    apiDuration.add(res.timings.duration, { endpoint: endpoint.name });
    requestCounter.add(1);
  } catch (e) {
    errorRate.add(1);
    dropoutRate.add(1);
    console.error(`Request failed: ${e.message}`);
  }

  // Vary sleep to randomize request patterns
  sleep(Math.random() * 2);
}
