import { api, toQueryString } from './client'
import type {
  AdminCarDetail,
  Brand,
  CarDetail,
  CarListItem,
  CarPhoto,
  CarQuery,
  CarStatus,
  CreateInquiryRequest,
  Dashboard,
  Inquiry,
  InquiryStatus,
  LoginResponse,
  Model,
  PagedResult,
  SaveCarRequest,
} from './types'

export const publicApi = {
  searchCars: (query: CarQuery) => api.get<PagedResult<CarListItem>>(`/api/cars${toQueryString(query)}`),
  getCar: (id: number) => api.get<CarDetail>(`/api/cars/${id}`),
  getBrands: () => api.get<Brand[]>('/api/brands'),
  createInquiry: (request: CreateInquiryRequest) => api.post<void>('/api/inquiries', request),
}

export const authApi = {
  login: (email: string, password: string) => api.post<LoginResponse>('/api/auth/login', { email, password }),
}

export const adminApi = {
  dashboard: () => api.get<Dashboard>('/api/admin/dashboard'),

  searchCars: (query: CarQuery) => api.get<PagedResult<CarListItem>>(`/api/admin/cars${toQueryString(query)}`),
  getCar: (id: number) => api.get<AdminCarDetail>(`/api/admin/cars/${id}`),
  createCar: (request: SaveCarRequest) => api.post<{ id: number }>('/api/admin/cars', request),
  updateCar: (id: number, request: SaveCarRequest) => api.put<void>(`/api/admin/cars/${id}`, request),
  changeCarStatus: (id: number, status: CarStatus) => api.patch<void>(`/api/admin/cars/${id}/status`, { status }),
  deleteCar: (id: number) => api.delete<void>(`/api/admin/cars/${id}`),

  uploadPhotos: (carId: number, files: File[]) => {
    const form = new FormData()
    files.forEach((file) => form.append('files', file))
    return api.post<CarPhoto[]>(`/api/admin/cars/${carId}/photos`, form)
  },
  deletePhoto: (carId: number, photoId: number) => api.delete<void>(`/api/admin/cars/${carId}/photos/${photoId}`),
  reorderPhotos: (carId: number, photoIds: number[]) =>
    api.put<void>(`/api/admin/cars/${carId}/photos/order`, { photoIds }),

  createBrand: (name: string) => api.post<Brand>('/api/admin/brands', { name }),
  createModel: (brandId: number, name: string) => api.post<Model>(`/api/admin/brands/${brandId}/models`, { name }),

  searchInquiries: (query: { status?: InquiryStatus; page?: number; pageSize?: number }) =>
    api.get<PagedResult<Inquiry>>(`/api/admin/inquiries${toQueryString(query)}`),
  changeInquiryStatus: (id: number, status: InquiryStatus) =>
    api.patch<void>(`/api/admin/inquiries/${id}/status`, { status }),
  deleteInquiry: (id: number) => api.delete<void>(`/api/admin/inquiries/${id}`),
}
