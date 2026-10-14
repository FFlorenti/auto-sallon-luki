import { useTranslation } from 'react-i18next'
import type { CarStatus } from '@/api/types'

export function Spinner() {
  const { t } = useTranslation()
  return (
    <div className="flex justify-center py-16" role="status">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-zinc-200 border-t-brand-600" />
      <span className="sr-only">{t('common.loading')}</span>
    </div>
  )
}

export function ErrorState({ onRetry, message }: { onRetry?: () => void; message?: string }) {
  const { t } = useTranslation()
  return (
    <div className="card mx-auto my-10 max-w-md p-6 text-center" role="alert">
      <p className="text-zinc-700">{message ?? t('common.error')}</p>
      {onRetry && (
        <button type="button" className="btn-secondary mt-4" onClick={onRetry}>
          {t('common.retry')}
        </button>
      )}
    </div>
  )
}

const statusStyles: Record<CarStatus, string> = {
  Available: 'bg-emerald-100 text-emerald-800',
  Sold: 'bg-ink-950 text-white',
}

export function StatusBadge({ status }: { status: CarStatus }) {
  const { t } = useTranslation()
  return (
    <span className={`rounded-md px-2.5 py-1 text-xs font-bold tracking-wide uppercase shadow-sm ${statusStyles[status]}`}>
      {t(`status.${status}`)}
    </span>
  )
}
