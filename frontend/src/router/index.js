import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/post/:id',
    name: 'Detail',
    component: () => import('../views/Detail.vue')
  },
  {
    path: '/dang-tin',
    name: 'PostHouse',
    component: () => import('../views/PostHouse.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Auth/Login.vue')
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('../views/Auth/Register.vue')
  },
  {
    path: '/admin',
    name: 'AdminDashboard',
    component: () => import('../views/Admin/Dashboard.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 }
  }
})

// Hộ vệ định tuyến (Navigation Guards)
router.beforeEach((to, from, next) => {
  const user = JSON.parse(localStorage.getItem('user'))
  const isAuthenticated = !!user && !!user.token

  if (to.meta.requiresAuth && !isAuthenticated) {
    next({ name: 'Login', query: { redirect: to.fullPath } })
  } else if (to.meta.requiresAdmin && (!isAuthenticated || user.role !== 'Admin')) {
    next({ name: 'Home' })
  } else {
    next()
  }
})

export default router
