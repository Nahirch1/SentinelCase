const TOKEN_STORAGE_KEY = 'sistema-centinela-api-token'

export function getApiToken(): string | undefined {
  const storedToken =
    sessionStorage.getItem(TOKEN_STORAGE_KEY)?.trim()

  if (storedToken) {
    return storedToken
  }

  const developmentToken =
    import.meta.env.VITE_API_TOKEN?.trim()

  return developmentToken || undefined
}

export function setApiToken(
  token: string,
): void {
  const normalizedToken = token.trim()

  if (!normalizedToken) {
    sessionStorage.removeItem(TOKEN_STORAGE_KEY)
    return
  }

  sessionStorage.setItem(
    TOKEN_STORAGE_KEY,
    normalizedToken,
  )
}

export function clearApiToken(): void {
  sessionStorage.removeItem(TOKEN_STORAGE_KEY)
}
