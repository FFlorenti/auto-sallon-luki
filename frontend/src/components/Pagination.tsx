import { useTranslation } from 'react-i18next'
import { ChevronIcon } from './icons'

interface Props {
  page: number
  totalPages: number
  onPageChange: (page: number) => void
}

function pageNumbers(page: number, total: number): (number | '…')[] {
  const pages = new Set([1, total, page - 1, page, page + 1].filter((p) => p >= 1 && p <= total))
  const sorted = [...pages].sort((a, b) => a - b)
  return sorted.flatMap((p, i) => (i > 0 && p - sorted[i - 1] > 1 ? ['…' as const, p] : [p]))
}

export function Pagination({ page, totalPages, onPageChange }: Props) {
  const { t } = useTranslation()
  if (totalPages <= 1) return null

  return (
    <nav className="mt-8 flex items-center justify-center gap-1" aria-label={t('listing.pageOf', { page, total: totalPages })}>
      <button
        type="button"
        className="btn-secondary px-3"
        disabled={page <= 1}
        onClick={() => onPageChange(page - 1)}
        aria-label={t('listing.prev')}
      >
        <ChevronIcon direction="left" />
      </button>
      {pageNumbers(page, totalPages).map((p, i) =>
        p === '…' ? (
          <span key={`gap-${i}`} className="px-2 text-zinc-400">…</span>
        ) : (
          <button
            key={p}
            type="button"
            onClick={() => onPageChange(p)}
            aria-current={p === page ? 'page' : undefined}
            className={p === page ? 'btn-primary min-w-10 px-3' : 'btn-secondary min-w-10 px-3'}
          >
            {p}
          </button>
        ),
      )}
      <button
        type="button"
        className="btn-secondary px-3"
        disabled={page >= totalPages}
        onClick={() => onPageChange(page + 1)}
        aria-label={t('listing.next')}
      >
        <ChevronIcon />
      </button>
    </nav>
  )
}
