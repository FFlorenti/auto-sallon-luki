import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'

export function NotFoundPage() {
  const { t } = useTranslation()
  return (
    <div className="container-page py-20 text-center">
      <p className="text-6xl font-bold text-zinc-300">404</p>
      <h1 className="mt-4 text-2xl font-bold">{t('notFound.title')}</h1>
      <Link to="/" className="btn-primary mt-6">{t('notFound.home')}</Link>
    </div>
  )
}
