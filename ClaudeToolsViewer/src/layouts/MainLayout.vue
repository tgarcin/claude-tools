<template>
  <q-layout view="hHh LpR fFf">
    <q-header elevated class="bg-dark">
      <q-toolbar>
        <q-toolbar-title>
          <span class="text-weight-bold">Claude Tools</span>
          <span class="text-caption q-ml-sm text-grey-5">Personal Assistant</span>
        </q-toolbar-title>
        <q-btn flat to="/" label="Chat" :color="route.name === 'home' ? 'primary' : 'grey-5'" />
        <q-btn flat to="/audit" label="Audit" :color="route.name === 'audit' ? 'primary' : 'grey-5'" />
        <q-btn flat to="/monitor" label="Monitor" :color="route.name === 'monitor' ? 'primary' : 'grey-5'" />
      </q-toolbar>
    </q-header>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'

const route = useRoute()
</script>
