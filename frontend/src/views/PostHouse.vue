<template>
  <div class="container post-house-page animate-fade-in">
    <div class="glass-panel form-card">
      <h2 class="form-title gradient-text">Đăng Tin Cho Thuê / Bán Nhà</h2>
      <p class="form-subtitle">Điền đầy đủ thông tin bên dưới để tiếp cận hàng ngàn khách hàng tiềm năng.</p>

      <div v-if="successMessage" class="success-msg">
        🎉 {{ successMessage }}
      </div>
      <div v-if="errorMessage" class="error-msg">
        ⚠️ {{ errorMessage }}
      </div>

      <form @submit.prevent="handleSubmit">
        <div class="form-row">
          <!-- Title -->
          <div class="form-group col-12">
            <label class="form-label">Tiêu đề tin đăng *</label>
            <input v-model="form.title" type="text" class="form-input" placeholder="Ví dụ: Cho thuê căn hộ mini 1PN ban công Cầu Giấy" required />
          </div>
        </div>

        <div class="form-row grid-2">
          <!-- Type -->
          <div class="form-group">
            <label class="form-label">Hình thức *</label>
            <select v-model="form.type" class="form-input" required>
              <option value="Rent">Cho thuê</option>
              <option value="Sale">Mua bán</option>
            </select>
          </div>

          <!-- Category -->
          <div class="form-group">
            <label class="form-label">Loại bất động sản *</label>
            <select v-model="form.categoryId" class="form-input" required>
              <option value="" disabled>Chọn loại hình</option>
              <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
            </select>
          </div>
        </div>

        <div class="form-row grid-2">
          <!-- Price -->
          <div class="form-group">
            <label class="form-label">Giá (VNĐ) *</label>
            <input v-model.number="form.price" type="number" class="form-input" placeholder="Ví dụ: 3500000" required />
          </div>

          <!-- Area -->
          <div class="form-group">
            <label class="form-label">Diện tích (m²) *</label>
            <input v-model.number="form.area" type="number" class="form-input" placeholder="Ví dụ: 25" required />
          </div>
        </div>

        <div class="form-row grid-2">
          <!-- Location -->
          <div class="form-group">
            <label class="form-label">Khu vực địa lý *</label>
            <select v-model="form.locationId" class="form-input" required>
              <option value="" disabled>Chọn khu vực hành chính</option>
              <option v-for="loc in locations" :key="loc.id" :value="loc.id">
                {{ loc.type === 'Province' ? '📍 ' : '└─ ' }}{{ loc.name }}
              </option>
            </select>
          </div>

          <!-- Address -->
          <div class="form-group">
            <label class="form-label">Địa chỉ chi tiết *</label>
            <input v-model="form.address" type="text" class="form-input" placeholder="Ví dụ: Số 20 ngõ 155 Cầu Giấy" required />
          </div>
        </div>

        <!-- Description -->
        <div class="form-group">
          <label class="form-label">Mô tả chi tiết *</label>
          <textarea v-model="form.description" class="form-input form-textarea" placeholder="Nhập mô tả về phòng, tiện nghi, giờ giấc, chi phí điện nước..." rows="6" required></textarea>
        </div>

        <div class="form-row grid-2">
          <!-- Contact Name -->
          <div class="form-group">
            <label class="form-label">Họ tên người liên hệ</label>
            <input v-model="form.contactName" type="text" class="form-input" placeholder="Ví dụ: Anh Hùng" />
          </div>

          <!-- Contact Phone -->
          <div class="form-group">
            <label class="form-label">Số điện thoại liên hệ</label>
            <input v-model="form.contactPhone" type="tel" class="form-input" placeholder="Ví dụ: 0987654321" />
          </div>
        </div>

        <!-- Image Urls -->
        <div class="form-group">
          <label class="form-label">Danh sách ảnh (URL hình ảnh)</label>
          <div v-for="(url, index) in form.imageUrls" :key="index" class="image-input-row flex-center">
            <input v-model="form.imageUrls[index]" type="url" class="form-input" placeholder="Nhập link ảnh (ví dụ: https://images.unsplash.com/...)" />
            <button type="button" @click="removeImageUrl(index)" class="btn btn-outline btn-remove" :disabled="form.imageUrls.length === 1">✕</button>
          </div>
          <button type="button" @click="addImageUrl" class="btn btn-outline btn-add-img">+ Thêm ảnh</button>
        </div>

        <!-- Submit -->
        <div class="form-actions-post">
          <router-link to="/" class="btn btn-outline">Hủy bỏ</router-link>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            {{ submitting ? 'Đang đăng tin...' : 'Đăng tin ngay' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { useAuthStore } from '../stores/auth'

const API_BASE = 'http://localhost:5123/api'
const authStore = useAuthStore()
const router = useRouter()

// States
const categories = ref([])
const locations = ref([])
const successMessage = ref('')
const errorMessage = ref('')
const submitting = ref(false)

// Form State
const form = reactive({
  title: '',
  description: '',
  price: null,
  area: null,
  address: '',
  locationId: '',
  categoryId: '',
  type: 'Rent',
  contactName: authStore.user?.fullName || '',
  contactPhone: authStore.user?.phoneNumber || '',
  imageUrls: ['']
})

onMounted(() => {
  fetchFilters()
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
    console.error('Lỗi tải bộ lọc', err)
  }
}

const addImageUrl = () => {
  form.imageUrls.push('')
}

const removeImageUrl = (index) => {
  form.imageUrls.splice(index, 1)
}

const handleSubmit = async () => {
  errorMessage.value = ''
  successMessage.value = ''
  submitting.value = true

  // Validate images (lọc các url trống)
  const cleanImages = form.imageUrls.filter(url => url.trim() !== '')
  
  const payload = {
    title: form.title,
    description: form.description,
    price: form.price,
    area: form.area,
    address: form.address,
    locationId: parseInt(form.locationId),
    categoryId: parseInt(form.categoryId),
    type: form.type,
    contactName: form.contactName || null,
    contactPhone: form.contactPhone || null,
    imageUrls: cleanImages
  }

  try {
    const response = await axios.post(`${API_BASE}/posts`, payload, {
      headers: {
        Authorization: `Bearer ${authStore.token}`
      }
    })
    successMessage.value = 'Đăng tin thành công! Đang chuyển hướng...'
    setTimeout(() => {
      router.push(`/post/${response.data.id}`)
    }, 1500)
  } catch (err) {
    errorMessage.value = err.response?.data?.message || 'Đăng tin thất bại. Vui lòng kiểm tra lại thông tin.'
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.post-house-page {
  padding: 2.5rem 0;
}

.form-card {
  max-width: 800px;
  margin: 0 auto;
  padding: 3rem;
  border-radius: var(--radius-lg);
}

.form-title {
  font-size: 2rem;
  margin-bottom: 0.5rem;
}

.form-subtitle {
  color: var(--text-secondary);
  font-size: 0.95rem;
  margin-bottom: 2.5rem;
}

.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

@media (max-width: 576px) {
  .grid-2 {
    grid-template-columns: 1fr;
    gap: 0;
  }
}

.form-textarea {
  font-family: var(--font-body);
  resize: vertical;
}

/* Image URL management */
.image-input-row {
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.btn-remove {
  padding: 0.75rem;
  color: var(--status-rejected);
  border-color: rgba(239, 68, 68, 0.2);
}

.btn-remove:hover {
  background: rgba(239, 68, 68, 0.1);
  border-color: var(--status-rejected);
}

.btn-add-img {
  padding: 0.5rem 1rem;
  font-size: 0.85rem;
  margin-top: 0.5rem;
}

.form-actions-post {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 3rem;
  border-top: 1px solid var(--border-glass);
  padding-top: 1.5rem;
}

/* Alert notifications */
.success-msg {
  background: rgba(16, 185, 129, 0.15);
  border: 1px solid rgba(16, 185, 129, 0.3);
  color: #34d399;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-sm);
  font-size: 0.95rem;
  margin-bottom: 2rem;
  font-weight: 500;
}

.error-msg {
  background: rgba(239, 68, 68, 0.15);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ff6b6b;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-sm);
  font-size: 0.95rem;
  margin-bottom: 2rem;
  font-weight: 500;
}
</style>
