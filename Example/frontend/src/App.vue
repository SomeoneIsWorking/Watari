<script setup lang="ts">
import { ref } from 'vue'
import { Api } from './generated/api'
import * as models from './generated/models'

const result = ref<string>('')

const testHello = async () => {
  try {
    const res = await Api.Hello('World')
    result.value = res
  } catch (e) {
    result.value = 'Error: ' + e
  }
}

const testGetX = async () => {
  try {
    const y: models.Y = {
      Value: 42
    }
    const res = await Api.GetX(y)
    result.value = `X: ${res.Name}`
  } catch (e) {
    result.value = 'Error: ' + e
  }
}
</script>

<template>
  <div>
    <a href="https://vite.dev" target="_blank">
      <img src="/vite.svg" class="logo" alt="Vite logo" />
    </a>
    <a href="https://vuejs.org/" target="_blank">
      <img src="./assets/vue.svg" class="logo vue" alt="Vue logo" />
    </a>
  </div>
  <HelloWorld msg="Vite + Vue" />
  <div>
    <button @click="testHello">Test Hello</button>
    <button @click="testGetX">Test GetX</button>
    <p>Result: {{ result }}</p>
  </div>
</template>

<style scoped>
.logo {
  height: 6em;
  padding: 1.5em;
  will-change: filter;
  transition: filter 300ms;
}
.logo:hover {
  filter: drop-shadow(0 0 2em #646cffaa);
}
.logo.vue:hover {
  filter: drop-shadow(0 0 2em #42b883aa);
}
</style>
