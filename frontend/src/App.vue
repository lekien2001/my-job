<template>
  <div class="app-layout">
    <!-- Navigation Header -->
    <header class="navbar glass-panel">
      <div class="container nav-container">
        <router-link to="/" class="logo-area">
          <span class="logo-icon gradient-bg">🏠</span>
          <span class="logo-text gradient-text">RealEstate</span>
        </router-link>

        <nav class="nav-links">
          <router-link to="/" class="nav-link" active-class="active">Tìm kiếm nhà</router-link>
          <router-link to="/dang-tin" class="nav-link" active-class="active">Đăng tin</router-link>
          <router-link v-if="authStore.isAdmin" to="/admin" class="nav-link admin-link" active-class="active">⚙️ Quản trị</router-link>
        </nav>

        <div class="nav-actions">
          <!-- Theme Toggle Button -->
          <button @click="toggleTheme" class="theme-btn" :title="theme === 'dark' ? 'Chuyển sang chế độ sáng' : 'Chuyển sang chế độ tối'">
            {{ theme === 'dark' ? '☀️' : '🌙' }}
          </button>

          <!-- Authentication actions -->
          <div v-if="authStore.isAuthenticated" class="user-menu">
            <span class="user-name">Xin chào, {{ authStore.user?.fullName }}</span>
            <button @click="handleLogout" class="btn btn-outline btn-sm">Đăng xuất</button>
          </div>
          <div v-else class="auth-btns">
            <router-link to="/login" class="btn btn-outline btn-sm">Đăng nhập</router-link>
            <router-link to="/register" class="btn btn-primary btn-sm">Đăng ký</router-link>
          </div>
        </div>
      </div>
    </header>

    <!-- Main Content Area -->
    <main class="main-content">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>

    <!-- Footer -->
    <footer class="footer">
      <div class="container footer-container">
        <p class="copyright">&copy; 2026 RealEstate Platform. Xây dựng bằng Vue 3 + .NET Core API.</p>
        <div class="footer-links">
          <a href="#" class="footer-link">Điều khoản</a>
          <a href="#" class="footer-link">Bảo mật</a>
          <a href="#" class="footer-link">Liên hệ</a>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from './stores/auth'

const authStore = useAuthStore()
const router = useRouter()
const theme = ref('dark')

onMounted(() => {
  // Đồng bộ theme ban đầu
  const savedTheme = localStorage.getItem('theme') || 'dark'
  theme.value = savedTheme
  document.documentElement.setAttribute('data-theme', savedTheme)
})

const toggleTheme = () => {
  const newTheme = theme.value === 'dark' ? 'light' : 'dark'
  theme.value = newTheme
  localStorage.setItem('theme', newTheme)
  document.documentElement.setAttribute('data-theme', newTheme)
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.app-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.navbar {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 70px;
  z-index: 100;
  display: flex;
  align-items: center;
}

.nav-container {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.logo-area {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-family: var(--font-heading);
  font-size: 1.5rem;
  font-weight: 800;
}

.logo-icon {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
}

.nav-links {
  display: flex;
  gap: 1.5rem;
}

.nav-link {
  font-weight: 500;
  font-size: 0.95rem;
  color: var(--text-secondary);
  padding: 0.5rem 0;
  position: relative;
}

.nav-link:hover, .nav-link.active {
  color: var(--text-primary);
}

.nav-link.active::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 2px;
  background: linear-gradient(135deg, var(--primary) 0%, var(--secondary) 100%);
  border-radius: 2px;
}

.admin-link {
  color: var(--secondary);
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 1.25rem;
}

.theme-btn {
  background: transparent;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 50%;
  transition: var(--transition-fast);
}

.theme-btn:hover {
  transform: scale(1.15);
}

.user-menu {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-name {
  font-size: 0.9rem;
  font-weight: 500;
  color: var(--text-secondary);
}

.auth-btns {
  display: flex;
  gap: 0.75rem;
}

.btn-sm {
  padding: 0.4rem 0.85rem;
  font-size: 0.85rem;
}

.main-content {
  flex-grow: 1;
  margin-top: 70px;
  padding: 2rem 0;
}

.footer {
  border-top: 1px solid var(--border-glass);
  padding: 1.5rem 0;
  background: rgba(15, 23, 42, 0.2);
}

.footer-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
}

.copyright {
  font-size: 0.85rem;
  color: var(--text-muted);
}

.footer-links {
  display: flex;
  gap: 1.5rem;
}

.footer-link {
  font-size: 0.85rem;
  color: var(--text-muted);
}

.footer-link:hover {
  color: var(--text-secondary);
}

/* Page transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.25s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

@media (max-width: 768px) {
  .nav-links {
    display: none; /* Mobile menu can be added if needed, kept simple */
  }
}
</style>
