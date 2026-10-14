import { useRef, type PointerEvent } from 'react'

const MIN_DISTANCE = 50

export function useSwipe(onSwipe: (direction: 1 | -1) => void) {
  const start = useRef<{ x: number; y: number } | null>(null)

  return {
    onPointerDown: (e: PointerEvent) => {
      start.current = { x: e.clientX, y: e.clientY }
    },
    onPointerUp: (e: PointerEvent) => {
      if (!start.current) return
      const dx = e.clientX - start.current.x
      const dy = e.clientY - start.current.y
      start.current = null
      if (Math.abs(dx) >= MIN_DISTANCE && Math.abs(dx) > Math.abs(dy) * 1.5) {
        onSwipe(dx < 0 ? 1 : -1)
      }
    },
    onPointerCancel: () => {
      start.current = null
    },
  }
}
