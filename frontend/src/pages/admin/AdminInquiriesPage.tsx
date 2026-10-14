import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Link, useSearchParams } from 'react-router'
import { adminApi } from '@/api/endpoints'
import { INQUIRY_STATUSES, type Inquiry, type InquiryStatus } from '@/api/types'
import { useAuth } from '@/auth/useAuth'
import { ErrorState, Spinner } from '@/components/Feedback'
import { Pagination } from '@/components/Pagination'
import { phoneHref } from '@/lib/config'
import { formatDate, formatDateTime } from '@/lib/format'

const statusStyles: Record<InquiryStatus, string> = {
  New: 'bg-red-100 text-red-800',
  Contacted: 'bg-amber-100 text-amber-800',
  Closed: 'bg-zinc-100 text-zinc-600',
}

export function AdminInquiriesPage() {
  const { t } = useTranslation()
  const { isAdmin } = useAuth()
  const queryClient = useQueryClient()
  const [params, setParams] = useSearchParams()
  const status = INQUIRY_STATUSES.find((s) => s === params.get('status'))
  const page = Math.max(1, Number(params.get('page')) || 1)

  const { data, isPending, isError, refetch } = useQuery({
    queryKey: ['admin', 'inquiries', status, page],
    queryFn: () => adminApi.searchInquiries({ status, page, pageSize: 20 }),
    placeholderData: keepPreviousData,
  })

  const invalidate = () => void queryClient.invalidateQueries({ queryKey: ['admin'] })
  const changeStatus = useMutation({
    mutationFn: ({ id, status }: { id: number; status: InquiryStatus }) => adminApi.changeInquiryStatus(id, status),
    onSuccess: invalidate,
  })
  const remove = useMutation({ mutationFn: adminApi.deleteInquiry, onSuccess: invalidate })

  const setFilter = (s?: InquiryStatus) => setParams(s ? { status: s } : {})
  const tabClass = (active: boolean) =>
    `rounded-lg px-3 py-1.5 text-sm font-medium ${active ? 'bg-brand-600 text-white' : 'bg-white text-zinc-700 hover:bg-zinc-100'}`

  const actions = (inquiry: Inquiry) => (
    <div className="flex flex-wrap gap-2">
      {inquiry.status === 'New' && (
        <button type="button" className="btn-secondary btn-sm" onClick={() => changeStatus.mutate({ id: inquiry.id, status: 'Contacted' })}>
          {t('admin.inquiries.markContacted')}
        </button>
      )}
      {inquiry.status !== 'Closed' ? (
        <button type="button" className="btn-secondary btn-sm" onClick={() => changeStatus.mutate({ id: inquiry.id, status: 'Closed' })}>
          {t('admin.inquiries.markClosed')}
        </button>
      ) : (
        <button type="button" className="btn-secondary btn-sm" onClick={() => changeStatus.mutate({ id: inquiry.id, status: 'New' })}>
          {t('admin.inquiries.reopen')}
        </button>
      )}
      {isAdmin && (
        <button type="button" className="btn-danger btn-sm"
          onClick={() => window.confirm(t('admin.inquiries.confirmDelete')) && remove.mutate(inquiry.id)}>
          {t('common.delete')}
        </button>
      )}
    </div>
  )

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">{t('admin.nav.inquiries')}</h1>

      <div className="mb-4 flex flex-wrap gap-2">
        <button type="button" className={tabClass(!status)} onClick={() => setFilter()}>{t('common.all')}</button>
        {INQUIRY_STATUSES.map((s) => (
          <button key={s} type="button" className={tabClass(status === s)} onClick={() => setFilter(s)}>
            {t(`admin.inquiries.status.${s}`)}
          </button>
        ))}
      </div>

      {isPending ? (
        <Spinner />
      ) : isError ? (
        <ErrorState onRetry={() => void refetch()} />
      ) : data.items.length === 0 ? (
        <p className="card p-8 text-center text-zinc-600">{t('admin.inquiries.empty')}</p>
      ) : (
        <>
          <ul className="space-y-3">
            {data.items.map((inquiry) => (
              <li key={inquiry.id} className="card p-4">
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <p className="font-semibold">
                      {inquiry.name}
                      <span className={`ml-2 rounded-full px-2 py-0.5 text-xs font-semibold ${statusStyles[inquiry.status]}`}>
                        {t(`admin.inquiries.status.${inquiry.status}`)}
                      </span>
                      {inquiry.type === 'TestDrive' && (
                        <span className="ml-2 rounded-full bg-brand-100 px-2 py-0.5 text-xs font-semibold text-brand-700">
                          {t('inquiry.typeTestDrive')}
                        </span>
                      )}
                    </p>
                    <p className="mt-1 text-sm">
                      <a href={phoneHref(inquiry.phone)} className="font-medium text-brand-700 hover:underline">{inquiry.phone}</a>
                      {inquiry.email && (
                        <> · <a href={`mailto:${inquiry.email}`} className="text-brand-700 hover:underline">{inquiry.email}</a></>
                      )}
                    </p>
                  </div>
                  <p className="text-xs text-zinc-500">{t('admin.inquiries.received')}: {formatDateTime(inquiry.createdAt)}</p>
                </div>

                <p className="mt-2 text-sm text-zinc-600">
                  {inquiry.car ? (
                    <Link to={`/admin/cars/${inquiry.car.id}`} className="font-medium hover:underline">{inquiry.car.title}</Link>
                  ) : (
                    t('admin.inquiries.general')
                  )}
                  {inquiry.preferredDate && <> · {t('admin.inquiries.preferredDate')}: <strong>{formatDate(inquiry.preferredDate)}</strong></>}
                </p>

                <p className="mt-3 rounded-lg bg-zinc-50 p-3 text-sm whitespace-pre-line text-zinc-800">{inquiry.message}</p>
                <div className="mt-3">{actions(inquiry)}</div>
              </li>
            ))}
          </ul>
          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            onPageChange={(p) => setParams({ ...(status ? { status } : {}), page: String(p) })}
          />
        </>
      )}
    </div>
  )
}
