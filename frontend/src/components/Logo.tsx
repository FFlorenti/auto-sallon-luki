export function Logo({ variant = 'onDark', className = '' }: { variant?: 'onDark' | 'onLight'; className?: string }) {
  const textColor = variant === 'onDark' ? '#ffffff' : '#0c0c0e'

  return (
    <svg viewBox="0 0 200 94" role="img" aria-label="Auto Sallon Luki" className={className}>
      <path
        d="M10 38 C 30 34, 46 31, 62 22 C 76 13, 92 8, 112 8 C 136 8, 152 16, 166 27 C 174 32, 184 34, 192 36"
        fill="none"
        stroke="var(--color-brand-600)"
        strokeWidth="5"
        strokeLinecap="round"
      />
      <text
        x="100" y="58" textAnchor="middle" textLength="168" lengthAdjust="spacingAndGlyphs"
        fill={textColor} fontFamily="'Saira Variable', sans-serif" fontWeight="700" fontSize="19"
      >
        AUTO SALLON
      </text>
      <text
        x="100" y="91" textAnchor="middle" textLength="150" lengthAdjust="spacingAndGlyphs"
        fill="var(--color-brand-600)" fontFamily="'Saira Variable', sans-serif" fontWeight="800" fontSize="38"
        style={{ fontStretch: '125%' }}
      >
        LUKI
      </text>
    </svg>
  )
}
