import http from 'k6/http'
import { check, sleep } from 'k6'

export const options = {
  vus: 1,
  duration: '1m',

  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: [
      'p(95)<500',
      'p(99)<1000',
    ],
  },
}

const baseUrl =
  __ENV.BASE_URL || 'http://api:8080'

const token = __ENV.TOKEN

export default function () {
  const response = http.get(
    `${baseUrl}/api/incidents?pageNumber=1&pageSize=20`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    },
  )

  check(response, {
    'incidents returns 200': (r) =>
      r.status === 200,
  })

  sleep(0.75)
}
