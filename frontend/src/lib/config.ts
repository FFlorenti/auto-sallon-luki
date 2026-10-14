export const config = {
  dealerName: import.meta.env.VITE_DEALER_NAME ?? 'Auto Sallon Luki',
  dealerPhone: import.meta.env.VITE_DEALER_PHONE ?? '',
  dealerWhatsApp: import.meta.env.VITE_DEALER_WHATSAPP ?? '',
  dealerAddress: import.meta.env.VITE_DEALER_ADDRESS ?? '',
  turnstileSiteKey: import.meta.env.VITE_TURNSTILE_SITE_KEY ?? '',
}

export function phoneHref(phone: string) {
  return `tel:${phone.replace(/[^\d+]/g, '')}`
}

export function whatsAppHref(message = '') {
  const base = `https://wa.me/${config.dealerWhatsApp}`
  return message ? `${base}?text=${encodeURIComponent(message)}` : base
}
