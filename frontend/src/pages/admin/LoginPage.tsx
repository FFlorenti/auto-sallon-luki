import { useState, type FormEvent } from 'react'
import { useMutation } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Navigate, useLocation, useNavigate } from 'react-router'
import { ApiError } from '@/api/client'
import { authApi } from '@/api/endpoints'
import { useAuth } from '@/auth/useAuth'
import { Logo } from '@/components/Logo'

export function LoginPage() {
  const { t } = useTranslation()
  const { session, signIn } = useAuth()
  const navigate = useNavigate()
  const from = (useLocation().state as { from?: string } | null)?.from ?? '/admin'
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')

  const login = useMutation({
    mutationFn: () => authApi.login(email, password),
    onSuccess: (response) => {
      signIn(response)
      void navigate(from, { replace: true })
    },
  })

  if (session) return <Navigate to={from} replace />

  const onSubmit = (e: FormEvent) => {
    e.preventDefault()
    login.mutate()
  }

  const error = login.error instanceof ApiError
    ? login.error.status === 429 ? t('admin.login.tooMany') : t('admin.login.invalid')
    : login.isError ? t('common.error') : null

  return (
    <div className="relative flex min-h-screen items-center justify-center overflow-hidden bg-ink-950 p-4">
      <div className="pointer-events-none absolute -top-40 left-1/2 h-96 w-96 -translate-x-1/2 rounded-full bg-brand-600/25 blur-3xl" />
      <form onSubmit={onSubmit} className="card relative w-full max-w-sm space-y-4 p-8">
        <div className="text-center">
          <Logo variant="onLight" className="mx-auto h-16 w-auto" />
          <h1 className="mt-4 text-xl font-bold">{t('admin.login.title')}</h1>
        </div>
        <div>
          <label htmlFor="email" className="label">{t('admin.login.email')}</label>
          <input id="email" type="email" className="input" autoComplete="username" required value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>
        <div>
          <label htmlFor="password" className="label">{t('admin.login.password')}</label>
          <input id="password" type="password" className="input" autoComplete="current-password" required value={password} onChange={(e) => setPassword(e.target.value)} />
        </div>
        {error && <p className="rounded-lg bg-red-50 p-3 text-sm text-red-700" role="alert">{error}</p>}
        <button type="submit" className="btn-primary w-full" disabled={login.isPending}>
          {login.isPending ? t('common.loading') : t('admin.login.submit')}
        </button>
      </form>
    </div>
  )
}
