import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router'
import { ApiError } from '@/api/client'
import { publicApi } from '@/api/endpoints'
import { ContactButtons } from '@/components/ContactButtons'
import { FavoriteButton } from '@/components/FavoriteButton'
import { ErrorState, Spinner, StatusBadge } from '@/components/Feedback'
import { CalendarIcon, ChevronIcon, FuelIcon, GaugeIcon, GearboxIcon } from '@/components/icons'
import { InquiryForm } from '@/components/InquiryForm'
import { PhotoGallery } from '@/components/PhotoGallery'
import { config } from '@/lib/config'
import { formatKm, formatPrice } from '@/lib/format'

export function CarDetailPage() {
  const { t, i18n } = useTranslation()
  const id = Number(useParams().id)

  const { data: car, isPending, error, refetch } = useQuery({
    queryKey: ['car', id],
    queryFn: () => publicApi.getCar(id),
    enabled: Number.isInteger(id) && id > 0,
    retry: (count, err) => !(err instanceof ApiError && err.status === 404) && count < 2,
  })

  const title = car ? `${car.brand} ${car.model} ${car.year}` : ''
  useEffect(() => {
    if (title) document.title = `${title} | ${config.dealerName}`
    return () => {
      document.title = config.dealerName
    }
  }, [title])

  if (!Number.isInteger(id) || (error instanceof ApiError && error.status === 404)) {
    return <ErrorState message={t('detail.notFound')} />
  }
  if (isPending) return <Spinner />
  if (error || !car) return <ErrorState onRetry={() => void refetch()} />

  const description = (i18n.language === 'en' ? car.descriptionEn || car.descriptionSq : car.descriptionSq || car.descriptionEn) ?? ''

  const specs: [string, string | null][] = [
    [t('car.year'), String(car.year)],
    [t('car.mileage'), formatKm(car.mileageKm)],
    [t('car.fuel'), t(`fuel.${car.fuelType}`)],
    [t('car.transmission'), t(`transmission.${car.transmission}`)],
    [t('car.body'), t(`body.${car.bodyType}`)],
    [t('car.engine'), car.engineSizeCc ? `${car.engineSizeCc} cm³` : null],
    [t('car.power'), car.powerHp ? `${car.powerHp} HP (${Math.round(car.powerHp * 0.7355)} kW)` : null],
    [t('car.color'), car.color],
    [t('car.doors'), car.doors ? String(car.doors) : null],
    [t('car.customs'), car.isCustomsCleared ? t('common.yes') : t('common.no')],
  ]

  const keySpecs = [
    { icon: CalendarIcon, label: t('car.year'), value: String(car.year) },
    { icon: GaugeIcon, label: t('car.mileage'), value: formatKm(car.mileageKm) },
    { icon: FuelIcon, label: t('car.fuel'), value: t(`fuel.${car.fuelType}`) },
    { icon: GearboxIcon, label: t('car.transmission'), value: t(`transmission.${car.transmission}`) },
  ]

  return (
    <div className="container-page py-6 sm:py-10">
      <nav className="mb-5 flex items-center gap-1.5 text-sm text-zinc-500" aria-label="Breadcrumb">
        <Link to="/" className="hover:text-brand-700">{t('detail.backToList')}</Link>
        <ChevronIcon width={14} height={14} />
        <span className="truncate text-zinc-800">{car.brand} {car.model}</span>
      </nav>

      {car.status === 'Sold' && (
        <p className="mb-5 rounded-xl bg-ink-950 p-4 text-sm font-medium text-white">{t('detail.soldNotice')}</p>
      )}

      <div className="grid gap-8 lg:grid-cols-[1fr_380px]">
        <div className="min-w-0 space-y-6">
          <PhotoGallery key={car.id} photos={car.photos} alt={title} />

          <ul className="grid grid-cols-2 gap-3 sm:grid-cols-4">
            {keySpecs.map(({ icon: Icon, label, value }) => (
              <li key={label} className="card flex flex-col items-center gap-1 p-4 text-center">
                <Icon width={22} height={22} className="text-brand-600" />
                <span className="text-xs text-zinc-500">{label}</span>
                <span className="font-semibold">{value}</span>
              </li>
            ))}
          </ul>

          <section className="card p-6">
            <h2 className="mb-4 text-xl font-semibold">{t('car.specs')}</h2>
            <dl className="grid grid-cols-1 gap-x-10 sm:grid-cols-2">
              {specs.filter(([, value]) => value).map(([label, value]) => (
                <div key={label} className="flex justify-between gap-4 border-b border-zinc-100 py-3 text-sm">
                  <dt className="text-zinc-500">{label}</dt>
                  <dd className="text-right font-medium">{value}</dd>
                </div>
              ))}
            </dl>
          </section>

          {description && (
            <section className="card p-6">
              <h2 className="mb-3 text-xl font-semibold">{t('car.description')}</h2>
              <p className="leading-relaxed whitespace-pre-line text-zinc-700">{description}</p>
            </section>
          )}
        </div>

        <aside className="space-y-6 lg:sticky lg:top-24 lg:self-start">
          <div className="card overflow-hidden">
            <div className="p-6">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h1 className="text-2xl font-bold">{car.brand} {car.model}</h1>
                  <p className="mt-1 text-sm text-zinc-500">{car.year} · {formatKm(car.mileageKm)} · {t(`fuel.${car.fuelType}`)}</p>
                </div>
                <FavoriteButton carId={car.id} className="border border-zinc-200" />
              </div>
              {car.status !== 'Available' && <div className="mt-3"><StatusBadge status={car.status} /></div>}
            </div>
            <div className="border-y border-zinc-100 bg-zinc-50 px-6 py-4">
              <p className="text-xs font-medium tracking-wider text-zinc-500 uppercase">{t('car.price')}</p>
              <p className="font-display text-4xl font-bold text-brand-600">{formatPrice(car.price)}</p>
            </div>
            <div className="p-6">
              <ContactButtons carTitle={title} />
            </div>
          </div>

          {car.status !== 'Sold' && (
            <div className="card p-6">
              <h2 className="mb-4 text-xl font-semibold">{t('inquiry.title')}</h2>
              <InquiryForm carId={car.id} />
            </div>
          )}
        </aside>
      </div>
    </div>
  )
}
