export class ApiError extends Error {
  readonly status: number
  readonly fieldErrors: Record<string, string[]>

  constructor(status: number, message: string, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

let accessToken: string | null = null
let onUnauthorized: (() => void) | null = null

export function setAccessToken(token: string | null) {
  accessToken = token
}

export function setUnauthorizedHandler(handler: (() => void) | null) {
  onUnauthorized = handler
}

type QueryValue = string | number | boolean | null | undefined

export function toQueryString(params: object): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params as Record<string, QueryValue>)) {
    if (value !== undefined && value !== null && value !== '') {
      search.set(key, String(value))
    }
  }
  const qs = search.toString()
  return qs ? `?${qs}` : ''
}

export async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  if (init.body && !(init.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
  }
  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  const response = await fetch(path, { ...init, headers })

  if (response.status === 401 && accessToken) {
    onUnauthorized?.()
  }

  if (!response.ok) {
    throw await toApiError(response)
  }

  if (response.status === 204 || response.status === 202 || response.headers.get('content-length') === '0') {
    return undefined as T
  }

  const text = await response.text()
  return (text ? JSON.parse(text) : undefined) as T
}

async function toApiError(response: Response): Promise<ApiError> {
  if (response.status === 429) {
    return new ApiError(429, 'Too many requests')
  }

  try {
    const problem = (await response.json()) as {
      title?: string
      detail?: string
      errors?: Record<string, string[]>
    }
    return new ApiError(response.status, problem.detail ?? problem.title ?? response.statusText, problem.errors ?? {})
  } catch {
    return new ApiError(response.status, response.statusText)
  }
}

export const api = {
  get: <T>(path: string) => apiFetch<T>(path),
  post: <T>(path: string, body?: unknown) =>
    apiFetch<T>(path, { method: 'POST', body: body instanceof FormData ? body : JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) => apiFetch<T>(path, { method: 'PUT', body: JSON.stringify(body) }),
  patch: <T>(path: string, body: unknown) => apiFetch<T>(path, { method: 'PATCH', body: JSON.stringify(body) }),
  delete: <T>(path: string) => apiFetch<T>(path, { method: 'DELETE' }),
}
