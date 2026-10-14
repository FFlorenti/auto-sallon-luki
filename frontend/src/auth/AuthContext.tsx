import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { setAccessToken, setUnauthorizedHandler } from '@/api/client'
import type { LoginResponse } from '@/api/types'
import { AuthContext } from './useAuth'

const KEY = 'autoluki.session'

function loadSession(): LoginResponse | null {
  try {
    const raw = sessionStorage.getItem(KEY)
    if (!raw) return null
    const session = JSON.parse(raw) as LoginResponse
    return new Date(session.expiresAt) > new Date() ? session : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<LoginResponse | null>(() => {
    const initial = loadSession()
    setAccessToken(initial?.accessToken ?? null)
    return initial
  })

  const signIn = useCallback((s: LoginResponse) => {
    sessionStorage.setItem(KEY, JSON.stringify(s))
    setAccessToken(s.accessToken)
    setSession(s)
  }, [])

  const signOut = useCallback(() => {
    sessionStorage.removeItem(KEY)
    setAccessToken(null)
    setSession(null)
  }, [])

  useEffect(() => {
    setUnauthorizedHandler(signOut)
    return () => setUnauthorizedHandler(null)
  }, [signOut])

  useEffect(() => {
    if (!session) return
    const ms = new Date(session.expiresAt).getTime() - Date.now()
    const timer = setTimeout(signOut, Math.max(ms, 0))
    return () => clearTimeout(timer)
  }, [session, signOut])

  const value = useMemo(
    () => ({ session, isAdmin: session?.roles.includes('Admin') ?? false, signIn, signOut }),
    [session, signIn, signOut],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
