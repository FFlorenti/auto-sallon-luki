import { useEffect, useState } from 'react'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { adminApi } from '@/api/endpoints'
import { CAR_STATUSES, type CarStatus } from '@/api/types'
import { useAuth } from '@/auth/useAuth'
import { ErrorState, Spinner, StatusBadge } from '@/components/Feedback'
import { Pagination } from '@/components/Pagination'
import { formatKm, formatPrice } from '@/lib/format'
import { useCarQueryParams } from '@/lib/useCarQueryParams'

export function AdminCarsPage() {
  const { t } = useTranslation()
  const { isAdmin } = useAuth()
  const queryClient = useQueryClient()
  const { query, update } = useCarQueryParams()
  const [search, setSearch] = useState(query.search ?? '')

  const { data, isPending, isError, refetch } = useQuery({
    queryKey: ['admin', 'cars', query],
    queryFn: () => adminApi.searchCars({ ...query, pageSize: 20 }),
    placeholderData: keepPreviousData,
  })

  useEffect(() => {
    if (search === (query.search ?? '')) return
    const timer = setTimeout(() => update({ search: search.trim() || undefined }), 400)
    return () => clearTimeout(timer)
  }, [search, query.search, update])

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['admin'] })
    void queryClient.invalidateQueries({ queryKey: ['cars'] })
    void queryClient.invalidateQueries({ queryKey: ['car'] })
  }

  const changeStatus = useMutation({
    mutationFn: ({ id, status }: { id: number; status: CarStatus }) => adminApi.changeCarStatus(id, status),
    onSuccess: invalidate,
  })

  const remove = useMutation({
    mutationFn: adminApi.deleteCar,
    onSuccess: invalidate,
  })

  const tabClass = (active: boolean) =>
    `rounded-lg px-3 py-1.5 text-sm font-medium ${active ? 'bg-brand-600 text-white' : 'bg-white text-zinc-700 hover:bg-zinc-100'}`

  return (
    <div>
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-bold">{t('admin.nav.cars')}</h1>
        <Link to="/admin/cars/new" className="btn-primary">{t('admin.cars.add')}</Link>
      </div>

      <div className="mb-4 flex flex-wrap items-center gap-2">
        <button type="button" className={tabClass(!query.status)} onClick={() => update({ status: undefined })}>{t('common.all')}</button>
        {CAR_STATUSES.map((s) => (
          <button key={s} type="button" className={tabClass(query.status === s)} onClick={() => update({ status: s })}>
            {t(`status.${s}`)}
          </button>
        ))}
        <input
          type="search"
          className="input ml-auto w-full sm:w-64"
          placeholder={t('filters.search')}
          aria-label={t('filters.search')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {isPending ? (
        <Spinner />
      ) : isError ? (
        <ErrorState onRetry={() => void refetch()} />
      ) : data.items.length === 0 ? (
        <p className="card p-8 text-center text-zinc-600">{t('admin.cars.empty')}</p>
      ) : (
        <>
          <div className="card overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-zinc-200 bg-zinc-50 text-xs text-zinc-500 uppercase">
                <tr>
                  <th className="p-3">{t('admin.cars.car')}</th>
                  <th className="p-3">{t('car.price')}</th>
                  <th className="p-3">{t('car.status')}</th>
                  <th className="p-3 text-right">{t('admin.cars.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-100">
                {data.items.map((car) => (
                  <tr key={car.id} className="hover:bg-zinc-50">
                    <td className="p-3">
                      <Link to={`/admin/cars/${car.id}`} className="flex items-center gap-3">
                        <div className="h-12 w-16 shrink-0 overflow-hidden rounded bg-zinc-100">
                          {car.coverPhotoUrl && <img src={car.coverPhotoUrl} alt="" className="h-full w-full object-cover" />}
                        </div>
                        <div>
                          <p className="font-medium text-zinc-900">{car.brand} {car.model}</p>
                          <p className="text-xs text-zinc-500">#{car.id} · {car.year} · {formatKm(car.mileageKm)}</p>
                        </div>
                      </Link>
                    </td>
                    <td className="p-3 font-medium whitespace-nowrap">{formatPrice(car.price)}</td>
                    <td className="p-3"><StatusBadge status={car.status} /></td>
                    <td className="p-3">
                      <div className="flex justify-end gap-2 whitespace-nowrap">
                        {car.status !== 'Sold' && (
                          <button type="button" className="btn-secondary btn-sm" disabled={changeStatus.isPending}
                            onClick={() => changeStatus.mutate({ id: car.id, status: 'Sold' })}>
                            {t('admin.cars.markSold')}
                          </button>
                        )}
                        {car.status !== 'Available' && (
                          <button type="button" className="btn-secondary btn-sm" disabled={changeStatus.isPending}
                            onClick={() => changeStatus.mutate({ id: car.id, status: 'Available' })}>
                            {t('admin.cars.markAvailable')}
                          </button>
                        )}
                        <Link to={`/admin/cars/${car.id}`} className="btn-secondary btn-sm">{t('common.edit')}</Link>
                        {isAdmin && (
                          <button type="button" className="btn-danger btn-sm" disabled={remove.isPending}
                            onClick={() => window.confirm(t('admin.cars.confirmDelete')) && remove.mutate(car.id)}>
                            {t('common.delete')}
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Pagination page={data.page} totalPages={data.totalPages} onPageChange={(page) => update({ page })} />
        </>
      )}
    </div>
  )
}
