import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, NavLink, Outlet, ScrollRestoration } from 'react-router'
import { LANGUAGES } from '@/i18n'
import { config, phoneHref, whatsAppHref } from '@/lib/config'
import { useFavorites } from '@/lib/favorites'
import { HeartIcon, MapPinIcon, MenuIcon, PhoneIcon, WhatsAppIcon } from './icons'
import { Logo } from './Logo'

const YEAR = new Date().getFullYear()

export function LanguageSwitcher({ className = '', dark = true }: { className?: string; dark?: boolean }) {
  const { i18n } = useTranslation()
  return (
    <div
      className={`flex rounded-lg border p-0.5 text-xs font-semibold ${dark ? 'border-white/15' : 'border-zinc-300'} ${className}`}
      role="group"
      aria-label="Language"
    >
      {LANGUAGES.map((lng) => {
        const active = i18n.language === lng
        return (
          <button
            key={lng}
            type="button"
            onClick={() => void i18n.changeLanguage(lng)}
            aria-pressed={active}
            className={`rounded-md px-2.5 py-1 uppercase transition ${
              active ? 'bg-brand-600 text-white' : dark ? 'text-zinc-300 hover:text-white' : 'text-zinc-600 hover:text-zinc-900'
            }`}
          >
            {lng}
          </button>
        )
      })}
    </div>
  )
}

export function Layout() {
  const { t } = useTranslation()
  const { ids } = useFavorites()
  const [menuOpen, setMenuOpen] = useState(false)

  const navClass = ({ isActive }: { isActive: boolean }) =>
    `relative px-3 py-2 text-sm font-medium transition ${
      isActive
        ? 'text-white after:absolute after:inset-x-3 after:-bottom-[17px] after:h-0.5 after:rounded-full after:bg-brand-600 max-md:after:hidden'
        : 'text-zinc-400 hover:text-white'
    }`

  const links = (
    <>
      <NavLink to="/" end className={navClass} onClick={() => setMenuOpen(false)}>{t('nav.cars')}</NavLink>
      <NavLink to="/favorites" className={navClass} onClick={() => setMenuOpen(false)}>
        <span className="inline-flex items-center gap-1.5">
          <HeartIcon width={16} height={16} filled={ids.length > 0} className={ids.length > 0 ? 'text-brand-500' : ''} />
          {t('nav.favorites')}
          {ids.length > 0 && <span className="rounded-full bg-brand-600 px-1.5 text-xs text-white">{ids.length}</span>}
        </span>
      </NavLink>
      <NavLink to="/contact" className={navClass} onClick={() => setMenuOpen(false)}>{t('nav.contact')}</NavLink>
    </>
  )

  return (
    <div className="flex min-h-screen flex-col">
      <a href="#main" className="sr-only focus:not-sr-only focus:absolute focus:z-50 focus:bg-white focus:p-3">
        Skip to content
      </a>

      <header className="sticky top-0 z-30 border-b border-white/10 bg-ink-950/95 backdrop-blur">
        <div className="container-page flex h-[72px] items-center justify-between gap-4">
          <Link to="/" aria-label={config.dealerName} className="shrink-0">
            <Logo className="h-14 w-auto" />
          </Link>

          <nav className="hidden items-center gap-1 md:flex">{links}</nav>

          <div className="flex items-center gap-2">
            {config.dealerPhone && (
              <a href={phoneHref(config.dealerPhone)} className="btn-primary hidden lg:inline-flex">
                <PhoneIcon width={16} height={16} /> {config.dealerPhone}
              </a>
            )}
            <LanguageSwitcher />
            <button
              type="button"
              className="rounded-lg p-2 text-zinc-300 hover:bg-white/10 hover:text-white md:hidden"
              onClick={() => setMenuOpen((o) => !o)}
              aria-expanded={menuOpen}
              aria-label={t('nav.menu')}
            >
              <MenuIcon />
            </button>
          </div>
        </div>
        {menuOpen && <nav className="container-page flex flex-col gap-1 border-t border-white/10 py-3 md:hidden">{links}</nav>}
      </header>

      <main id="main" className="flex-1">
        <Outlet />
      </main>

      <footer className="mt-16 border-t-4 border-brand-600 bg-ink-950 text-zinc-400">
        <div className="container-page grid gap-10 py-12 sm:grid-cols-2 lg:grid-cols-3">
          <div>
            <Logo className="h-14 w-auto" />
            <p className="mt-4 max-w-xs text-sm">{t('listing.heroText')}</p>
          </div>

          <div>
            <h2 className="text-sm font-semibold tracking-wider text-white uppercase">{t('footer.explore')}</h2>
            <ul className="mt-4 space-y-2 text-sm">
              <li><Link to="/" className="hover:text-white">{t('nav.cars')}</Link></li>
              <li><Link to="/favorites" className="hover:text-white">{t('nav.favorites')}</Link></li>
              <li><Link to="/contact" className="hover:text-white">{t('nav.contact')}</Link></li>
            </ul>
          </div>

          <div>
            <h2 className="text-sm font-semibold tracking-wider text-white uppercase">{t('footer.contact')}</h2>
            <ul className="mt-4 space-y-3 text-sm">
              {config.dealerAddress && (
                <li className="flex items-center gap-2"><MapPinIcon width={16} height={16} className="text-brand-500" /> {config.dealerAddress}</li>
              )}
              {config.dealerPhone && (
                <li>
                  <a href={phoneHref(config.dealerPhone)} className="flex items-center gap-2 hover:text-white">
                    <PhoneIcon width={16} height={16} className="text-brand-500" /> {config.dealerPhone}
                  </a>
                </li>
              )}
              {config.dealerWhatsApp && (
                <li>
                  <a href={whatsAppHref()} target="_blank" rel="noopener noreferrer" className="flex items-center gap-2 hover:text-white">
                    <WhatsAppIcon width={16} height={16} className="text-[#25D366]" /> WhatsApp
                  </a>
                </li>
              )}
            </ul>
          </div>
        </div>
        <div className="border-t border-white/10">
          <p className="container-page py-5 text-xs">© {YEAR} {config.dealerName}. {t('footer.rights')}</p>
        </div>
      </footer>

      <ScrollRestoration />
    </div>
  )
}
