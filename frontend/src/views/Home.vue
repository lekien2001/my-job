<template>
  <div class="container home-page animate-fade-in">
    <!-- Hero Banner & Search Bar -->
    <section class="hero-section">
      <h1 class="hero-title">Tìm Kiếm <span class="gradient-text">Nơi An Cư</span> Lý Tưởng</h1>
      <p class="hero-subtitle">Hệ thống tổng hợp tin đăng cho thuê, mua bán nhà đất từ người dùng và mạng xã hội.</p>

      <!-- Advanced Search Panel -->
      <div class="search-panel glass-panel">
        <div class="search-row">
          <!-- Keyword Search -->
          <div class="search-col keyword-col">
            <label class="form-label">Từ khóa tìm kiếm</label>
            <div class="input-wrapper">
              <span class="input-icon">🔍</span>
              <input v-model="filters.keyword" type="text" class="form-input" placeholder="Nhập địa chỉ, từ khóa cần tìm..." @keyup.enter="handleSearch" />
            </div>
          </div>

          <!-- Transaction Type -->
          <div class="search-col">
            <label class="form-label">Hình thức</label>
            <select v-model="filters.type" class="form-input" @change="handleSearch">
              <option value="">Tất cả</option>
              <option value="Rent">Cho thuê</option>
              <option value="Sale">Mua bán</option>
            </select>
          </div>

          <!-- Category -->
          <div class="search-col">
            <label class="form-label">Loại bất động sản</label>
            <select v-model="filters.categoryId" class="form-input" @change="handleSearch">
              <option value="">Tất cả danh mục</option>
              <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
            </select>
          </div>

          <!-- Location -->
          <div class="search-col">
            <label class="form-label">Khu vực</label>
            <select v-model="filters.locationId" class="form-input" @change="handleSearch">
              <option value="">Tất cả khu vực</option>
              <option v-for="loc in locations" :key="loc.id" :value="loc.id">
                {{ loc.type === 'Province' ? '📍 ' : '└─ ' }}{{ loc.name }}
              </option>
            </select>
          </div>
        </div>

        <div class="search-row extra-row" v-if="showAdvanced">
          <!-- Min/Max Price -->
          <div class="search-col">
            <label class="form-label">Giá tối thiểu (VNĐ)</label>
            <input v-model.number="filters.minPrice" type="number" class="form-input" placeholder="Ví dụ: 1000000" />
          </div>
          <div class="search-col">
            <label class="form-label">Giá tối đa (VNĐ)</label>
            <input v-model.number="filters.maxPrice" type="number" class="form-input" placeholder="Ví dụ: 10000000" />
          </div>

          <!-- Min/Max Area -->
          <div class="search-col">
            <label class="form-label">Diện tích tối thiểu (m²)</label>
            <input v-model.number="filters.minArea" type="number" class="form-input" placeholder="Ví dụ: 20" />
          </div>
          <div class="search-col">
            <label class="form-label">Nguồn tin</label>
            <select v-model="filters.source" class="form-input" @change="handleSearch">
              <option value="">Tất cả nguồn</option>
              <option value="Web">Website chính chủ</option>
              <option value="Facebook">Cào từ Facebook</option>
            </select>
          </div>
        </div>

        <div class="panel-actions">
          <button @click="showAdvanced = !showAdvanced" class="btn btn-outline">
            {{ showAdvanced ? 'Thu gọn bộ lọc' : 'Lọc nâng cao' }}
          </button>
          <button @click="handleReset" class="btn btn-outline">Làm mới</button>
          <button @click="handleSearch" class="btn btn-primary">Tìm kiếm ngay</button>
        </div>
      </div>
    </section>

    <!-- Listings & Sort Header -->
    <section class="listings-section">
      <div class="listings-header">
        <h2 class="section-title">Danh Sách Tin Đăng ({{ totalCount }})</h2>
        
        <div class="sort-box">
          <span class="sort-label">Sắp xếp:</span>
          <select v-model="filters.sortBy" class="form-input sort-select" @change="handleSearch">
            <option value="newest">Mới nhất</option>
            <option value="price_asc">Giá tăng dần</option>
            <option value="price_desc">Giá giảm dần</option>
            <option value="area_asc">Diện tích tăng dần</option>
            <option value="area_desc">Diện tích giảm dần</option>
          </select>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="loading-state flex-center">
        <div class="loader"></div>
        <p>Đang tải dữ liệu...</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="posts.length === 0" class="empty-state flex-center glass-card">
        <span class="empty-icon">😢</span>
        <h3>Không tìm thấy tin đăng phù hợp</h3>
        <p>Thử thay đổi bộ lọc tìm kiếm hoặc từ khóa của bạn.</p>
        <button @click="handleReset" class="btn btn-primary">Reset bộ lọc</button>
      </div>

      <!-- Grid Cards -->
      <div v-else class="grid-responsive posts-grid">
        <div v-for="post in posts" :key="post.id" class="glass-card post-card">
          <!-- Thumbnail & Badges -->
          <div class="card-image-wrapper">
            <img :src="post.thumbnailUrl || 'https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?auto=format&fit=crop&w=800&q=80'" alt="Room Thumbnail" class="card-image" />
            <span class="badge badge-type" :class="post.type">{{ post.type === 'Rent' ? 'Cho thuê' : 'Mua bán' }}</span>
            <span class="badge badge-source" :class="post.source">{{ post.source }}</span>
          </div>

          <!-- Card Content -->
          <div class="card-content">
            <div class="card-category">{{ post.categoryName }}</div>
            <h3 class="card-title" :title="post.title">{{ post.title }}</h3>
            
            <div class="card-metrics">
              <span class="metric-price">{{ formatPrice(post.price, post.type) }}</span>
              <span class="metric-separator">•</span>
              <span class="metric-area">{{ post.area }} m²</span>
            </div>

            <p class="card-location">📍 {{ post.address }} ({{ post.locationName }})</p>
            
            <div class="card-footer">
              <span class="card-time">📅 {{ formatDate(post.createdAt) }}</span>
              <router-link :to="`/post/${post.id}`" class="btn btn-primary btn-sm btn-view">Xem chi tiết</router-link>
            </div>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="pagination flex-center">
        <button @click="changePage(filters.page - 1)" :disabled="filters.page === 1" class="btn btn-outline page-btn">« Trước</button>
        <span class="page-info">Trang {{ filters.page }} / {{ totalPages }}</span>
        <button @click="changePage(filters.page + 1)" :disabled="filters.page === totalPages" class="btn btn-outline page-btn">Sau »</button>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import axios from 'axios'

const API_BASE = 'http://localhost:5123/api'

// States
const posts = ref([])
const categories = ref([])
const locations = ref([])
const totalCount = ref(0)
const totalPages = ref(1)
const loading = ref(false)
const showAdvanced = ref(false)

// Query Filters
const filters = reactive({
  page: 1,
  pageSize: 8,
  keyword: '',
  minPrice: null,
  maxPrice: null,
  minArea: null,
  maxArea: null,
  locationId: '',
  categoryId: '',
  type: '',
  source: '',
  sortBy: 'newest'
})

onMounted(() => {
  fetchFilters()
  fetchPosts()
})

const fetchFilters = async () => {
  try {
    const [catRes, locRes] = await Promise.all([
      axios.get(`${API_BASE}/categories`),
      axios.get(`${API_BASE}/locations`)
    ])
    categories.value = catRes.data
    locations.value = locRes.data
  } catch (err) {
    console.error('Lỗi khi tải bộ lọc', err)
  }
}

const fetchPosts = async () => {
  loading.value = true
  try {
    // Clean up empty params
    const params = {}
    for (const key in filters) {
      if (filters[key] !== '' && filters[key] !== null && filters[key] !== undefined) {
        params[key] = filters[key]
      }
    }

    const response = await axios.get(`${API_BASE}/posts`, { params })
    posts.value = response.data.items
    totalCount.value = response.data.totalCount
    totalPages.value = Math.ceil(totalCount.value / filters.pageSize)
  } catch (err) {
    console.error('Lỗi khi tải danh sách tin', err)
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  filters.page = 1
  fetchPosts()
}

const handleReset = () => {
  filters.page = 1
  filters.keyword = ''
  filters.minPrice = null
  filters.maxPrice = null
  filters.minArea = null
  filters.maxArea = null
  filters.locationId = ''
  filters.categoryId = ''
  filters.type = ''
  filters.source = ''
  filters.sortBy = 'newest'
  fetchPosts()
}

const changePage = (page) => {
  if (page < 1 || page > totalPages.value) return
  filters.page = page
  fetchPosts()
  window.scrollTo({ top: 400, behavior: 'smooth' })
}

// Helpers
const formatPrice = (price, type) => {
  if (price >= 1000000000) {
    return (price / 1000000000).toFixed(1) + ' tỷ' + (type === 'Rent' ? '/tháng' : '')
  }
  if (price >= 1000000) {
    return (price / 1000000).toFixed(1) + ' triệu' + (type === 'Rent' ? '/tháng' : '')
  }
  return price.toLocaleString('vi-VN') + ' đ' + (type === 'Rent' ? '/tháng' : '')
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleDateString('vi-VN')
}
</script>

<style scoped>
.home-page {
  padding-bottom: 4rem;
}

.hero-section {
  text-align: center;
  padding: 3rem 0;
}

.hero-title {
  font-size: 2.75rem;
  font-weight: 800;
  margin-bottom: 1rem;
}

.hero-subtitle {
  color: var(--text-secondary);
  font-size: 1.1rem;
  max-width: 600px;
  margin: 0 auto 2.5rem auto;
}

/* Search Panel */
.search-panel {
  padding: 2rem;
  border-radius: var(--radius-lg);
  max-width: 1000px;
  margin: 0 auto;
  text-align: left;
}

.search-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.search-row.extra-row {
  border-top: 1px solid var(--border-glass);
  padding-top: 1.25rem;
  margin-top: 1rem;
  animation: fadeIn 0.3s ease;
}

.keyword-col {
  grid-column: span 2;
}

@media (max-width: 768px) {
  .keyword-col {
    grid-column: span 1;
  }
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 12px;
  color: var(--text-muted);
}

.input-wrapper .form-input {
  padding-left: 2.25rem;
}

.panel-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1.5rem;
}

/* Listings Section */
.listings-section {
  margin-top: 4rem;
}

.listings-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  border-bottom: 1px solid var(--border-glass);
  padding-bottom: 1rem;
}

.section-title {
  font-size: 1.5rem;
}

.sort-box {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.sort-label {
  font-size: 0.9rem;
  color: var(--text-secondary);
  white-space: nowrap;
}

.sort-select {
  padding: 0.4rem 1rem;
  font-size: 0.9rem;
}

/* Card layout */
.posts-grid {
  margin-bottom: 3rem;
}

.post-card {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  height: 100%;
}

.card-image-wrapper {
  position: relative;
  width: 100%;
  height: 200px;
  overflow: hidden;
}

.card-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: var(--transition-normal);
}

.post-card:hover .card-image {
  transform: scale(1.08);
}

.badge {
  position: absolute;
  padding: 0.25rem 0.65rem;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
}

.badge-type {
  top: 10px;
  left: 10px;
}

.badge-type.Rent {
  background-color: var(--primary);
  color: #fff;
}

.badge-type.Sale {
  background-color: var(--secondary);
  color: #fff;
}

.badge-source {
  top: 10px;
  right: 10px;
}

.badge-source.Web {
  background-color: var(--status-active);
  color: #fff;
}

.badge-source.Facebook {
  background-color: #1877f2; /* Facebook Blue */
  color: #fff;
}

.card-content {
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  flex-grow: 1;
}

.card-category {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--secondary);
  text-transform: uppercase;
  margin-bottom: 0.5rem;
}

.card-title {
  font-size: 1.05rem;
  line-height: 1.4;
  margin-bottom: 0.75rem;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  height: 2.8rem;
}

.card-metrics {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}

.metric-price {
  font-size: 1.25rem;
  font-weight: 800;
  color: var(--primary);
}

.metric-separator {
  color: var(--text-muted);
}

.metric-area {
  font-weight: 600;
  color: var(--text-secondary);
}

.card-location {
  font-size: 0.85rem;
  color: var(--text-secondary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-bottom: 1.25rem;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  border-top: 1px solid var(--border-glass);
  padding-top: 0.75rem;
}

.card-time {
  font-size: 0.8rem;
  color: var(--text-muted);
}

.btn-view {
  padding: 0.35rem 0.75rem;
  font-size: 0.8rem;
}

/* Loading & Empty state */
.loading-state {
  flex-direction: column;
  padding: 5rem 0;
  gap: 1rem;
  color: var(--text-secondary);
}

.loader {
  border: 4px solid var(--border-glass);
  border-top: 4px solid var(--primary);
  border-radius: 50%;
  width: 40px;
  height: 40px;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.empty-state {
  flex-direction: column;
  padding: 4rem;
  text-align: center;
  gap: 0.75rem;
}

.empty-icon {
  font-size: 3rem;
}

.empty-state p {
  color: var(--text-secondary);
  margin-bottom: 1rem;
}

/* Pagination */
.pagination {
  gap: 1.5rem;
  margin-top: 2rem;
}

.page-info {
  font-size: 0.95rem;
  font-weight: 600;
}

.page-btn {
  padding: 0.5rem 1rem;
}
</style>
