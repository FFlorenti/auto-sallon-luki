import { useTranslation } from 'react-i18next'
import { useFavorites } from '@/lib/favorites'
import { HeartIcon } from './icons'

export function FavoriteButton({ carId, className = '' }: { carId: number; className?: string }) {
  const { t } = useTranslation()
  const { isFavorite, toggle } = useFavorites()
  const active = isFavorite(carId)

  return (
    <button
      type="button"
      onClick={(e) => {
        e.preventDefault()
        toggle(carId)
      }}
      aria-pressed={active}
      aria-label={active ? t('favorites.remove') : t('favorites.add')}
      title={active ? t('favorites.remove') : t('favorites.add')}
      className={`rounded-full bg-white/90 p-2 shadow transition hover:scale-110 ${active ? 'text-red-500' : 'text-zinc-600'} ${className}`}
    >
      <HeartIcon filled={active} />
    </button>
  )
}
