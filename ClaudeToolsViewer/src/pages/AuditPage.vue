<template>
  <q-page class="page-container">
    <q-card class="section-card">
      <q-card-section class="row items-center justify-between">
        <div class="text-h6">Audit Log</div>
        <q-btn flat icon="refresh" color="primary" :loading="store.loading" @click="store.fetchLogs()" />
      </q-card-section>

      <q-card-section v-if="store.error">
        <q-banner class="bg-negative text-white">{{ store.error }}</q-banner>
      </q-card-section>

      <q-card-section class="q-pa-none">
        <q-table
          :rows="store.logs"
          :columns="columns"
          row-key="id"
          flat
          dark
          class="audit-table"
          :loading="store.loading"
          :pagination="{ rowsPerPage: 20 }"
          @row-click="onRowClick"
        >
          <template #body-cell-success="props">
            <q-td :props="props">
              <q-icon
                :name="props.row.success ? 'check_circle' : 'error'"
                :color="props.row.success ? 'positive' : 'negative'"
                size="sm"
              />
            </q-td>
          </template>

          <template #body-cell-timestamp="props">
            <q-td :props="props">
              {{ formatDate(props.row.timestamp) }}
            </q-td>
          </template>

          <template #body-cell-inputData="props">
            <q-td :props="props">
              <span class="ellipsis-cell">{{ props.row.inputData }}</span>
            </q-td>
          </template>

          <template #body-cell-tokens="props">
            <q-td :props="props">
              {{ props.row.tokensIn }} / {{ props.row.tokensOut }}
            </q-td>
          </template>
        </q-table>
      </q-card-section>
    </q-card>

    <!-- Detail dialog -->
    <q-dialog v-model="showDetail" maximized transition-show="slide-up" transition-hide="slide-down">
      <q-card class="bg-dark">
        <q-toolbar class="bg-dark">
          <q-toolbar-title>Audit Detail #{{ store.selectedLog?.id }}</q-toolbar-title>
          <q-btn flat icon="close" @click="showDetail = false" />
        </q-toolbar>

        <q-card-section v-if="store.selectedLog" class="q-gutter-md" style="max-width: 960px; margin: 0 auto;">
          <div class="row q-col-gutter-md">
            <div class="col-6">
              <div class="text-grey-5 text-caption">Timestamp</div>
              <div>{{ formatDate(store.selectedLog.timestamp) }}</div>
            </div>
            <div class="col-3">
              <div class="text-grey-5 text-caption">Status</div>
              <q-badge :color="store.selectedLog.success ? 'positive' : 'negative'">
                {{ store.selectedLog.success ? 'Success' : 'Failed' }}
              </q-badge>
            </div>
            <div class="col-3">
              <div class="text-grey-5 text-caption">Tokens (in / out)</div>
              <div>{{ store.selectedLog.tokensIn }} / {{ store.selectedLog.tokensOut }}</div>
            </div>
          </div>

          <div v-if="store.selectedLog.errorMessage">
            <div class="text-grey-5 text-caption">Error</div>
            <q-banner class="bg-negative text-white q-mt-xs">{{ store.selectedLog.errorMessage }}</q-banner>
          </div>

          <div>
            <div class="text-grey-5 text-caption">Input</div>
            <pre class="code-block">{{ store.selectedLog.inputData }}</pre>
          </div>

          <div>
            <div class="text-grey-5 text-caption">Output</div>
            <pre class="code-block">{{ store.selectedLog.outputData }}</pre>
          </div>

          <div v-if="store.selectedLog.promptSent">
            <div class="text-grey-5 text-caption">Prompt Sent (raw JSON)</div>
            <pre class="code-block">{{ formatJson(store.selectedLog.promptSent) }}</pre>
          </div>

          <div v-if="store.selectedLog.rawResponse">
            <div class="text-grey-5 text-caption">Raw Response</div>
            <pre class="code-block">{{ formatJson(store.selectedLog.rawResponse) }}</pre>
          </div>
        </q-card-section>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuditStore, type AuditLog } from 'src/stores/audit'

const store = useAuditStore()
const showDetail = ref(false)

const columns = [
  { name: 'success', label: '', field: 'success', align: 'center' as const, style: 'width: 40px' },
  { name: 'timestamp', label: 'Time', field: 'timestamp', align: 'left' as const },
  { name: 'operation', label: 'Operation', field: 'operation', align: 'left' as const },
  { name: 'inputData', label: 'Input', field: 'inputData', align: 'left' as const, style: 'max-width: 300px' },
  { name: 'tokens', label: 'Tokens (in/out)', field: 'tokensIn', align: 'right' as const },
]

function formatDate(iso: string) {
  return new Date(iso).toLocaleString()
}

function formatJson(raw: string) {
  try {
    return JSON.stringify(JSON.parse(raw), null, 2)
  } catch {
    return raw
  }
}

async function onRowClick(_evt: Event, row: AuditLog) {
  await store.fetchDetail(row.id)
  showDetail.value = true
}

onMounted(() => {
  store.fetchLogs()
})
</script>

<style lang="scss" scoped>
.audit-table {
  :deep(tr) {
    cursor: pointer;
  }
}

.ellipsis-cell {
  display: inline-block;
  max-width: 300px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.code-block {
  background: #111;
  border: 1px solid #333;
  border-radius: 4px;
  padding: 12px;
  overflow-x: auto;
  white-space: pre-wrap;
  word-break: break-word;
  font-size: 13px;
  color: #ccc;
  margin-top: 4px;
}
</style>
