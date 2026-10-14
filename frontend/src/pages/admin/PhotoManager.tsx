import { useRef } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/api/client'
import { adminApi } from '@/api/endpoints'
import type { CarPhoto } from '@/api/types'
import { ChevronIcon } from '@/components/icons'

export function PhotoManager({ carId, photos }: { carId: number; photos: CarPhoto[] }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const fileInput = useRef<HTMLInputElement>(null)

  const refresh = () => {
    void queryClient.invalidateQueries({ queryKey: ['admin'] })
    void queryClient.invalidateQueries({ queryKey: ['cars'] })
    void queryClient.invalidateQueries({ queryKey: ['car', carId] })
  }

  const upload = useMutation({
    mutationFn: (files: File[]) => adminApi.uploadPhotos(carId, files),
    onSuccess: refresh,
    onSettled: () => {
      if (fileInput.current) fileInput.current.value = ''
    },
  })
  const remove = useMutation({ mutationFn: (photoId: number) => adminApi.deletePhoto(carId, photoId), onSuccess: refresh })
  const reorder = useMutation({ mutationFn: (ids: number[]) => adminApi.reorderPhotos(carId, ids), onSuccess: refresh })

  const move = (index: number, delta: number) => {
    const ids = photos.map((p) => p.id)
    const [moved] = ids.splice(index, 1)
    ids.splice(index + delta, 0, moved)
    reorder.mutate(ids)
  }

  const error = [upload.error, remove.error, reorder.error].find(Boolean)
  const busy = upload.isPending || remove.isPending || reorder.isPending

  return (
    <section className="card p-5">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold">{t('admin.photos.title')}</h2>
          <p className="text-xs text-zinc-500">{t('admin.photos.hint')}</p>
        </div>
        <label className={`btn-primary cursor-pointer ${busy ? 'pointer-events-none opacity-60' : ''}`}>
          {upload.isPending ? t('admin.photos.uploading') : t('admin.photos.upload')}
          <input
            ref={fileInput}
            type="file"
            accept="image/jpeg,image/png,image/webp"
            multiple
            className="sr-only"
            onChange={(e) => {
              const files = Array.from(e.target.files ?? [])
              if (files.length > 0) upload.mutate(files)
            }}
          />
        </label>
      </div>

      {error && (
        <p className="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700" role="alert">
          {error instanceof ApiError ? error.message : t('common.error')}
        </p>
      )}

      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
        {photos.map((photo, i) => (
          <div key={photo.id} className="group relative overflow-hidden rounded-lg border border-zinc-200">
            <img src={photo.thumbUrl} alt="" loading="lazy" className="aspect-4/3 w-full object-cover" />
            {i === 0 && (
              <span className="absolute top-2 left-2 rounded bg-brand-600 px-2 py-0.5 text-xs font-semibold text-white">
                {t('admin.photos.cover')}
              </span>
            )}
            <div className="flex items-center justify-between bg-white p-1.5">
              <div className="flex gap-1">
                <button type="button" className="btn-secondary btn-sm px-2" disabled={busy || i === 0} onClick={() => move(i, -1)} aria-label={t('admin.photos.moveLeft')}>
                  <ChevronIcon direction="left" width={14} height={14} />
                </button>
                <button type="button" className="btn-secondary btn-sm px-2" disabled={busy || i === photos.length - 1} onClick={() => move(i, 1)} aria-label={t('admin.photos.moveRight')}>
                  <ChevronIcon width={14} height={14} />
                </button>
              </div>
              <button type="button" className="btn-danger btn-sm" disabled={busy}
                onClick={() => window.confirm(t('admin.photos.confirmDelete')) && remove.mutate(photo.id)}>
                {t('common.delete')}
              </button>
            </div>
          </div>
        ))}
      </div>
    </section>
  )
}
