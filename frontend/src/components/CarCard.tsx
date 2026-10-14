import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { CarListItem } from '@/api/types'
import { formatKm, formatPrice } from '@/lib/format'
import { FavoriteButton } from './FavoriteButton'
import { StatusBadge } from './Feedback'
import { CalendarIcon, CarIcon, FuelIcon, GaugeIcon, GearboxIcon } from './icons'

export function CarCard({ car }: { car: CarListItem }) {
  const { t } = useTranslation()
  const title = `${car.brand} ${car.model}`

  const specs = [
    { icon: CalendarIcon, text: String(car.year) },
    { icon: GaugeIcon, text: formatKm(car.mileageKm) },
    { icon: FuelIcon, text: t(`fuel.${car.fuelType}`) },
    { icon: GearboxIcon, text: t(`transmission.${car.transmission}`) },
  ]

  return (
    <Link
      to={`/cars/${car.id}`}
      className="card group flex flex-col overflow-hidden transition duration-200 hover:-translate-y-1 hover:shadow-card-hover"
    >
      <div className="relative aspect-4/3 overflow-hidden bg-zinc-100">
        {car.coverPhotoUrl ? (
          <img
            src={car.coverPhotoUrl}
            alt={title}
            loading="lazy"
            className="h-full w-full object-cover transition duration-500 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full flex-col items-center justify-center gap-1 text-zinc-400">
            <CarIcon width={48} height={48} />
            <span className="text-xs">{t('car.noPhoto')}</span>
          </div>
        )}
        <FavoriteButton carId={car.id} className="absolute top-3 right-3" />
        {car.status !== 'Available' && (
          <div className="absolute top-3 left-3">
            <StatusBadge status={car.status} />
          </div>
        )}
      </div>

      <div className="flex flex-1 flex-col p-4">
        <h3 className="text-lg font-semibold text-zinc-900 group-hover:text-brand-700">{title}</h3>
        <ul className="mt-3 grid grid-cols-2 gap-x-3 gap-y-2 text-sm text-zinc-600">
          {specs.map(({ icon: Icon, text }) => (
            <li key={text} className="flex items-center gap-1.5">
              <Icon width={15} height={15} className="shrink-0 text-zinc-400" />
              <span className="truncate">{text}</span>
            </li>
          ))}
        </ul>
        <p className="mt-4 border-t border-zinc-100 pt-3 font-display text-2xl font-bold text-brand-600">
          {formatPrice(car.price)}
        </p>
      </div>
    </Link>
  )
}

export function CarCardSkeleton() {
  return (
    <div className="card overflow-hidden" aria-hidden="true">
      <div className="aspect-4/3 animate-pulse bg-zinc-200" />
      <div className="space-y-3 p-4">
        <div className="h-5 w-2/3 animate-pulse rounded bg-zinc-200" />
        <div className="grid grid-cols-2 gap-2">
          {[0, 1, 2, 3].map((i) => <div key={i} className="h-4 animate-pulse rounded bg-zinc-100" />)}
        </div>
        <div className="h-7 w-1/3 animate-pulse rounded bg-zinc-200" />
      </div>
    </div>
  )
}
