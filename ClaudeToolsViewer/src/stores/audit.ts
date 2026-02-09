import { defineStore } from 'pinia'
import { ref } from 'vue'

export interface AuditLog {
  id: number
  timestamp: string
  operation: string
  inputData: string | null
  outputData: string | null
  promptSent: string | null
  rawResponse: string | null
  tokensIn: number
  tokensOut: number
  success: boolean
  errorMessage: string | null
}

export const useAuditStore = defineStore('audit', () => {
  const logs = ref<AuditLog[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const selectedLog = ref<AuditLog | null>(null)

  async function fetchLogs(limit = 50) {
    loading.value = true
    error.value = null
    try {
      const res = await fetch(`/api/audit?limit=${limit}`)
      if (!res.ok) throw new Error(`HTTP ${res.status}`)
      logs.value = await res.json()
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Unknown error'
    } finally {
      loading.value = false
    }
  }

  async function fetchDetail(id: number) {
    try {
      const res = await fetch(`/api/audit/${id}`)
      if (!res.ok) throw new Error(`HTTP ${res.status}`)
      selectedLog.value = await res.json()
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Unknown error'
    }
  }

  return { logs, loading, error, selectedLog, fetchLogs, fetchDetail }
})
