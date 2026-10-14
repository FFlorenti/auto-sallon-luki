import i18n from '@/i18n'

function locale() {
  return i18n.language === 'sq' ? 'sq-AL' : 'en-GB'
}

export function formatPrice(value: number) {
  return new Intl.NumberFormat(locale(), { style: 'currency', currency: 'EUR', maximumFractionDigits: 0 }).format(value)
}

export function formatKm(value: number) {
  return `${new Intl.NumberFormat(locale()).format(value)} km`
}

export function formatDate(value: string) {
  return new Intl.DateTimeFormat(locale(), { dateStyle: 'medium' }).format(new Date(value))
}

export function formatDateTime(value: string) {
  return new Intl.DateTimeFormat(locale(), { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value))
}
