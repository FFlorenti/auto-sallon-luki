import { useEffect, useRef, useState } from 'react'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { publicApi } from '@/api/endpoints'
import { CAR_SORTS, type CarQuery, type CarSort } from '@/api/types'
import { CarCard, CarCardSkeleton } from '@/components/CarCard'
import { CarFilters } from '@/components/CarFilters'
import { ErrorState } from '@/components/Feedback'
import { CheckIcon, CloseIcon, SearchIcon } from '@/components/icons'
import { Pagination } from '@/components/Pagination'
import { formatKm, formatPrice } from '@/lib/format'
import { useBrands } from '@/lib/useBrands'
import { useCarQueryParams } from '@/lib/useCarQueryParams'

function ActiveFilters({ query, onRemove }: { query: CarQuery; onRemove: (changes: Partial<CarQuery>) => void }) {
  const { t } = useTranslation()
  const { data: brands } = useBrands()
  const brand = brands?.find((b) => b.id === query.brandId)

  const chips: { label: string; remove: Partial<CarQuery> }[] = []
  if (query.search) chips.push({ label: `"${query.search}"`, remove: { search: undefined } })
  if (brand) chips.push({ label: brand.name, remove: { brandId: undefined, modelId: undefined } })
  const model = brand?.models.find((m) => m.id === query.modelId)
  if (model) chips.push({ label: model.name, remove: { modelId: undefined } })
  if (query.yearFrom) chips.push({ label: `${t('car.year')} ≥ ${query.yearFrom}`, remove: { yearFrom: undefined } })
  if (query.yearTo) chips.push({ label: `${t('car.year')} ≤ ${query.yearTo}`, remove: { yearTo: undefined } })
  if (query.priceFrom) chips.push({ label: `≥ ${formatPrice(query.priceFrom)}`, remove: { priceFrom: undefined } })
  if (query.priceTo) chips.push({ label: `≤ ${formatPrice(query.priceTo)}`, remove: { priceTo: undefined } })
  if (query.mileageMax) chips.push({ label: `≤ ${formatKm(query.mileageMax)}`, remove: { mileageMax: undefined } })
  if (query.fuelType) chips.push({ label: t(`fuel.${query.fuelType}`), remove: { fuelType: undefined } })
  if (query.transmission) chips.push({ label: t(`transmission.${query.transmission}`), remove: { transmission: undefined } })
  if (query.bodyType) chips.push({ label: t(`body.${query.bodyType}`), remove: { bodyType: undefined } })

  if (chips.length === 0) return null

  return (
    <ul className="mb-5 flex flex-wrap gap-2">
      {chips.map((chip) => (
        <li key={chip.label}>
          <button
            type="button"
            onClick={() => onRemove(chip.remove)}
            className="inline-flex items-center gap-1.5 rounded-full border border-brand-100 bg-brand-50 py-1 pr-2 pl-3 text-sm font-medium text-brand-700 transition hover:border-brand-500"
            aria-label={`${t('filters.removeFilter')}: ${chip.label}`}
          >
            {chip.label}
            <CloseIcon width={14} height={14} />
          </button>
        </li>
      ))}
    </ul>
  )
}

export function CarListPage() {
  const { t } = useTranslation()
  const { query, update, reset } = useCarQueryParams()
  const [filtersOpen, setFiltersOpen] = useState(false)
  const resultsRef = useRef<HTMLDivElement>(null)
  const [search, setSearch] = useState(query.search ?? '')
  const [prevUrlSearch, setPrevUrlSearch] = useState(query.search)
  if (query.search !== prevUrlSearch) {
    setPrevUrlSearch(query.search)
    setSearch(query.search ?? '')
  }

  const { data, isPending, isError, refetch, isPlaceholderData } = useQuery({
    queryKey: ['cars', query],
    queryFn: () => publicApi.searchCars(query),
    placeholderData: keepPreviousData,
  })

  const { data: stock } = useQuery({
    queryKey: ['cars', 'stock'],
    queryFn: () => publicApi.searchCars({ pageSize: 1 }),
  })

  useEffect(() => {
    if (search === (query.search ?? '')) return
    const timer = setTimeout(() => update({ search: search.trim() || undefined }), 400)
    return () => clearTimeout(timer)
  }, [search, query.search, update])

  const scrollToResults = () => resultsRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })

  const changePage = (page: number) => {
    update({ page })
    scrollToResults()
  }

  const trust = [
    stock ? t('listing.trustStock', { count: stock.totalCount }) : null,
    t('listing.trustContact'),
    t('listing.trustTestDrive'),
  ].filter(Boolean)

  return (
    <>
      <section className="relative overflow-hidden bg-ink-950 text-white">
        <div className="pointer-events-none absolute -top-40 right-0 h-96 w-96 rounded-full bg-brand-600/25 blur-3xl" />
        <div className="pointer-events-none absolute -bottom-48 -left-20 h-80 w-80 rounded-full bg-brand-600/10 blur-3xl" />

        <div className="relative container-page py-12 sm:py-20">
          <h1 className="max-w-3xl text-4xl font-bold sm:text-6xl" style={{ fontStretch: '110%' }}>
            {t('listing.heroTitle')}
          </h1>
          <p className="mt-4 max-w-2xl text-base text-zinc-300 sm:text-lg">{t('listing.heroText')}</p>

          <form
            className="mt-8 flex max-w-2xl gap-2 rounded-2xl bg-white p-2 shadow-2xl shadow-black/40"
            role="search"
            onSubmit={(e) => {
              e.preventDefault()
              update({ search: search.trim() || undefined })
              scrollToResults()
            }}
          >
            <div className="relative flex-1">
              <SearchIcon className="pointer-events-none absolute top-1/2 left-3 -translate-y-1/2 text-zinc-400" />
              <input
                type="search"
                className="h-12 w-full rounded-xl pr-3 pl-10 text-base text-zinc-900 placeholder:text-zinc-400 focus:outline-none"
                placeholder={t('filters.search')}
                aria-label={t('filters.search')}
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
            <button type="submit" className="btn-primary h-12 px-6 text-base">
              {t('listing.searchButton')}
            </button>
          </form>

          <ul className="mt-6 flex flex-wrap gap-x-6 gap-y-2 text-sm text-zinc-300">
            {trust.map((item) => (
              <li key={item} className="flex items-center gap-2">
                <span className="rounded-full bg-brand-600 p-0.5 text-white"><CheckIcon width={12} height={12} /></span>
                {item}
              </li>
            ))}
          </ul>
        </div>
      </section>

      <div ref={resultsRef} className="container-page scroll-mt-20 py-8 lg:grid lg:grid-cols-[280px_1fr] lg:gap-8">
        <aside
          className={`${filtersOpen ? 'fixed inset-0 z-40 overflow-y-auto bg-white p-5' : 'hidden'} lg:static lg:block lg:bg-transparent lg:p-0`}
          aria-label={t('filters.title')}
        >
          <div className="lg:card lg:sticky lg:top-24 lg:max-h-[calc(100vh-7.5rem)] lg:overflow-y-auto lg:p-5">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="text-lg font-semibold">{t('filters.title')}</h2>
              <button type="button" className="rounded-lg p-1.5 text-zinc-500 hover:bg-zinc-100 lg:hidden" onClick={() => setFiltersOpen(false)} aria-label={t('filters.close')}>
                <CloseIcon />
              </button>
            </div>
            <CarFilters query={query} onChange={update} onReset={reset} />
            <button type="button" className="btn-primary mt-4 w-full lg:hidden" onClick={() => setFiltersOpen(false)}>
              {t('filters.close')}
            </button>
          </div>
        </aside>

        <section aria-live="polite">
          <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
            <p className="font-display text-xl font-semibold">
              {data ? t('listing.results', { count: data.totalCount }) : ' '}
            </p>
            <div className="flex items-center gap-2">
              <button type="button" className="btn-secondary lg:hidden" onClick={() => setFiltersOpen(true)}>
                {t('filters.show')}
              </button>
              <label htmlFor="sort" className="sr-only">{t('filters.sort')}</label>
              <select
                id="sort"
                className="input w-auto"
                value={query.sort ?? 'Newest'}
                onChange={(e) => update({ sort: e.target.value === 'Newest' ? undefined : (e.target.value as CarSort) })}
              >
                {CAR_SORTS.map((s) => (
                  <option key={s} value={s}>{t(`sort.${s}`)}</option>
                ))}
              </select>
            </div>
          </div>

          <ActiveFilters query={query} onRemove={update} />

          {isPending ? (
            <div className="grid gap-6 sm:grid-cols-2 xl:grid-cols-3">
              {Array.from({ length: 6 }, (_, i) => <CarCardSkeleton key={i} />)}
            </div>
          ) : isError ? (
            <ErrorState onRetry={() => void refetch()} />
          ) : data.items.length === 0 ? (
            <div className="card p-12 text-center text-zinc-600">
              <SearchIcon width={40} height={40} className="mx-auto text-zinc-300" />
              <p className="mt-3">{t('listing.empty')}</p>
              <button type="button" className="btn-secondary mt-5" onClick={reset}>{t('filters.reset')}</button>
            </div>
          ) : (
            <>
              <div className={`grid gap-6 transition-opacity sm:grid-cols-2 xl:grid-cols-3 ${isPlaceholderData ? 'opacity-60' : ''}`}>
                {data.items.map((car) => (
                  <CarCard key={car.id} car={car} />
                ))}
              </div>
              <Pagination page={data.page} totalPages={data.totalPages} onPageChange={changePage} />
            </>
          )}
        </section>
      </div>
    </>
  )
}
