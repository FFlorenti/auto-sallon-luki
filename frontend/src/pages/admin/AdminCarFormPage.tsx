import { useEffect, useMemo, useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link, useLocation, useNavigate, useParams } from 'react-router'
import { z } from 'zod'
import { ApiError } from '@/api/client'
import { adminApi } from '@/api/endpoints'
import { BODY_TYPES, FUEL_TYPES, TRANSMISSIONS, type AdminCarDetail, type SaveCarRequest } from '@/api/types'
import { ErrorState, Spinner, StatusBadge } from '@/components/Feedback'
import { useBrands } from '@/lib/useBrands'
import { PhotoManager } from './PhotoManager'

const maxYear = new Date().getFullYear() + 1

const requiredInt = (min: number, max: number) =>
  z.string().trim().min(1).refine((v) => Number.isInteger(Number(v)) && Number(v) >= min && Number(v) <= max)
const optionalInt = (min: number, max: number) =>
  z.string().trim().refine((v) => v === '' || (Number.isInteger(Number(v)) && Number(v) >= min && Number(v) <= max))

const schema = z.object({
  brandId: z.string().min(1),
  modelId: z.string().min(1),
  year: requiredInt(1950, maxYear),
  price: z.string().trim().min(1).refine((v) => Number(v) > 0 && Number(v) < 10_000_000),
  mileageKm: requiredInt(0, 2_000_000),
  fuelType: z.enum(FUEL_TYPES),
  transmission: z.enum(TRANSMISSIONS),
  bodyType: z.enum(BODY_TYPES),
  engineSizeCc: optionalInt(50, 10_000),
  powerHp: optionalInt(1, 2_000),
  color: z.string().max(30),
  doors: optionalInt(1, 6),
  vin: z.string().trim().refine((v) => v === '' || /^[A-HJ-NPR-Za-hj-npr-z0-9]{17}$/.test(v)),
  isCustomsCleared: z.boolean(),
  descriptionSq: z.string().max(4000),
  descriptionEn: z.string().max(4000),
})

type FormValues = z.infer<typeof schema>

const emptyForm: FormValues = {
  brandId: '', modelId: '', year: '', price: '', mileageKm: '',
  fuelType: 'Diesel', transmission: 'Manual', bodyType: 'Sedan',
  engineSizeCc: '', powerHp: '', color: '', doors: '', vin: '',
  isCustomsCleared: true, descriptionSq: '', descriptionEn: '',
}

function toForm({ car, vin }: AdminCarDetail): FormValues {
  const s = (v: number | string | null) => (v === null ? '' : String(v))
  return {
    brandId: String(car.brandId), modelId: String(car.modelId), year: String(car.year),
    price: String(car.price), mileageKm: String(car.mileageKm),
    fuelType: car.fuelType, transmission: car.transmission, bodyType: car.bodyType,
    engineSizeCc: s(car.engineSizeCc), powerHp: s(car.powerHp), color: s(car.color), doors: s(car.doors),
    vin: s(vin), isCustomsCleared: car.isCustomsCleared,
    descriptionSq: s(car.descriptionSq), descriptionEn: s(car.descriptionEn),
  }
}

function toRequest(v: FormValues): SaveCarRequest {
  const num = (s: string) => (s.trim() === '' ? null : Number(s))
  const str = (s: string) => (s.trim() === '' ? null : s.trim())
  return {
    modelId: Number(v.modelId), year: Number(v.year), price: Number(v.price), mileageKm: Number(v.mileageKm),
    fuelType: v.fuelType, transmission: v.transmission, bodyType: v.bodyType,
    engineSizeCc: num(v.engineSizeCc), powerHp: num(v.powerHp), color: str(v.color), doors: num(v.doors),
    vin: str(v.vin), isCustomsCleared: v.isCustomsCleared,
    descriptionSq: str(v.descriptionSq), descriptionEn: str(v.descriptionEn),
  }
}

export function AdminCarFormPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const params = useParams()
  const created = (useLocation().state as { created?: boolean } | null)?.created
  const id = params.id ? Number(params.id) : null
  const isEdit = id !== null
  const [notice, setNotice] = useState<string | null>(null)

  const brands = useBrands()
  const car = useQuery({
    queryKey: ['admin', 'car', id],
    queryFn: () => adminApi.getCar(id!),
    enabled: isEdit,
  })

  const form = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: emptyForm })
  const { register, handleSubmit, control, setValue, reset, setError, formState: { errors, isDirty } } = form

  useEffect(() => {
    if (car.data) reset(toForm(car.data))
  }, [car.data, reset])

  const brandId = Number(useWatch({ control, name: 'brandId' }))
  const models = useMemo(() => brands.data?.find((b) => b.id === brandId)?.models ?? [], [brands.data, brandId])

  const save = useMutation({
    mutationFn: async (values: FormValues) => {
      const request = toRequest(values)
      if (isEdit) {
        await adminApi.updateCar(id, request)
        return id
      }
      return (await adminApi.createCar(request)).id
    },
    onSuccess: (savedId) => {
      void queryClient.invalidateQueries({ queryKey: ['admin'] })
      void queryClient.invalidateQueries({ queryKey: ['cars'] })
      void queryClient.invalidateQueries({ queryKey: ['car', savedId] })
      if (isEdit) {
        setNotice(t('admin.form.saved'))
      } else {
        void navigate(`/admin/cars/${savedId}`, { replace: true, state: { created: true } })
      }
    },
    onError: (error) => {
      if (error instanceof ApiError) {
        for (const [field, messages] of Object.entries(error.fieldErrors)) {
          const name = (field.charAt(0).toLowerCase() + field.slice(1)) as keyof FormValues
          if (name in emptyForm) setError(name, { message: messages[0] })
        }
      }
    },
  })

  const addLookup = useMutation({
    mutationFn: async (kind: 'brand' | 'model') => {
      const name = window.prompt(t(kind === 'brand' ? 'admin.form.newBrand' : 'admin.form.newModel'))?.trim()
      if (!name) return
      if (kind === 'brand') {
        const brand = await adminApi.createBrand(name)
        await queryClient.invalidateQueries({ queryKey: ['brands'] })
        setValue('brandId', String(brand.id), { shouldDirty: true })
        setValue('modelId', '', { shouldDirty: true })
      } else {
        const model = await adminApi.createModel(brandId, name)
        await queryClient.invalidateQueries({ queryKey: ['brands'] })
        setValue('modelId', String(model.id), { shouldDirty: true, shouldValidate: true })
      }
    },
  })

  if (isEdit && car.isPending) return <Spinner />
  if (isEdit && car.isError) return <ErrorState onRetry={() => void car.refetch()} />

  const fieldError = (name: keyof FormValues) =>
    errors[name] && <p className="field-error">{errors[name]?.message || t('admin.form.invalid')}</p>
  const input = (name: keyof FormValues, label: string, props: React.InputHTMLAttributes<HTMLInputElement> = {}) => (
    <div>
      <label htmlFor={`car-${name}`} className="label">{label}</label>
      <input id={`car-${name}`} className="input" aria-invalid={errors[name] ? true : undefined} {...props} {...register(name)} />
      {fieldError(name)}
    </div>
  )

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center gap-3">
        <Link to="/admin/cars" className="text-sm text-zinc-500 hover:text-brand-700">← {t('admin.nav.cars')}</Link>
        <h1 className="w-full text-2xl font-bold">
          {isEdit ? t('admin.form.editTitle') : t('admin.form.newTitle')}
          {car.data && <span className="ml-3 align-middle"><StatusBadge status={car.data.car.status} /></span>}
        </h1>
        {isEdit && car.data && (
          <Link to={`/cars/${id}`} target="_blank" className="text-sm text-brand-700 hover:underline">
            {t('admin.nav.website')} ↗
          </Link>
        )}
      </div>

      {created && !notice && <p className="rounded-lg bg-emerald-50 p-3 text-sm text-emerald-800">{t('admin.form.created')}</p>}
      {notice && !isDirty && <p className="rounded-lg bg-emerald-50 p-3 text-sm text-emerald-800" role="status">{notice}</p>}

      <form onSubmit={handleSubmit((v) => save.mutate(v))} noValidate className="card space-y-6 p-5">
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label htmlFor="car-brandId" className="label">{t('car.brand')}</label>
            <div className="flex gap-2">
              <Controller control={control} name="brandId" render={({ field }) => (
                <select id="car-brandId" className="input" aria-invalid={errors.brandId ? true : undefined}
                  {...field} onChange={(e) => { field.onChange(e); setValue('modelId', '') }}>
                  <option value="">{t('admin.form.selectBrand')}</option>
                  {brands.data?.map((b) => <option key={b.id} value={b.id}>{b.name}</option>)}
                </select>
              )} />
              <button type="button" className="btn-secondary px-3" onClick={() => addLookup.mutate('brand')} title={t('admin.form.newBrand')}>+</button>
            </div>
            {errors.brandId && <p className="field-error">{t('admin.form.required')}</p>}
          </div>
          <div>
            <label htmlFor="car-modelId" className="label">{t('car.model')}</label>
            <div className="flex gap-2">
              <Controller control={control} name="modelId" render={({ field }) => (
                <select id="car-modelId" className="input" disabled={!brandId} aria-invalid={errors.modelId ? true : undefined} {...field}>
                  <option value="">{t('admin.form.selectModel')}</option>
                  {models.map((m) => <option key={m.id} value={m.id}>{m.name}</option>)}
                </select>
              )} />
              <button type="button" className="btn-secondary px-3" disabled={!brandId} onClick={() => addLookup.mutate('model')} title={t('admin.form.newModel')}>+</button>
            </div>
            {errors.modelId && <p className="field-error">{t('admin.form.required')}</p>}
          </div>
        </div>
        {addLookup.error instanceof ApiError && <p className="field-error">{addLookup.error.message}</p>}

        <div className="grid gap-4 sm:grid-cols-3">
          {input('year', t('car.year'), { type: 'number', inputMode: 'numeric', min: 1950, max: maxYear })}
          {input('price', `${t('car.price')} (€)`, { type: 'number', inputMode: 'decimal', min: 1, step: 'any' })}
          {input('mileageKm', `${t('car.mileage')} (km)`, { type: 'number', inputMode: 'numeric', min: 0 })}
        </div>

        <div className="grid gap-4 sm:grid-cols-3">
          {([
            ['fuelType', t('car.fuel'), FUEL_TYPES, 'fuel'],
            ['transmission', t('car.transmission'), TRANSMISSIONS, 'transmission'],
            ['bodyType', t('car.body'), BODY_TYPES, 'body'],
          ] as const).map(([name, label, options, prefix]) => (
            <div key={name}>
              <label htmlFor={`car-${name}`} className="label">{label}</label>
              <select id={`car-${name}`} className="input" {...register(name)}>
                {options.map((o) => <option key={o} value={o}>{t(`${prefix}.${o}`)}</option>)}
              </select>
            </div>
          ))}
        </div>

        <div className="grid gap-4 sm:grid-cols-4">
          {input('engineSizeCc', `${t('car.engine')} (cm³)`, { type: 'number', inputMode: 'numeric' })}
          {input('powerHp', `${t('car.power')} (HP)`, { type: 'number', inputMode: 'numeric' })}
          {input('color', t('car.color'))}
          {input('doors', t('car.doors'), { type: 'number', inputMode: 'numeric', min: 1, max: 6 })}
        </div>

        <div className="grid items-end gap-4 sm:grid-cols-2">
          {input('vin', t('admin.form.vin'), { maxLength: 17, className: 'input font-mono uppercase' })}
          <label className="flex items-center gap-2 pb-2 text-sm font-medium">
            <input type="checkbox" className="h-4 w-4 rounded" {...register('isCustomsCleared')} />
            {t('car.customs')}
          </label>
        </div>

        <div className="grid gap-4 lg:grid-cols-2">
          {(['descriptionSq', 'descriptionEn'] as const).map((name) => (
            <div key={name}>
              <label htmlFor={`car-${name}`} className="label">{t(`admin.form.${name}`)}</label>
              <textarea id={`car-${name}`} rows={6} className="input" {...register(name)} />
              {fieldError(name)}
            </div>
          ))}
        </div>

        {save.isError && !(save.error instanceof ApiError && Object.keys(save.error.fieldErrors).length > 0) && (
          <p className="rounded-lg bg-red-50 p-3 text-sm text-red-700" role="alert">
            {save.error instanceof ApiError ? save.error.message : t('common.error')}
          </p>
        )}

        <div className="flex gap-3">
          <button type="submit" className="btn-primary" disabled={save.isPending}>
            {save.isPending ? t('common.saving') : t('common.save')}
          </button>
          <Link to="/admin/cars" className="btn-secondary">{t('common.cancel')}</Link>
        </div>
      </form>

      {isEdit && car.data ? (
        <PhotoManager carId={id} photos={car.data.car.photos} />
      ) : (
        <p className="card p-5 text-sm text-zinc-500">{t('admin.photos.saveFirst')}</p>
      )}
    </div>
  )
}
