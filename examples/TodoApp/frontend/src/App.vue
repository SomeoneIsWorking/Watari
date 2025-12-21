<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Api } from './generated/api'

interface TodoItem {
  Id: string;
  Text: string;
  Completed: boolean;
}

const todos = ref<TodoItem[]>([])
const newTodoText = ref('')

const loadTodos = async () => {
  try {
    const result = await Api.GetTodos()
    todos.value = result
  } catch (e) {
    console.error('Error loading todos:', e)
  }
}

const addTodo = async () => {
  if (!newTodoText.value.trim()) return
  try {
    const todo = await Api.AddTodo(newTodoText.value.trim())
    todos.value.push(todo)
    newTodoText.value = ''
  } catch (e) {
    console.error('Error adding todo:', e)
  }
}

const updateTodo = async (id: string, text: string, completed: boolean) => {
  try {
    await Api.UpdateTodo(id, text, completed)
    const todo = todos.value.find(t => t.Id === id)
    if (todo) {
      todo.Text = text
      todo.Completed = completed
    }
  } catch (e) {
    console.error('Error updating todo:', e)
  }
}

const deleteTodo = async (id: string) => {
  try {
    await Api.DeleteTodo(id)
    todos.value = todos.value.filter(t => t.Id !== id)
  } catch (e) {
    console.error('Error deleting todo:', e)
  }
}

const toggleCompleted = async (todo: TodoItem) => {
  await updateTodo(todo.Id, todo.Text, !todo.Completed)
}

const moveWindowUp = async () => {
  await Api.MoveWindowUp()
}

const moveWindowDown = async () => {
  await Api.MoveWindowDown()
}

const moveWindowLeft = async () => {
  await Api.MoveWindowLeft()
}

const moveWindowRight = async () => {
  await Api.MoveWindowRight()
}

onMounted(() => {
  loadTodos()
})
</script>

<template>
  <div class="app">
    <h1>Todo App</h1>
    
    <div class="window-controls">
      <button @click="moveWindowUp" class="control-button">↑</button>
      <div class="horizontal-controls">
        <button @click="moveWindowLeft" class="control-button">←</button>
        <button @click="moveWindowRight" class="control-button">→</button>
      </div>
      <button @click="moveWindowDown" class="control-button">↓</button>
    </div>
    
    <div class="add-todo">
      <input 
        v-model="newTodoText" 
        @keyup.enter="addTodo" 
        placeholder="Add a new todo..." 
        class="todo-input"
      />
      <button @click="addTodo" class="add-button">Add</button>
    </div>

    <div class="todo-list">
      <div 
        v-for="todo in todos" 
        :key="todo.Id" 
        class="todo-item"
        :class="{ completed: todo.Completed }"
      >
        <input 
          type="checkbox" 
          :checked="todo.Completed" 
          @change="toggleCompleted(todo)"
          class="todo-checkbox"
        />
        <span class="todo-text">{{ todo.Text }}</span>
        <button @click="deleteTodo(todo.Id)" class="delete-button">Delete</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.app {
  max-width: 600px;
  margin: 0 auto;
  padding: 20px;
  font-family: Arial, sans-serif;
}

h1 {
  text-align: center;
  color: #333;
}

.window-controls {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 20px;
}

.horizontal-controls {
  display: flex;
  gap: 10px;
}

.control-button {
  width: 40px;
  height: 40px;
  font-size: 20px;
  background-color: #2196F3;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.control-button:hover {
  background-color: #1976D2;
}

.add-todo {
  display: flex;
  margin-bottom: 20px;
}

.todo-input {
  flex: 1;
  padding: 10px;
  font-size: 16px;
  border: 1px solid #ddd;
  border-radius: 4px 0 0 4px;
}

.add-button {
  padding: 10px 20px;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 0 4px 4px 0;
  cursor: pointer;
  font-size: 16px;
}

.add-button:hover {
  background-color: #45a049;
}

.todo-list {
  border: 1px solid #ddd;
  border-radius: 4px;
}

.todo-item {
  display: flex;
  align-items: center;
  padding: 10px;
  border-bottom: 1px solid #eee;
}

.todo-item:last-child {
  border-bottom: none;
}

.todo-item.completed .todo-text {
  text-decoration: line-through;
  color: #888;
}

.todo-checkbox {
  margin-right: 10px;
}

.todo-text {
  flex: 1;
  font-size: 16px;
}

.delete-button {
  padding: 5px 10px;
  background-color: #f44336;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.delete-button:hover {
  background-color: #da190b;
}
</style>
