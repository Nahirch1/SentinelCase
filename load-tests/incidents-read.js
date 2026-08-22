import http from 'k6/http'
import { check, sleep } from 'k6'

export const options = {
  stages: [
    { duration: '20s', target: 5 },
    { duration: '40s', target: 15 },
    { duration: '20s', target: 30 },
    { duration: '20s', target: 0 },
  ],

  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<500'],
  },
}

const baseUrl =
  __ENV.BASE_URL || 'http://host.docker.internal:8080'

export default function () {
  const response = http.get(
    `${baseUrl}/health`,
  )

  check(response, {
    'health returns 200': (r) =>
      r.status === 200,
  })

  sleep(0.2)
}
