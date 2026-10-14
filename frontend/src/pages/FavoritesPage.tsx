import { useQueries } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { publicApi } from '@/api/endpoints'
import type { CarListItem } from '@/api/types'
import { CarCard } from '@/components/CarCard'
import { Spinner } from '@/components/Feedback'
import { useFavorites } from '@/lib/favorites'

export function FavoritesPage() {
  const { t } = useTranslation()
  const { ids } = useFavorites()

  const results = useQueries({
    queries: ids.map((id) => ({ queryKey: ['car', id], queryFn: () => publicApi.getCar(id), retry: false })),
  })

  const cars: CarListItem[] = results.flatMap((r) =>
    r.data
      ? [{ ...r.data, coverPhotoUrl: r.data.photos[0]?.thumbUrl ?? null }]
      : [],
  )
  const someMissing = results.some((r) => r.isError)

  return (
    <div className="container-page py-8">
      <h1 className="mb-6 text-2xl font-bold">{t('favorites.title')}</h1>

      {ids.length === 0 ? (
        <div className="card p-10 text-center text-zinc-600">
          <p>{t('favorites.empty')}</p>
          <Link to="/" className="btn-primary mt-4">{t('nav.cars')}</Link>
        </div>
      ) : results.some((r) => r.isPending) ? (
        <Spinner />
      ) : (
        <>
          {someMissing && <p className="mb-4 text-sm text-zinc-500">{t('favorites.unavailable')}</p>}
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {cars.map((car) => (
              <CarCard key={car.id} car={car} />
            ))}
          </div>
        </>
      )}
    </div>
  )
}
