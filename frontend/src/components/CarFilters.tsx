import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useBrands } from '@/lib/useBrands'
import { BODY_TYPES, FUEL_TYPES, TRANSMISSIONS, type CarQuery } from '@/api/types'

interface Props {
  query: CarQuery
  onChange: (changes: Partial<CarQuery>) => void
  onReset: () => void
}

const currentYear = new Date().getFullYear()
const YEARS = Array.from({ length: currentYear + 2 - 1990 }, (_, i) => currentYear + 1 - i)

function DebouncedNumber({ value, onCommit, ...props }: {
  value: number | undefined
  onCommit: (v: number | undefined) => void
  id: string
  placeholder?: string
}) {
  const [text, setText] = useState(value?.toString() ?? '')
  const [prevValue, setPrevValue] = useState(value)
  if (value !== prevValue) {
    setPrevValue(value)
    setText(value?.toString() ?? '')
  }
  useEffect(() => {
    const n = text === '' ? undefined : Number(text)
    if (n === value || (n !== undefined && !Number.isFinite(n))) return
    const timer = setTimeout(() => onCommit(n), 500)
    return () => clearTimeout(timer)
  }, [text, value, onCommit])

  return <input type="number" min={0} inputMode="numeric" className="input" value={text} onChange={(e) => setText(e.target.value)} {...props} />
}

export function CarFilters({ query, onChange, onReset }: Props) {
  const { t } = useTranslation()
  const { data: brands } = useBrands()
  const models = brands?.find((b) => b.id === query.brandId)?.models ?? []

  const select = (key: keyof CarQuery, label: string, options: { value: string | number; label: string }[], anyLabel: string) => (
    <div>
      <label htmlFor={`f-${key}`} className="label">{label}</label>
      <select
        id={`f-${key}`}
        className="input"
        value={query[key]?.toString() ?? ''}
        onChange={(e) => {
          const raw = e.target.value
          const value = raw === '' ? undefined : typeof options[0]?.value === 'number' ? Number(raw) : raw
          onChange(key === 'brandId' ? { brandId: value as number | undefined, modelId: undefined } : { [key]: value })
        }}
      >
        <option value="">{anyLabel}</option>
        {options.map((o) => (
          <option key={o.value} value={o.value}>{o.label}</option>
        ))}
      </select>
    </div>
  )

  return (
    <div className="space-y-4">
      {select('brandId', t('car.brand'), brands?.map((b) => ({ value: b.id, label: b.name })) ?? [], t('filters.anyBrand'))}
      {select('modelId', t('car.model'), models.map((m) => ({ value: m.id, label: m.name })), t('filters.anyModel'))}

      <div className="grid grid-cols-2 gap-3">
        {select('yearFrom', t('filters.yearFrom'), YEARS.map((y) => ({ value: y, label: String(y) })), t('common.any'))}
        {select('yearTo', t('filters.yearTo'), YEARS.map((y) => ({ value: y, label: String(y) })), t('common.any'))}
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <label htmlFor="f-priceFrom" className="label">{t('filters.priceFrom')}</label>
          <DebouncedNumber id="f-priceFrom" value={query.priceFrom} onCommit={(v) => onChange({ priceFrom: v })} />
        </div>
        <div>
          <label htmlFor="f-priceTo" className="label">{t('filters.priceTo')}</label>
          <DebouncedNumber id="f-priceTo" value={query.priceTo} onCommit={(v) => onChange({ priceTo: v })} />
        </div>
      </div>

      <div>
        <label htmlFor="f-mileageMax" className="label">{t('filters.mileageMax')}</label>
        <DebouncedNumber id="f-mileageMax" value={query.mileageMax} onCommit={(v) => onChange({ mileageMax: v })} />
      </div>

      {select('fuelType', t('car.fuel'), FUEL_TYPES.map((f) => ({ value: f, label: t(`fuel.${f}`) })), t('common.all'))}
      {select('transmission', t('car.transmission'), TRANSMISSIONS.map((x) => ({ value: x, label: t(`transmission.${x}`) })), t('common.all'))}
      {select('bodyType', t('car.body'), BODY_TYPES.map((b) => ({ value: b, label: t(`body.${b}`) })), t('common.all'))}

      <button type="button" className="btn-secondary w-full" onClick={onReset}>
        {t('filters.reset')}
      </button>
    </div>
  )
}
