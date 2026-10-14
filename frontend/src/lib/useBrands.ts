import { useQuery } from '@tanstack/react-query'
import { publicApi } from '@/api/endpoints'

export function useBrands() {
  return useQuery({ queryKey: ['brands'], queryFn: publicApi.getBrands, staleTime: 10 * 60_000 })
}
