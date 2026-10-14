import { useTranslation } from 'react-i18next'
import { config, phoneHref, whatsAppHref } from '@/lib/config'
import { PhoneIcon, WhatsAppIcon } from './icons'

export function ContactButtons({ carTitle }: { carTitle?: string }) {
  const { t } = useTranslation()
  const message = carTitle
    ? t('detail.whatsappMessage', { car: carTitle, url: window.location.href })
    : ''

  return (
    <div className="grid grid-cols-2 gap-3">
      {config.dealerPhone && (
        <a href={phoneHref(config.dealerPhone)} className="btn-primary py-3">
          <PhoneIcon /> {t('detail.call')}
        </a>
      )}
      {config.dealerWhatsApp && (
        <a href={whatsAppHref(message)} target="_blank" rel="noopener noreferrer" className="btn-whatsapp py-3">
          <WhatsAppIcon /> {t('detail.whatsapp')}
        </a>
      )}
    </div>
  )
}
