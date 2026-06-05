import { defineStore } from 'pinia'
import axios from 'axios'

const API_URL = 'http://localhost:5123/api/auth'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user')) || null,
    loading: false,
    error: null
  }),
  getters: {
    isAuthenticated: (state) => !!state.user && !!state.user.token,
    isAdmin: (state) => state.user?.role === 'Admin',
    token: (state) => state.user?.token || ''
  },
  actions: {
    async login(email, password) {
      this.loading = true
      this.error = null
      try {
        const response = await axios.post(`${API_URL}/login`, { email, password })
        this.user = response.data
        localStorage.setItem('user', JSON.stringify(this.user))
        axios.defaults.headers.common['Authorization'] = `Bearer ${this.user.token}`
        return true
      } catch (err) {
        this.error = err.response?.data?.message || 'Đăng nhập không thành công.'
        return false
      } finally {
        this.loading = false
      }
    },
    async loginWithPhone(phoneNumber, otpCode) {
      this.loading = true
      this.error = null
      try {
        const response = await axios.post(`${API_URL}/verify-otp`, { phoneNumber, otpCode })
        this.user = response.data
        localStorage.setItem('user', JSON.stringify(this.user))
        axios.defaults.headers.common['Authorization'] = `Bearer ${this.user.token}`
        return true
      } catch (err) {
        this.error = err.response?.data?.message || 'Xác thực OTP thất bại.'
        return false
      } finally {
        this.loading = false
      }
    },
    async register(email, phoneNumber, password, fullName) {
      this.loading = true
      this.error = null
      try {
        const response = await axios.post(`${API_URL}/register`, { email, phoneNumber, password, fullName })
        this.user = response.data
        localStorage.setItem('user', JSON.stringify(this.user))
        axios.defaults.headers.common['Authorization'] = `Bearer ${this.user.token}`
        return true
      } catch (err) {
        this.error = err.response?.data?.message || 'Đăng ký không thành công.'
        return false
      } finally {
        this.loading = false
      }
    },
    logout() {
      this.user = null
      localStorage.removeItem('user')
      delete axios.defaults.headers.common['Authorization']
    },
    init() {
      if (this.user?.token) {
        axios.defaults.headers.common['Authorization'] = `Bearer ${this.user.token}`
      }
    }
  }
})
