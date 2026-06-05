<template>
  <div class="container flex-center login-page animate-fade-in">
    <div class="glass-panel login-card">
      <h2 class="card-title text-center gradient-text">Chào Mừng Trở Lại</h2>
      <p class="card-subtitle text-center">Đăng nhập để đăng tin hoặc quản lý tin đăng của bạn.</p>

      <!-- Tabs Selector -->
      <div class="tabs-selector">
        <button @click="loginMethod = 'email'" class="tab-btn" :class="{ active: loginMethod === 'email' }">Email / Mật khẩu</button>
        <button @click="loginMethod = 'phone'" class="tab-btn" :class="{ active: loginMethod === 'phone' }">Số điện thoại / OTP</button>
      </div>

      <!-- Error message display -->
      <div v-if="errorMessage || authStore.error" class="error-msg">
        ⚠️ {{ errorMessage || authStore.error }}
      </div>

      <!-- Email Login Form -->
      <form v-if="loginMethod === 'email'" @submit.prevent="handleEmailLogin">
        <div class="form-group">
          <label class="form-label">Email đăng nhập</label>
          <input v-model="emailForm.email" type="email" class="form-input" placeholder="example@gmail.com" required />
        </div>
        <div class="form-group">
          <label class="form-label">Mật khẩu</label>
          <input v-model="emailForm.password" type="password" class="form-input" placeholder="••••••••" required />
        </div>
        <button type="submit" class="btn btn-primary btn-block" :disabled="authStore.loading">
          {{ authStore.loading ? 'Đang xác thực...' : 'Đăng nhập' }}
        </button>
      </form>

      <!-- Phone Login Form -->
      <div v-else class="phone-login-area">
        <!-- Step 1: Send OTP -->
        <form v-if="!otpSent" @submit.prevent="handleSendOtp">
          <div class="form-group">
            <label class="form-label">Số điện thoại của bạn</label>
            <input v-model="phoneForm.phone" type="tel" class="form-input" placeholder="Ví dụ: 0987654321" required />
          </div>
          <button type="submit" class="btn btn-primary btn-block" :disabled="sendingOtp">
            {{ sendingOtp ? 'Đang gửi mã OTP...' : 'Gửi mã xác thực OTP' }}
          </button>
        </form>

        <!-- Step 2: Verify OTP -->
        <form v-else @submit.prevent="handleVerifyOtp">
          <p class="otp-notice">Chúng tôi đã gửi mã xác thực gồm 6 chữ số tới số điện thoại <strong>{{ phoneForm.phone }}</strong>. Mã mặc định cho nhà phát triển là <strong>123456</strong>.</p>
          
          <div class="form-group">
            <label class="form-label">Nhập mã xác thực OTP</label>
            <input v-model="phoneForm.otp" type="text" class="form-input otp-input" placeholder="123456" maxlength="6" required />
          </div>
          
          <div class="phone-actions">
            <button type="button" @click="otpSent = false" class="btn btn-outline">Quay lại</button>
            <button type="submit" class="btn btn-primary" :disabled="authStore.loading">
              {{ authStore.loading ? 'Đang xác thực...' : 'Xác nhận đăng nhập' }}
            </button>
          </div>
        </form>
      </div>

      <div class="card-footer-auth">
        <p>Chưa có tài khoản? <router-link to="/register" class="auth-link">Đăng ký ngay</router-link></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axios from 'axios'
import { useAuthStore } from '../../stores/auth'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

// States
const loginMethod = ref('email')
const errorMessage = ref('')
const sendingOtp = ref(false)
const otpSent = ref(false)

// Forms
const emailForm = reactive({
  email: '',
  password: ''
})

const phoneForm = reactive({
  phone: '',
  otp: ''
})

const handleEmailLogin = async () => {
  errorMessage.value = ''
  const success = await authStore.login(emailForm.email, emailForm.password)
  if (success) {
    navigateUser()
  }
}

const handleSendOtp = async () => {
  errorMessage.value = ''
  sendingOtp.value = true
  try {
    await axios.post('http://localhost:5123/api/auth/send-otp', {
      phoneNumber: phoneForm.phone
    })
    otpSent.value = true
  } catch (err) {
    errorMessage.value = err.response?.data?.message || 'Không thể gửi OTP. Vui lòng kiểm tra số điện thoại.'
  } finally {
    sendingOtp.value = false
  }
}

const handleVerifyOtp = async () => {
  errorMessage.value = ''
  const success = await authStore.loginWithPhone(phoneForm.phone, phoneForm.otp)
  if (success) {
    navigateUser()
  }
}

// Redirect after login
const navigateUser = () => {
  const redirectPath = route.query.redirect || '/'
  router.push(redirectPath)
}
</script>

<style scoped>
.login-page {
  min-height: calc(100vh - 200px);
  padding: 2rem 0;
}

.login-card {
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

/* Tabs */
.tabs-selector {
  display: flex;
  background: rgba(15, 23, 42, 0.4);
  border: 1px solid var(--border-glass);
  border-radius: var(--radius-sm);
  padding: 0.25rem;
  margin-bottom: 2rem;
}

.tab-btn {
  flex: 1;
  background: transparent;
  border: none;
  color: var(--text-secondary);
  font-weight: 600;
  font-size: 0.9rem;
  padding: 0.5rem 0;
  cursor: pointer;
  border-radius: 4px;
  transition: var(--transition-fast);
}

.tab-btn.active {
  background-color: var(--primary);
  color: #fff;
}

/* Error message */
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

.otp-notice {
  font-size: 0.85rem;
  color: var(--text-secondary);
  line-height: 1.5;
  margin-bottom: 1.25rem;
  background: rgba(255, 255, 255, 0.03);
  padding: 0.75rem;
  border-radius: 6px;
}

.otp-input {
  text-align: center;
  font-size: 1.5rem;
  letter-spacing: 0.5rem;
  font-weight: 700;
}

.phone-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
}

.phone-actions .btn {
  flex: 1;
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
