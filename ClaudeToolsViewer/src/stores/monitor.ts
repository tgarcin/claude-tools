import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface SystemSnapshot {
  cpuPercent: number
  memUsedMb: number
  memTotalMb: number
  diskUsedPercent: number
  netRxBytesPerSec: number
  netTxBytesPerSec: number
  timestamp: string
}

export const useMonitorStore = defineStore('monitor', () => {
  const current = ref<SystemSnapshot | null>(null)
  const history = ref<SystemSnapshot[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  let pollTimer: ReturnType<typeof setInterval> | null = null
  let historyTimer: ReturnType<typeof setInterval> | null = null
  let historyMinutes = 60

  const memPercent = computed(() => {
    if (!current.value || current.value.memTotalMb === 0) return 0
    return Math.round(
      (current.value.memUsedMb / current.value.memTotalMb) * 100
    )
  })

  async function fetchCurrent() {
    try {
      const res = await fetch('/api/monitor/current')
      if (res.status === 204) return
      if (!res.ok) throw new Error(`HTTP ${res.status}`)
      current.value = await res.json()
      error.value = null
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Unknown error'
    }
  }

  async function fetchHistory(minutes?: number) {
    if (minutes !== undefined) historyMinutes = minutes
    loading.value = true
    try {
      const res = await fetch(`/api/monitor/history?minutes=${historyMinutes}`)
      if (!res.ok) throw new Error(`HTTP ${res.status}`)
      history.value = await res.json()
      error.value = null
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Unknown error'
    } finally {
      loading.value = false
    }
  }

  function startPolling() {
    if (pollTimer) return
    fetchCurrent()
    pollTimer = setInterval(fetchCurrent, 3000)
    // Refresh history every 30s (matches backend persist interval)
    historyTimer = setInterval(() => fetchHistory(), 30000)
  }

  function stopPolling() {
    if (pollTimer) {
      clearInterval(pollTimer)
      pollTimer = null
    }
    if (historyTimer) {
      clearInterval(historyTimer)
      historyTimer = null
    }
  }

  return {
    current,
    history,
    loading,
    error,
    memPercent,
    fetchCurrent,
    fetchHistory,
    startPolling,
    stopPolling,
  }
})
