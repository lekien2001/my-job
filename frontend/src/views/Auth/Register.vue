<template>
  <div class="container flex-center register-page animate-fade-in">
    <div class="glass-panel register-card">
      <h2 class="card-title text-center gradient-text">Đăng Ký Tài Khoản</h2>
      <p class="card-subtitle text-center">Tạo tài khoản để đăng tin cho thuê hoặc bán nhà nhanh chóng.</p>

      <!-- Error message display -->
      <div v-if="errorMessage || authStore.error" class="error-msg">
        ⚠️ {{ errorMessage || authStore.error }}
      </div>

      <!-- Registration Form -->
      <form @submit.prevent="handleRegister">
        <div class="form-group">
          <label class="form-label">Họ và tên của bạn</label>
          <input v-model="form.fullName" type="text" class="form-input" placeholder="Ví dụ: Nguyễn Văn An" required />
        </div>

        <div class="form-group">
          <label class="form-label">Địa chỉ Email (tùy chọn)</label>
          <input v-model="form.email" type="email" class="form-input" placeholder="example@gmail.com" />
        </div>

        <div class="form-group">
          <label class="form-label">Số điện thoại (tùy chọn)</label>
          <input v-model="form.phoneNumber" type="tel" class="form-input" placeholder="Ví dụ: 0987654321" />
        </div>

        <div class="form-group">
          <label class="form-label">Mật khẩu đăng nhập</label>
          <input v-model="form.password" type="password" class="form-input" placeholder="••••••••" required minlength="6" />
        </div>

        <button type="submit" class="btn btn-primary btn-block" :disabled="authStore.loading">
          {{ authStore.loading ? 'Đang tạo tài khoản...' : 'Đăng ký tài khoản' }}
        </button>
      </form>

      <div class="card-footer-auth">
        <p>Đã có tài khoản? <router-link to="/login" class="auth-link">Đăng nhập</router-link></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'

const authStore = useAuthStore()
const router = useRouter()

// States
const errorMessage = ref('')

// Form
const form = reactive({
  fullName: '',
  email: '',
  phoneNumber: '',
  password: ''
})

const handleRegister = async () => {
  errorMessage.value = ''
  
  if (!form.email && !form.phoneNumber) {
    errorMessage.value = 'Bạn phải nhập ít nhất Email hoặc Số điện thoại để đăng ký.'
    return
  }

  const success = await authStore.register(
    form.email || null,
    form.phoneNumber || null,
    form.password,
    form.fullName
  )

  if (success) {
    router.push('/')
  }
}
</script>

<style scoped>
.register-page {
  min-height: calc(100vh - 200px);
  padding: 2rem 0;
}

.register-card {
  width: 100%;
  max-width: 440px;
  padding: 2.5rem;
  border-radius: var(--radius-lg);
}

.text-center {
  text-align: center;
}

.card-title {
  font-size: 1.8rem;
  margin-bottom: 0.5rem;
}

.card-subtitle {
  color: var(--text-secondary);
  font-size: 0.9rem;
  margin-bottom: 2rem;
}

.error-msg {
  background: rgba(239, 68, 68, 0.15);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ff6b6b;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-sm);
  font-size: 0.9rem;
  margin-bottom: 1.5rem;
  font-weight: 500;
}

.btn-block {
  width: 100%;
  margin-top: 1.5rem;
}

.card-footer-auth {
  text-align: center;
  margin-top: 2rem;
  font-size: 0.9rem;
  color: var(--text-secondary);
  border-top: 1px solid var(--border-glass);
  padding-top: 1.25rem;
}

.auth-link {
  color: var(--primary);
  font-weight: 600;
}

.auth-link:hover {
  text-decoration: underline;
}
</style>
