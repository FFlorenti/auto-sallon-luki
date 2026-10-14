import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { adminApi } from '@/api/endpoints'
import { ErrorState, Spinner } from '@/components/Feedback'

export function DashboardPage() {
  const { t } = useTranslation()
  const { data, isPending, isError, refetch } = useQuery({ queryKey: ['admin', 'dashboard'], queryFn: adminApi.dashboard })

  if (isPending) return <Spinner />
  if (isError) return <ErrorState onRetry={() => void refetch()} />

  const stats: { label: string; value: number; to?: string; highlight?: boolean }[] = [
    { label: t('admin.dashboard.inStock'), value: data.carsInStock, to: '/admin/cars?status=Available' },
    { label: t('admin.dashboard.soldMonth'), value: data.carsSoldThisMonth },
    { label: t('admin.dashboard.soldTotal'), value: data.carsSoldTotal, to: '/admin/cars?status=Sold' },
    { label: t('admin.dashboard.inquiriesNew'), value: data.inquiriesNew, to: '/admin/inquiries?status=New', highlight: data.inquiriesNew > 0 },
    { label: t('admin.dashboard.inquiriesMonth'), value: data.inquiriesThisMonth },
    { label: t('admin.dashboard.inquiriesTotal'), value: data.inquiriesTotal, to: '/admin/inquiries' },
  ]

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">{t('admin.nav.dashboard')}</h1>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {stats.map((s) => {
          const content = (
            <>
              <p className="text-sm text-zinc-500">{s.label}</p>
              <p className={`mt-2 text-3xl font-bold ${s.highlight ? 'text-red-600' : 'text-zinc-900'}`}>{s.value}</p>
            </>
          )
          return s.to ? (
            <Link key={s.label} to={s.to} className="card p-5 transition hover:shadow-md">{content}</Link>
          ) : (
            <div key={s.label} className="card p-5">{content}</div>
          )
        })}
      </div>
      <Link to="/admin/cars/new" className="btn-primary mt-8">{t('admin.cars.add')}</Link>
    </div>
  )
}
