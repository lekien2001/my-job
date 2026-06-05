<template>
  <div class="container detail-page animate-fade-in">
    <!-- Back Button -->
    <div class="back-nav">
      <router-link to="/" class="btn btn-outline btn-sm">← Quay lại danh sách</router-link>
    </div>

    <!-- Error/Loading states -->
    <div v-if="loading" class="loading-state flex-center">
      <div class="loader"></div>
      <p>Đang tải chi tiết tin đăng...</p>
    </div>

    <div v-else-if="error" class="error-state glass-card flex-center">
      <span class="error-icon">❌</span>
      <h3>Đã xảy ra lỗi</h3>
      <p>{{ error }}</p>
      <router-link to="/" class="btn btn-primary">Quay lại trang chủ</router-link>
    </div>

    <!-- Main Content -->
    <div v-else-if="post" class="detail-container">
      <!-- Left side: Image gallery & details -->
      <div class="detail-main">
        <!-- Main Image display -->
        <div class="gallery-wrapper glass-card">
          <div class="active-image-box">
            <img :src="activeImage" alt="Room Gallery" class="active-image" />
          </div>
          <!-- Thumbnail list if multiple images -->
          <div v-if="post.imageUrls && post.imageUrls.length > 1" class="thumbnail-list">
            <img v-for="(img, idx) in post.imageUrls" :key="idx" :src="img" alt="Thumb" 
                 class="thumbnail-img" :class="{ active: activeImage === img }" @click="activeImage = img" />
          </div>
        </div>

        <!-- Details Card -->
        <div class="info-card glass-panel">
          <div class="card-meta">
            <span class="badge badge-type" :class="post.type">{{ post.type === 'Rent' ? 'Cho thuê' : 'Mua bán' }}</span>
            <span class="badge badge-source" :class="post.source">{{ post.source }}</span>
            <span class="category-tag">{{ post.categoryName }}</span>
          </div>

          <h1 class="post-title">{{ post.title }}</h1>
          <p class="post-address">📍 {{ post.address }} ({{ post.locationName }})</p>

          <div class="specs-grid">
            <div class="spec-item">
              <span class="spec-label">Mức giá</span>
              <span class="spec-value highlight">{{ formatPrice(post.price, post.type) }}</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Diện tích</span>
              <span class="spec-value">{{ post.area }} m²</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Ngày cập nhật</span>
              <span class="spec-value">{{ formatDate(post.createdAt) }}</span>
            </div>
          </div>

          <div class="description-section">
            <h3 class="section-subtitle">Mô tả chi tiết</h3>
            <p class="description-text">{{ post.description }}</p>
          </div>
        </div>
      </div>

      <!-- Right side: Contact box -->
      <div class="detail-sidebar">
        <div class="contact-card glass-panel">
          <h3 class="sidebar-title">Thông tin liên hệ</h3>
          
          <!-- Owner info -->
          <div class="owner-info flex-center">
            <img :src="post.userAvatarUrl || 'https://api.dicebear.com/7.x/adventurer/svg?seed=owner'" alt="Owner Avatar" class="owner-avatar" />
            <div class="owner-details">
              <h4 class="owner-name">{{ post.contactName || post.userFullName || 'Chính chủ' }}</h4>
              <span class="owner-role">{{ post.source === 'Facebook' ? 'Thành viên Facebook' : 'Thành viên Web' }}</span>
            </div>
          </div>

          <!-- Direct Actions -->
          <div class="action-buttons">
            <a v-if="post.contactPhone" :href="`tel:${post.contactPhone}`" class="btn btn-primary btn-block btn-phone">
              📞 Gọi điện: {{ post.contactPhone }}
            </a>
            <a v-if="post.contactPhone" :href="`sms:${post.contactPhone}`" class="btn btn-outline btn-block">
              💬 Gửi tin nhắn SMS
            </a>
            <a v-if="post.source === 'Facebook' && post.externalUrl" :href="post.externalUrl" target="_blank" class="btn btn-secondary btn-block">
              🔗 Xem bài đăng Facebook gốc
            </a>
          </div>

          <div class="safety-tip">
            <p class="tip-title">⚠️ Lưu ý an toàn:</p>
            <p class="tip-text">Không đặt cọc tiền khi chưa xem nhà trực tiếp và xác thực giấy tờ chủ nhà.</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'

const API_BASE = 'http://localhost:5123/api'
const route = useRoute()

// States
const post = ref(null)
const loading = ref(true)
const error = ref(null)
const activeImage = ref('')

onMounted(async () => {
  const id = route.params.id
  try {
    const res = await axios.get(`${API_BASE}/posts/${id}`)
    post.value = res.data
    if (post.value.imageUrls && post.value.imageUrls.length > 0) {
      activeImage.value = post.value.imageUrls[0]
    } else {
      activeImage.value = 'https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?auto=format&fit=crop&w=800&q=80'
    }
  } catch (err) {
    error.value = err.response?.data?.message || 'Không thể tải thông tin tin đăng này.'
  } finally {
    loading.value = false
  }
})

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
  return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
}
</script>

<style scoped>
.detail-page {
  padding-top: 1rem;
  padding-bottom: 4rem;
}

.back-nav {
  margin-bottom: 1.5rem;
}

.detail-container {
  display: grid;
  grid-template-columns: 8fr 4fr;
  gap: 2rem;
}

@media (max-width: 992px) {
  .detail-container {
    grid-template-columns: 1fr;
  }
}

/* Gallery styling */
.gallery-wrapper {
  padding: 1rem;
  border-radius: var(--radius-md);
  margin-bottom: 2rem;
}

.active-image-box {
  width: 100%;
  height: 450px;
  border-radius: var(--radius-sm);
  overflow: hidden;
}

@media (max-width: 768px) {
  .active-image-box {
    height: 280px;
  }
}

.active-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.thumbnail-list {
  display: flex;
  gap: 0.75rem;
  margin-top: 1rem;
  overflow-x: auto;
  padding-bottom: 0.5rem;
}

.thumbnail-img {
  width: 80px;
  height: 60px;
  object-fit: cover;
  border-radius: 4px;
  cursor: pointer;
  border: 2px solid transparent;
  transition: var(--transition-fast);
}

.thumbnail-img.active, .thumbnail-img:hover {
  border-color: var(--primary);
  transform: scale(1.05);
}

/* Info Card */
.info-card {
  padding: 2rem;
  border-radius: var(--radius-md);
}

.card-meta {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1rem;
}

.category-tag {
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--secondary);
  text-transform: uppercase;
  margin-left: auto;
}

.badge {
  position: static;
}

.badge-type.Rent {
  background-color: var(--primary);
  color: #fff;
}

.badge-type.Sale {
  background-color: var(--secondary);
  color: #fff;
}

.badge-source.Web {
  background-color: var(--status-active);
  color: #fff;
}

.badge-source.Facebook {
  background-color: #1877f2;
  color: #fff;
}

.post-title {
  font-size: 1.8rem;
  margin-bottom: 0.75rem;
  line-height: 1.35;
}

.post-address {
  font-size: 0.95rem;
  color: var(--text-secondary);
  margin-bottom: 2rem;
}

/* Specs */
.specs-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.5rem;
  border-top: 1px solid var(--border-glass);
  border-bottom: 1px solid var(--border-glass);
  padding: 1.5rem 0;
  margin-bottom: 2rem;
}

@media (max-width: 576px) {
  .specs-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
}

.spec-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.spec-label {
  font-size: 0.85rem;
  color: var(--text-muted);
}

.spec-value {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--text-primary);
}

.spec-value.highlight {
  color: var(--primary);
  font-size: 1.5rem;
}

/* Description */
.description-text {
  font-size: 0.98rem;
  line-height: 1.7;
  color: var(--text-secondary);
  white-space: pre-wrap; /* Quan trọng để giữ định dạng xuống hàng */
  background: rgba(15, 23, 42, 0.2);
  padding: 1.25rem;
  border-radius: var(--radius-sm);
  border-left: 3px solid var(--primary);
}

.section-subtitle {
  font-size: 1.15rem;
  margin-bottom: 1rem;
}

/* Sidebar Contact Card */
.contact-card {
  padding: 1.5rem;
  border-radius: var(--radius-md);
  position: sticky;
  top: 90px;
}

.sidebar-title {
  font-size: 1.2rem;
  margin-bottom: 1.25rem;
}

.owner-info {
  justify-content: flex-start;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.owner-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  border: 2px solid var(--border-glass);
}

.owner-name {
  font-size: 1.05rem;
  font-weight: 600;
}

.owner-role {
  font-size: 0.8rem;
  color: var(--text-muted);
}

.action-buttons {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
}

.btn-block {
  width: 100%;
}

.btn-phone {
  background-color: var(--status-active);
}

.btn-phone:hover {
  background-color: #059669;
}

.safety-tip {
  background: rgba(239, 68, 68, 0.08);
  border: 1px solid rgba(239, 68, 68, 0.2);
  padding: 0.75rem;
  border-radius: 6px;
}

.tip-title {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--status-rejected);
  margin-bottom: 0.25rem;
}

.tip-text {
  font-size: 0.8rem;
  color: var(--text-secondary);
}

/* Loader & Error */
.loading-state {
  flex-direction: column;
  padding: 8rem 0;
  gap: 1rem;
}

.error-state {
  flex-direction: column;
  padding: 4rem;
  gap: 1rem;
  text-align: center;
}

.error-icon {
  font-size: 3rem;
}
</style>
