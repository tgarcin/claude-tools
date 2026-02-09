<template>
  <q-page class="monitor-container">
    <div v-if="store.error" class="q-mb-md">
      <q-banner dense class="bg-negative text-white">
        {{ store.error }}
      </q-banner>
    </div>

    <!-- Gauge Cards Row -->
    <div class="gauge-grid">
      <q-card class="gauge-card">
        <q-card-section class="text-center">
          <div class="gauge-label">CPU</div>
          <q-circular-progress
            :value="store.current?.cpuPercent ?? 0"
            size="120px"
            :thickness="0.2"
            :color="cpuColor"
            track-color="grey-9"
            show-value
            class="q-my-sm"
          >
            <span class="gauge-value">{{ (store.current?.cpuPercent ?? 0).toFixed(0) }}%</span>
          </q-circular-progress>
        </q-card-section>
      </q-card>

      <q-card class="gauge-card">
        <q-card-section class="text-center">
          <div class="gauge-label">Memory</div>
          <q-circular-progress
            :value="store.memPercent"
            size="120px"
            :thickness="0.2"
            :color="memColor"
            track-color="grey-9"
            show-value
            class="q-my-sm"
          >
            <span class="gauge-value">{{ store.memPercent }}%</span>
          </q-circular-progress>
          <div class="gauge-sub">
            {{ formatMb(store.current?.memUsedMb ?? 0) }} / {{ formatMb(store.current?.memTotalMb ?? 0) }}
          </div>
        </q-card-section>
      </q-card>

      <q-card class="gauge-card">
        <q-card-section class="text-center">
          <div class="gauge-label">Disk</div>
          <q-circular-progress
            :value="store.current?.diskUsedPercent ?? 0"
            size="120px"
            :thickness="0.2"
            :color="diskColor"
            track-color="grey-9"
            show-value
            class="q-my-sm"
          >
            <span class="gauge-value">{{ (store.current?.diskUsedPercent ?? 0).toFixed(0) }}%</span>
          </q-circular-progress>
        </q-card-section>
      </q-card>

      <q-card class="gauge-card">
        <q-card-section class="text-center">
          <div class="gauge-label">Network</div>
          <div class="net-stats">
            <div class="net-row">
              <q-icon name="arrow_downward" color="positive" size="sm" />
              <span class="net-value">{{ formatBytes(store.current?.netRxBytesPerSec ?? 0) }}/s</span>
            </div>
            <div class="net-row">
              <q-icon name="arrow_upward" color="info" size="sm" />
              <span class="net-value">{{ formatBytes(store.current?.netTxBytesPerSec ?? 0) }}/s</span>
            </div>
          </div>
        </q-card-section>
      </q-card>
    </div>

    <!-- History Chart -->
    <q-card class="q-mt-md">
      <q-card-section>
        <div class="row items-center justify-between q-mb-sm">
          <div class="text-h6">History</div>
          <q-btn-group flat>
            <q-btn
              v-for="opt in historyOptions"
              :key="opt.minutes"
              :label="opt.label"
              :flat="selectedMinutes !== opt.minutes"
              :color="selectedMinutes === opt.minutes ? 'primary' : 'grey-5'"
              size="sm"
              dense
              @click="selectHistory(opt.minutes)"
            />
          </q-btn-group>
        </div>
        <v-chart :option="chartOption" autoresize style="height: 300px" />
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart } from 'echarts/charts'
import {
  TooltipComponent,
  GridComponent,
  LegendComponent,
} from 'echarts/components'
import VChart from 'vue-echarts'
import { useMonitorStore } from 'src/stores/monitor'

use([CanvasRenderer, LineChart, TooltipComponent, GridComponent, LegendComponent])

const store = useMonitorStore()
const selectedMinutes = ref(60)

const historyOptions = [
  { label: '10m', minutes: 10 },
  { label: '30m', minutes: 30 },
  { label: '1h', minutes: 60 },
  { label: '6h', minutes: 360 },
  { label: '24h', minutes: 1440 },
]

function selectHistory(minutes: number) {
  selectedMinutes.value = minutes
  store.fetchHistory(minutes)
}

const cpuColor = computed(() => {
  const v = store.current?.cpuPercent ?? 0
  if (v > 85) return 'negative'
  if (v > 60) return 'warning'
  return 'positive'
})

const memColor = computed(() => {
  const v = store.memPercent
  if (v > 85) return 'negative'
  if (v > 60) return 'warning'
  return 'positive'
})

const diskColor = computed(() => {
  const v = store.current?.diskUsedPercent ?? 0
  if (v > 90) return 'negative'
  if (v > 75) return 'warning'
  return 'positive'
})

function formatMb(mb: number): string {
  if (mb >= 1024) return (mb / 1024).toFixed(1) + ' GB'
  return Math.round(mb) + ' MB'
}

function formatBytes(bytes: number): string {
  if (bytes >= 1048576) return (bytes / 1048576).toFixed(1) + ' MB'
  if (bytes >= 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return Math.round(bytes) + ' B'
}

function formatTime(ts: string): string {
  const d = new Date(ts)
  return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

const chartOption = computed(() => ({
  backgroundColor: 'transparent',
  textStyle: { color: '#ccc' },
  tooltip: {
    trigger: 'axis',
    backgroundColor: '#222',
    borderColor: '#444',
    textStyle: { color: '#ccc' },
  },
  legend: {
    data: ['CPU %', 'RAM %'],
    textStyle: { color: '#ccc' },
    top: 0,
  },
  grid: {
    left: 50,
    right: 20,
    top: 30,
    bottom: 30,
  },
  xAxis: {
    type: 'category',
    data: store.history.map((s) => formatTime(s.timestamp)),
    axisLine: { lineStyle: { color: '#666' } },
    axisLabel: { color: '#999', fontSize: 11 },
  },
  yAxis: {
    type: 'value',
    min: 0,
    max: 100,
    axisLine: { lineStyle: { color: '#666' } },
    axisLabel: { color: '#999', fontSize: 11 },
    splitLine: { lineStyle: { color: '#333', type: 'dashed' } },
  },
  series: [
    {
      name: 'CPU %',
      type: 'line',
      data: store.history.map((s) => s.cpuPercent),
      smooth: true,
      symbol: 'none',
      lineStyle: { width: 2 },
      itemStyle: { color: '#c084fc' },
      areaStyle: { color: 'rgba(192, 132, 252, 0.08)' },
    },
    {
      name: 'RAM %',
      type: 'line',
      data: store.history.map((s) =>
        s.memTotalMb > 0 ? Math.round((s.memUsedMb / s.memTotalMb) * 100) : 0
      ),
      smooth: true,
      symbol: 'none',
      lineStyle: { width: 2 },
      itemStyle: { color: '#31CCEC' },
      areaStyle: { color: 'rgba(49, 204, 236, 0.08)' },
    },
  ],
}))

onMounted(() => {
  store.startPolling()
  store.fetchHistory(selectedMinutes.value)
})

onUnmounted(() => {
  store.stopPolling()
})
</script>

<style lang="scss" scoped>
.monitor-container {
  padding: 16px;
  max-width: 1100px;
  margin: 0 auto;
}

.gauge-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
}

.gauge-card {
  min-height: 200px;
}

.gauge-label {
  font-size: 14px;
  font-weight: 500;
  color: #999;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.gauge-value {
  font-size: 22px;
  font-weight: 700;
  color: #eee;
}

.gauge-sub {
  font-size: 12px;
  color: #888;
  margin-top: 4px;
}

.net-stats {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 20px 0;
}

.net-row {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.net-value {
  font-size: 18px;
  font-weight: 600;
  color: #eee;
}

@media (max-width: 800px) {
  .gauge-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
