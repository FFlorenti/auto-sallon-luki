import { createContext, useContext } from 'react'
import type { LoginResponse } from '@/api/types'

export interface AuthState {
  session: LoginResponse | null
  isAdmin: boolean
  signIn: (session: LoginResponse) => void
  signOut: () => void
}

export const AuthContext = createContext<AuthState | null>(null)

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
