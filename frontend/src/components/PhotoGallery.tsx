import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { CarPhoto } from '@/api/types'
import { useSwipe } from '@/lib/useSwipe'
import { CarIcon, ChevronIcon, CloseIcon, ExpandIcon } from './icons'

const arrowClass = 'absolute top-1/2 -translate-y-1/2 rounded-full p-2 shadow transition'

export function PhotoGallery({ photos, alt }: { photos: CarPhoto[]; alt: string }) {
  const { t } = useTranslation()
  const [index, setIndex] = useState(0)
  const [fullscreen, setFullscreen] = useState(false)
  const dialog = useRef<HTMLDialogElement>(null)
  const lastSwipeAt = useRef(Number.NEGATIVE_INFINITY)

  const go = (delta: number) => setIndex((i) => (i + delta + photos.length) % photos.length)
  const swipe = useSwipe((direction) => {
    lastSwipeAt.current = performance.now()
    go(direction)
  })

  useEffect(() => {
    if (!fullscreen) return
    dialog.current?.showModal()
    document.body.style.overflow = 'hidden'
    return () => {
      document.body.style.overflow = ''
    }
  }, [fullscreen])

  if (photos.length === 0) {
    return (
      <div className="card flex aspect-4/3 flex-col items-center justify-center gap-2 text-zinc-400">
        <CarIcon width={64} height={64} />
        <span>{t('car.noPhoto')}</span>
      </div>
    )
  }

  const many = photos.length > 1
  const label = (n: number) => `${alt} - ${t('detail.photoOf', { n, total: photos.length })}`
  const onArrowKeys = (e: React.KeyboardEvent) => {
    if (e.key === 'ArrowLeft') go(-1)
    if (e.key === 'ArrowRight') go(1)
  }

  return (
    <div>
      <div
        className="group relative aspect-4/3 touch-pan-y overflow-hidden rounded-2xl bg-ink-950 shadow-card select-none"
        onKeyDown={onArrowKeys}
        aria-roledescription="carousel"
        aria-label={alt}
        {...swipe}
      >
        <button
          type="button"
          className="block h-full w-full cursor-zoom-in"
          onClick={() => {
            if (performance.now() - lastSwipeAt.current > 400) setFullscreen(true)
          }}
          aria-label={t('detail.openFullscreen')}
        >
          <img src={photos[index].url} alt={label(index + 1)} draggable={false} className="h-full w-full object-contain" />
        </button>

        <span className="pointer-events-none absolute top-3 right-3 rounded-full bg-black/60 p-2 text-white opacity-80 transition group-hover:opacity-100">
          <ExpandIcon width={16} height={16} />
        </span>

        {many && (
          <>
            <button type="button" onClick={() => go(-1)} aria-label={t('detail.prevPhoto')} className={`${arrowClass} left-3 bg-white/90 hover:bg-white`}>
              <ChevronIcon direction="left" />
            </button>
            <button type="button" onClick={() => go(1)} aria-label={t('detail.nextPhoto')} className={`${arrowClass} right-3 bg-white/90 hover:bg-white`}>
              <ChevronIcon />
            </button>
            <span className="pointer-events-none absolute right-3 bottom-3 rounded-full bg-black/60 px-2.5 py-1 text-xs text-white">
              {index + 1} / {photos.length}
            </span>
          </>
        )}
      </div>

      {many && (
        <div className="mt-3 flex gap-2 overflow-x-auto pb-1">
          {photos.map((photo, i) => (
            <button
              key={photo.id}
              type="button"
              onClick={() => setIndex(i)}
              aria-label={t('detail.photoOf', { n: i + 1, total: photos.length })}
              aria-current={i === index}
              className={`h-16 w-20 shrink-0 overflow-hidden rounded-lg border-2 ${i === index ? 'border-brand-600' : 'border-transparent opacity-70 hover:opacity-100'}`}
            >
              <img src={photo.thumbUrl} alt="" loading="lazy" className="h-full w-full object-cover" />
            </button>
          ))}
        </div>
      )}

      {fullscreen && (
        <dialog
          ref={dialog}
          onClose={() => setFullscreen(false)}
          onKeyDown={onArrowKeys}
          aria-label={alt}
          className="m-0 h-dvh max-h-none w-screen max-w-none bg-black/95 p-0 backdrop:bg-black"
        >
          <div className="relative flex h-full w-full touch-none items-center justify-center select-none" {...swipe}>
            <img src={photos[index].url} alt={label(index + 1)} draggable={false} className="max-h-full max-w-full object-contain" />

            <button
              type="button"
              autoFocus
              onClick={() => dialog.current?.close()}
              aria-label={t('detail.closeFullscreen')}
              className="absolute top-4 right-4 rounded-full bg-white/10 p-3 text-white transition hover:bg-white/25"
            >
              <CloseIcon />
            </button>

            {many && (
              <>
                <button type="button" onClick={() => go(-1)} aria-label={t('detail.prevPhoto')} className={`${arrowClass} left-3 bg-white/10 p-3 text-white hover:bg-white/25 sm:left-6`}>
                  <ChevronIcon direction="left" />
                </button>
                <button type="button" onClick={() => go(1)} aria-label={t('detail.nextPhoto')} className={`${arrowClass} right-3 bg-white/10 p-3 text-white hover:bg-white/25 sm:right-6`}>
                  <ChevronIcon />
                </button>
                <span className="pointer-events-none absolute bottom-5 left-1/2 -translate-x-1/2 rounded-full bg-white/10 px-3 py-1 text-sm text-white">
                  {index + 1} / {photos.length}
                </span>
              </>
            )}
          </div>
        </dialog>
      )}
    </div>
  )
}
