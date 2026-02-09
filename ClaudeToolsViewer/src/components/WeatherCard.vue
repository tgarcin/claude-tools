<template>
  <q-card class="section-card">
    <q-card-section>
      <div class="text-h6">Weather Assistant</div>
      <div class="text-caption text-grey">Ask Claude about the weather anywhere</div>
    </q-card-section>

    <q-card-section>
      <div class="row q-gutter-sm items-end">
        <q-input
          v-model="location"
          dense
          outlined
          label="Location"
          placeholder="e.g. Montreal, Tokyo, Paris"
          class="col"
          :disable="store.loading"
          @keyup.enter="askWeather"
        />
        <q-btn
          color="primary"
          label="Ask"
          :loading="store.loading"
          :disable="!location.trim()"
          @click="askWeather"
        />
      </div>
    </q-card-section>

    <q-card-section v-if="store.error">
      <q-banner dense class="bg-negative text-white rounded-borders">
        {{ store.error }}
      </q-banner>
    </q-card-section>

    <q-card-section v-if="store.lastResponse">
      <div class="response-text">{{ store.lastResponse.text }}</div>
      <div class="text-caption text-grey q-mt-sm">
        Tokens: {{ store.lastResponse.tokensIn }} in / {{ store.lastResponse.tokensOut }} out
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useChatStore } from 'src/stores/chat'

const store = useChatStore()
const location = ref('')

async function askWeather() {
  if (!location.value.trim()) return
  const prompt = `What is the current weather like in ${location.value}? Give a brief, friendly summary. If you don't have real-time data, provide typical weather for this time of year and mention that.`
  await store.ask(prompt)
}
</script>

<style lang="scss" scoped>
.response-text {
  white-space: pre-wrap;
  line-height: 1.6;
  color: #ddd;
}
</style>
