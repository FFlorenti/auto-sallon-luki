import { useTranslation } from 'react-i18next'
import { ContactButtons } from '@/components/ContactButtons'
import { InquiryForm } from '@/components/InquiryForm'
import { config } from '@/lib/config'

export function ContactPage() {
  const { t } = useTranslation()

  return (
    <div className="container-page py-8">
      <h1 className="text-2xl font-bold">{t('contact.title')}</h1>
      <p className="mt-2 max-w-2xl text-zinc-600">{t('contact.intro')}</p>

      <div className="mt-8 grid gap-8 lg:grid-cols-2">
        <div className="card space-y-4 p-6">
          <div>
            <h2 className="text-sm font-medium text-zinc-500">{t('contact.address')}</h2>
            <p className="mt-1 text-lg">{config.dealerAddress}</p>
          </div>
          <div>
            <h2 className="text-sm font-medium text-zinc-500">{t('contact.phone')}</h2>
            <p className="mt-1 text-lg">{config.dealerPhone}</p>
          </div>
          <ContactButtons />
        </div>

        <div className="card p-6">
          <h2 className="mb-4 text-lg font-semibold">{t('inquiry.generalTitle')}</h2>
          <InquiryForm />
        </div>
      </div>
    </div>
  )
}
