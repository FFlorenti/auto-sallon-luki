import { useCallback, useMemo } from 'react'
import { useSearchParams } from 'react-router'
import { BODY_TYPES, CAR_SORTS, CAR_STATUSES, FUEL_TYPES, TRANSMISSIONS, type CarQuery } from '@/api/types'

const NUMBER_KEYS = ['brandId', 'modelId', 'yearFrom', 'yearTo', 'priceFrom', 'priceTo', 'mileageMax', 'page'] as const

function pick<T extends string>(value: string | null, allowed: readonly T[]): T | undefined {
  return allowed.find((x) => x === value)
}

export function useCarQueryParams() {
  const [params, setParams] = useSearchParams()

  const query = useMemo<CarQuery>(() => {
    const q: CarQuery = {
      search: params.get('search') || undefined,
      fuelType: pick(params.get('fuelType'), FUEL_TYPES),
      transmission: pick(params.get('transmission'), TRANSMISSIONS),
      bodyType: pick(params.get('bodyType'), BODY_TYPES),
      status: pick(params.get('status'), CAR_STATUSES),
      sort: pick(params.get('sort'), CAR_SORTS),
    }
    for (const key of NUMBER_KEYS) {
      const n = Number(params.get(key))
      if (Number.isFinite(n) && n > 0) q[key] = n
    }
    return q
  }, [params])

  const update = useCallback(
    (changes: Partial<CarQuery>) => {
      setParams(
        (prev) => {
          const next = new URLSearchParams(prev)
          for (const [key, value] of Object.entries(changes)) {
            if (value === undefined || value === null || value === '') next.delete(key)
            else next.set(key, String(value))
          }
          if (!('page' in changes)) next.delete('page')
          return next
        },
        { replace: !('page' in changes) },
      )
    },
    [setParams],
  )

  const reset = useCallback(() => setParams(new URLSearchParams()), [setParams])

  return { query, update, reset }
}
