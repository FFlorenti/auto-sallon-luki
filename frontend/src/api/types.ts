export const FUEL_TYPES = ['Petrol', 'Diesel', 'Hybrid', 'Electric', 'Lpg'] as const
export const TRANSMISSIONS = ['Manual', 'Automatic'] as const
export const BODY_TYPES = ['Sedan', 'Hatchback', 'Wagon', 'Suv', 'Coupe', 'Convertible', 'Van', 'Pickup'] as const
export const CAR_STATUSES = ['Available', 'Sold'] as const
export const CAR_SORTS = ['Newest', 'PriceAsc', 'PriceDesc', 'YearDesc', 'MileageAsc'] as const
export const INQUIRY_STATUSES = ['New', 'Contacted', 'Closed'] as const

export type FuelType = (typeof FUEL_TYPES)[number]
export type Transmission = (typeof TRANSMISSIONS)[number]
export type BodyType = (typeof BODY_TYPES)[number]
export type CarStatus = (typeof CAR_STATUSES)[number]
export type CarSort = (typeof CAR_SORTS)[number]
export type InquiryType = 'General' | 'TestDrive'
export type InquiryStatus = (typeof INQUIRY_STATUSES)[number]

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface CarListItem {
  id: number
  brand: string
  model: string
  year: number
  price: number
  mileageKm: number
  fuelType: FuelType
  transmission: Transmission
  bodyType: BodyType
  status: CarStatus
  coverPhotoUrl: string | null
}

export interface CarPhoto {
  id: number
  url: string
  thumbUrl: string
  sortOrder: number
}

export interface CarDetail {
  id: number
  brandId: number
  brand: string
  modelId: number
  model: string
  year: number
  price: number
  mileageKm: number
  fuelType: FuelType
  transmission: Transmission
  bodyType: BodyType
  engineSizeCc: number | null
  powerHp: number | null
  color: string | null
  doors: number | null
  isCustomsCleared: boolean
  descriptionSq: string | null
  descriptionEn: string | null
  status: CarStatus
  photos: CarPhoto[]
}

export interface AdminCarDetail {
  car: CarDetail
  vin: string | null
  soldAt: string | null
  createdAt: string
  updatedAt: string
}

export interface SaveCarRequest {
  modelId: number
  year: number
  price: number
  mileageKm: number
  fuelType: FuelType
  transmission: Transmission
  bodyType: BodyType
  engineSizeCc: number | null
  powerHp: number | null
  color: string | null
  doors: number | null
  vin: string | null
  isCustomsCleared: boolean
  descriptionSq: string | null
  descriptionEn: string | null
}

export interface CarQuery {
  search?: string
  brandId?: number
  modelId?: number
  yearFrom?: number
  yearTo?: number
  priceFrom?: number
  priceTo?: number
  mileageMax?: number
  fuelType?: FuelType
  transmission?: Transmission
  bodyType?: BodyType
  status?: CarStatus
  sort?: CarSort
  page?: number
  pageSize?: number
}

export interface Model {
  id: number
  name: string
}

export interface Brand {
  id: number
  name: string
  models: Model[]
}

export interface CreateInquiryRequest {
  carId: number | null
  type: InquiryType
  name: string
  phone: string
  email: string | null
  message: string
  preferredDate: string | null
  captchaToken: string
}

export interface Inquiry {
  id: number
  type: InquiryType
  name: string
  phone: string
  email: string | null
  message: string
  preferredDate: string | null
  status: InquiryStatus
  createdAt: string
  car: { id: number; title: string } | null
}

export interface LoginResponse {
  accessToken: string
  expiresAt: string
  email: string
  roles: string[]
}

export interface Dashboard {
  carsInStock: number
  carsSoldTotal: number
  carsSoldThisMonth: number
  inquiriesTotal: number
  inquiriesNew: number
  inquiriesThisMonth: number
}
