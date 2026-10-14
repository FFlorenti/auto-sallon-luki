import { useMemo, useRef, useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { Turnstile, type TurnstileInstance } from '@marsidev/react-turnstile'
import { useMutation } from '@tanstack/react-query'
import { useForm, useWatch } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { ApiError } from '@/api/client'
import { publicApi } from '@/api/endpoints'
import type { InquiryType } from '@/api/types'
import { config } from '@/lib/config'

function makeSchema(t: (key: string) => string) {
  return z.object({
    type: z.enum(['General', 'TestDrive']),
    name: z.string().trim().min(1, t('inquiry.errors.name')).max(100),
    phone: z.string().trim().regex(/^\+?[0-9\s\-/()]{6,29}$/, t('inquiry.errors.phone')),
    email: z.union([z.literal(''), z.email(t('inquiry.errors.email')).max(150)]),
    message: z.string().trim().min(1, t('inquiry.errors.message')).max(2000),
    preferredDate: z.string(),
  })
}

type FormValues = z.infer<ReturnType<typeof makeSchema>>

const today = () => new Date().toISOString().slice(0, 10)

export function InquiryForm({ carId }: { carId?: number }) {
  const { t, i18n } = useTranslation()
  const schema = useMemo(() => makeSchema(t), [t])
  const turnstile = useRef<TurnstileInstance>(null)
  const [captchaToken, setCaptchaToken] = useState<string | null>(null)

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      type: 'General',
      name: '',
      phone: '',
      email: '',
      message: carId ? t('inquiry.defaultMessage') : '',
      preferredDate: '',
    },
  })
  const { register, handleSubmit, control, formState: { errors } } = form
  const type = useWatch({ control, name: 'type' })

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      publicApi.createInquiry({
        carId: carId ?? null,
        type: values.type,
        name: values.name,
        phone: values.phone,
        email: values.email || null,
        message: values.message,
        preferredDate: values.type === 'TestDrive' && values.preferredDate ? values.preferredDate : null,
        captchaToken: captchaToken ?? '',
      }),
    onSuccess: () => form.reset(),
    onError: (error) => {
      if (error instanceof ApiError) {
        for (const [field, messages] of Object.entries(error.fieldErrors)) {
          const name = (field.charAt(0).toLowerCase() + field.slice(1)) as keyof FormValues
          if (name in form.getValues()) form.setError(name, { message: messages[0] })
        }
      }
    },
    onSettled: () => {
      setCaptchaToken(null)
      turnstile.current?.reset()
    },
  })

  if (mutation.isSuccess) {
    return (
      <div className="rounded-xl bg-emerald-50 p-6 text-center text-emerald-800" role="status">
        <p className="font-medium">{t('inquiry.success')}</p>
      </div>
    )
  }

  const errorText =
    mutation.error instanceof ApiError && mutation.error.status === 429
      ? t('inquiry.tooMany')
      : mutation.error instanceof ApiError && Object.keys(mutation.error.fieldErrors).length === 0
        ? mutation.error.message
        : mutation.isError && !(mutation.error instanceof ApiError)
          ? t('common.error')
          : null

  const field = (name: keyof FormValues) => ({
    id: `inq-${name}`,
    'aria-invalid': errors[name] ? true : undefined,
    'aria-describedby': errors[name] ? `inq-${name}-error` : undefined,
    ...register(name),
  })

  const fieldError = (name: keyof FormValues) =>
    errors[name] && <p id={`inq-${name}-error`} className="field-error">{errors[name]?.message}</p>

  return (
    <form onSubmit={handleSubmit((v) => mutation.mutate(v))} noValidate className="space-y-4">
      {carId && (
        <fieldset className="grid grid-cols-2 gap-2">
          <legend className="sr-only">{t('inquiry.title')}</legend>
          {(['General', 'TestDrive'] as InquiryType[]).map((value) => (
            <label key={value} className={`btn cursor-pointer border ${type === value ? 'border-brand-600 bg-brand-50 text-brand-700' : 'border-zinc-300 bg-white'}`}>
              <input type="radio" value={value} {...register('type')} className="sr-only" />
              {t(value === 'General' ? 'inquiry.typeGeneral' : 'inquiry.typeTestDrive')}
            </label>
          ))}
        </fieldset>
      )}

      <div>
        <label htmlFor="inq-name" className="label">{t('inquiry.name')}</label>
        <input className="input" autoComplete="name" {...field('name')} />
        {fieldError('name')}
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="inq-phone" className="label">{t('inquiry.phone')}</label>
          <input className="input" type="tel" autoComplete="tel" {...field('phone')} />
          {fieldError('phone')}
        </div>
        <div>
          <label htmlFor="inq-email" className="label">
            {t('inquiry.email')} <span className="font-normal text-zinc-400">({t('common.optional')})</span>
          </label>
          <input className="input" type="email" autoComplete="email" {...field('email')} />
          {fieldError('email')}
        </div>
      </div>

      {type === 'TestDrive' && (
        <div>
          <label htmlFor="inq-preferredDate" className="label">{t('inquiry.preferredDate')}</label>
          <input className="input" type="date" min={today()} {...field('preferredDate')} />
          {fieldError('preferredDate')}
        </div>
      )}

      <div>
        <label htmlFor="inq-message" className="label">{t('inquiry.message')}</label>
        <textarea className="input min-h-28" rows={4} {...field('message')} />
        {fieldError('message')}
      </div>

      {config.turnstileSiteKey && (
        <Turnstile
          ref={turnstile}
          siteKey={config.turnstileSiteKey}
          options={{ language: i18n.language === 'sq' ? 'sq' : 'en', size: 'flexible' }}
          onSuccess={setCaptchaToken}
          onExpire={() => setCaptchaToken(null)}
          onError={() => setCaptchaToken(null)}
        />
      )}

      {errorText && <p className="rounded-lg bg-red-50 p-3 text-sm text-red-700" role="alert">{errorText}</p>}

      <button type="submit" className="btn-primary w-full py-3" disabled={mutation.isPending || !captchaToken}>
        {mutation.isPending ? t('inquiry.sending') : !captchaToken ? t('inquiry.captchaRequired') : t('inquiry.submit')}
      </button>
    </form>
  )
}
