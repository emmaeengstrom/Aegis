const API_BASE_URL = 'https://localhost:7269'

export function getAuthToken() {
  return sessionStorage.getItem('aegis_token')
}

export function clearAuthSession() {
  sessionStorage.removeItem('aegis_token')
  sessionStorage.removeItem('aegis_user_email')
  sessionStorage.removeItem('aegis_user_id')
}

export async function apiFetch(
  path: string,
  options: RequestInit = {},
) {
  const token = getAuthToken()

  const headers = new Headers(options.headers)

  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  if (
    options.body &&
    !headers.has('Content-Type')
  ) {
    headers.set(
      'Content-Type',
      'application/json',
    )
  }

  const response = await fetch(
    `${API_BASE_URL}${path}`,
    {
      ...options,
      headers,
    },
  )

  if (response.status === 401) {
    clearAuthSession()
  }

  return response
} 