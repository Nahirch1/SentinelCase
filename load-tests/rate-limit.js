import http from 'k6/http'
import { check } from 'k6'

export const options = {
  vus: 1,
  iterations: 120,
}

const baseUrl =
  __ENV.BASE_URL || 'http://api:8080'

export default function () {
  const response = http.get(
    `${baseUrl}/health`,
  )

  check(response, {
    '200 or 429': (r) =>
      r.status === 200 || r.status === 429,
  })
}
