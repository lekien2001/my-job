import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './style.css'
import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/auth'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

// Tự động khởi tạo cấu hình axios token nếu có sẵn
const authStore = useAuthStore()
authStore.init()

app.mount('#app')
