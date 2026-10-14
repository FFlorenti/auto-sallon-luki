import { useTranslation } from 'react-i18next'
import { Link, Navigate, NavLink, Outlet, useLocation } from 'react-router'
import { useAuth } from '@/auth/useAuth'
import { LanguageSwitcher } from '@/components/Layout'
import { Logo } from '@/components/Logo'

export function AdminLayout() {
  const { t } = useTranslation()
  const { session, signOut } = useAuth()
  const location = useLocation()

  if (!session) {
    return <Navigate to="/admin/login" replace state={{ from: location.pathname }} />
  }

  const navClass = ({ isActive }: { isActive: boolean }) =>
    `rounded-lg border-l-2 px-3 py-2 text-sm font-medium transition ${isActive ? 'border-brand-600 bg-white/10 text-white' : 'border-transparent text-zinc-400 hover:bg-white/5 hover:text-white'}`

  return (
    <div className="min-h-screen bg-zinc-50 md:grid md:grid-cols-[240px_1fr]">
      <aside className="bg-ink-950 p-4 md:sticky md:top-0 md:h-screen md:p-5">
        <div className="flex items-center justify-between md:block">
          <Link to="/admin" className="block">
            <Logo className="h-10 w-auto md:h-14" />
            <span className="mt-1 hidden text-xs font-semibold tracking-widest text-zinc-500 uppercase md:block">{t('admin.title')}</span>
          </Link>
          <LanguageSwitcher className="md:mt-4 md:w-fit" />
        </div>
        <nav className="mt-4 flex gap-1 overflow-x-auto md:mt-8 md:flex-col">
          <NavLink to="/admin" end className={navClass}>{t('admin.nav.dashboard')}</NavLink>
          <NavLink to="/admin/cars" className={navClass}>{t('admin.nav.cars')}</NavLink>
          <NavLink to="/admin/inquiries" className={navClass}>{t('admin.nav.inquiries')}</NavLink>
        </nav>
        <div className="mt-4 flex gap-2 border-t border-white/10 pt-4 text-sm md:mt-8 md:flex-col">
          <span className="hidden truncate text-zinc-400 md:block">{session.email}</span>
          <Link to="/" className="text-zinc-300 hover:text-white">{t('admin.nav.website')}</Link>
          <button type="button" onClick={signOut} className="text-left text-zinc-300 hover:text-white">
            {t('admin.nav.logout')}
          </button>
        </div>
      </aside>

      <main className="p-4 sm:p-8">
        <Outlet />
      </main>
    </div>
  )
}
