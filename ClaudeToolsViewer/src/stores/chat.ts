import { defineStore } from 'pinia'
import { ref } from 'vue'

const API_BASE = '/api/chat'

export interface ChatResponse {
  text: string
  tokensIn: number
  tokensOut: number
}

export const useChatStore = defineStore('chat', () => {
  const loading = ref(false)
  const lastResponse = ref<ChatResponse | null>(null)
  const error = ref<string | null>(null)

  async function ask(message: string): Promise<ChatResponse> {
    loading.value = true
    error.value = null
    lastResponse.value = null

    try {
      const res = await fetch(API_BASE, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message }),
      })

      if (!res.ok) {
        const err = await res.json()
        throw new Error(err.error || `HTTP ${res.status}`)
      }

      const data: ChatResponse = await res.json()
      lastResponse.value = data
      return data
    } catch (e) {
      const msg = e instanceof Error ? e.message : 'Unknown error'
      error.value = msg
      throw e
    } finally {
      loading.value = false
    }
  }

  return { loading, lastResponse, error, ask }
})
