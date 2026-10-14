import { useSyncExternalStore } from 'react'

const KEY = 'autoluki.favorites'
const listeners = new Set<() => void>()
let cache: number[] = read()

function read(): number[] {
  try {
    const parsed: unknown = JSON.parse(localStorage.getItem(KEY) ?? '[]')
    return Array.isArray(parsed) ? parsed.filter((x): x is number => typeof x === 'number') : []
  } catch {
    return []
  }
}

function write(ids: number[]) {
  cache = ids
  try {
    localStorage.setItem(KEY, JSON.stringify(ids))
  } catch {
  }
  listeners.forEach((l) => l())
}

function subscribe(listener: () => void) {
  listeners.add(listener)
  const onStorage = (e: StorageEvent) => {
    if (e.key === KEY) {
      cache = read()
      listener()
    }
  }
  window.addEventListener('storage', onStorage)
  return () => {
    listeners.delete(listener)
    window.removeEventListener('storage', onStorage)
  }
}

export function useFavorites() {
  const ids = useSyncExternalStore(subscribe, () => cache)

  return {
    ids,
    isFavorite: (id: number) => ids.includes(id),
    toggle: (id: number) => write(ids.includes(id) ? ids.filter((x) => x !== id) : [...ids, id]),
  }
}
