import type { SVGProps } from 'react'

type IconProps = SVGProps<SVGSVGElement>

const base = { width: 20, height: 20, viewBox: '0 0 24 24', 'aria-hidden': true } as const

export function HeartIcon({ filled, ...props }: IconProps & { filled?: boolean }) {
  return (
    <svg {...base} fill={filled ? 'currentColor' : 'none'} stroke="currentColor" strokeWidth={2} {...props}>
      <path d="M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.7l-1-1.1a5.5 5.5 0 0 0-7.8 7.8l1 1.1L12 21l7.8-7.5 1-1.1a5.5 5.5 0 0 0 0-7.8z" />
    </svg>
  )
}

export function PhoneIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2} {...props}>
      <path d="M22 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.5 19.5 0 0 1-6-6A19.8 19.8 0 0 1 2.1 4.2 2 2 0 0 1 4.1 2h3a2 2 0 0 1 2 1.7c.1.9.4 1.8.7 2.7a2 2 0 0 1-.5 2.1L8 9.8a16 16 0 0 0 6 6l1.3-1.3a2 2 0 0 1 2.1-.4c.9.3 1.8.6 2.7.7a2 2 0 0 1 1.7 2z" />
    </svg>
  )
}

export function WhatsAppIcon(props: IconProps) {
  return (
    <svg {...base} fill="currentColor" {...props}>
      <path d="M17.5 14.4c-.3-.1-1.8-.9-2-1-.3-.1-.5-.1-.7.1-.2.3-.8 1-.9 1.2-.2.2-.3.2-.6.1-.3-.1-1.3-.5-2.4-1.5-.9-.8-1.5-1.8-1.7-2.1-.2-.3 0-.5.1-.6l.4-.5c.2-.2.2-.3.3-.5.1-.2 0-.4 0-.5l-.9-2.2c-.2-.6-.5-.5-.7-.5h-.6c-.2 0-.5.1-.8.4-.3.3-1 1-1 2.5s1.1 2.9 1.2 3.1c.1.2 2.1 3.2 5.1 4.5.7.3 1.3.5 1.7.6.7.2 1.4.2 1.9.1.6-.1 1.8-.7 2-1.4.2-.7.2-1.3.2-1.4-.1-.1-.3-.2-.6-.3zM12 21.8c-1.8 0-3.5-.5-5-1.4l-.4-.2-3.7 1 1-3.6-.2-.4A9.8 9.8 0 1 1 12 21.8zm8.4-18.2A11.8 11.8 0 0 0 1.7 17.8L0 24l6.3-1.7a11.8 11.8 0 0 0 5.7 1.4A11.8 11.8 0 0 0 20.4 3.6z" />
    </svg>
  )
}

export function ChevronIcon({ direction = 'right', ...props }: IconProps & { direction?: 'left' | 'right' }) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2} {...props}>
      <path d={direction === 'right' ? 'm9 18 6-6-6-6' : 'm15 18-6-6 6-6'} />
    </svg>
  )
}

export function CarIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={1.5} {...props}>
      <path d="M5 17h14M3 13l2-6a2 2 0 0 1 1.9-1.4h10.2A2 2 0 0 1 19 7l2 6v4a1 1 0 0 1-1 1h-1a2 2 0 0 1-4 0H9a2 2 0 0 1-4 0H4a1 1 0 0 1-1-1z" />
      <circle cx="7" cy="17" r="2" />
      <circle cx="17" cy="17" r="2" />
    </svg>
  )
}

export function MenuIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2} {...props}>
      <path d="M4 6h16M4 12h16M4 18h16" />
    </svg>
  )
}

export function CalendarIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2} {...props}>
      <rect x="3" y="4" width="18" height="18" rx="2" />
      <path d="M16 2v4M8 2v4M3 10h18" />
    </svg>
  )
}

export function GaugeIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2} {...props}>
      <path d="M12 14l4-4M3.3 19a10 10 0 1 1 17.4 0" />
    </svg>
  )
}

export function FuelIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2} {...props}>
      <path d="M3 22h12M4 9h10M14 22V4a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v18M14 13h2a2 2 0 0 1 2 2v2a2 2 0 0 0 4 0V9.8a2 2 0 0 0-.6-1.4L18 5" />
    </svg>
  )
}

export function GearboxIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2} {...props}>
      <circle cx="5" cy="6" r="2" /><circle cx="12" cy="6" r="2" /><circle cx="19" cy="6" r="2" />
      <circle cx="5" cy="18" r="2" /><circle cx="12" cy="18" r="2" />
      <path d="M5 8v8M12 8v8M19 8v4H5" />
    </svg>
  )
}

export function CheckIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2.5} {...props}>
      <path d="M20 6 9 17l-5-5" />
    </svg>
  )
}

export function CloseIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2} {...props}>
      <path d="M18 6 6 18M6 6l12 12" />
    </svg>
  )
}

export function MapPinIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2} {...props}>
      <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z" />
      <circle cx="12" cy="10" r="3" />
    </svg>
  )
}

export function SearchIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2} {...props}>
      <circle cx="11" cy="11" r="8" />
      <path d="m21 21-4.3-4.3" />
    </svg>
  )
}

export function ExpandIcon(props: IconProps) {
  return (
    <svg {...base} fill="none" stroke="currentColor" strokeWidth={2} {...props}>
      <path d="M15 3h6v6M9 21H3v-6M21 3l-7 7M3 21l7-7" />
    </svg>
  )
}
