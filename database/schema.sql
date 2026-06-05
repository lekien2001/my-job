-- Khởi tạo Database schema cho dự án Real Estate Platform
-- Hệ quản trị cơ sở dữ liệu: MySQL

CREATE DATABASE IF NOT EXISTS `real_estate_db` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `real_estate_db`;

-- 1. Bảng loại hình bất động sản (Categories)
CREATE TABLE IF NOT EXISTS `categories` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL COMMENT 'Tên loại hình (ví dụ: Phòng trọ, Căn hộ, Nhà nguyên căn, Đất nền...)',
    `slug` VARCHAR(100) NOT NULL UNIQUE COMMENT 'Đường dẫn thân thiện (ví dụ: phong-tro, can-ho)',
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. Bảng vị trí hành chính (Locations) - Hỗ trợ cây phân cấp Tỉnh/Huyện/Xã
CREATE TABLE IF NOT EXISTS `locations` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(150) NOT NULL COMMENT 'Tên khu vực (ví dụ: TP. Hồ Chí Minh, Quận 1, Phường Bến Nghé)',
    `slug` VARCHAR(150) NOT NULL UNIQUE,
    `parent_id` INT NULL COMMENT 'ID của khu vực cha (ví dụ: Quận 1 có parent là TP. HCM)',
    `type` VARCHAR(50) NOT NULL COMMENT 'Cấp bậc: Province (Tỉnh), District (Huyện), Ward (Xã)',
    CONSTRAINT `fk_locations_parent` FOREIGN KEY (`parent_id`) REFERENCES `locations` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. Bảng Người dùng (Users) - Hỗ trợ đăng nhập bằng cả Email và Số điện thoại
CREATE TABLE IF NOT EXISTS `users` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `email` VARCHAR(100) NULL UNIQUE COMMENT 'Địa chỉ email đăng nhập',
    `phone_number` VARCHAR(20) NULL UNIQUE COMMENT 'Số điện thoại đăng nhập/liên hệ',
    `password_hash` VARCHAR(255) NULL COMMENT 'Mật khẩu đã được mã hóa (null nếu chỉ đăng nhập qua OTP)',
    `full_name` VARCHAR(100) NOT NULL COMMENT 'Họ và tên hiển thị',
    `avatar_url` VARCHAR(500) NULL,
    `role` VARCHAR(20) NOT NULL DEFAULT 'User' COMMENT 'Quyền hạn: Admin, User',
    `status` VARCHAR(20) NOT NULL DEFAULT 'Active' COMMENT 'Trạng thái: Active, Locked, Pending',
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4. Bảng Tin đăng (Posts) - Dùng chung cho cả Tin người dùng đăng và Tin cào từ Facebook
CREATE TABLE IF NOT EXISTS `posts` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `title` VARCHAR(255) NOT NULL COMMENT 'Tiêu đề tin đăng',
    `description` TEXT NOT NULL COMMENT 'Mô tả chi tiết nội dung nhà đất',
    `price` DECIMAL(15, 2) NOT NULL COMMENT 'Giá (nếu thuê: đồng/tháng, nếu bán: tổng giá trị)',
    `area` DECIMAL(8, 2) NOT NULL COMMENT 'Diện tích (m2)',
    `address` VARCHAR(255) NOT NULL COMMENT 'Địa chỉ chi tiết hiển thị',
    `location_id` INT NOT NULL COMMENT 'Khu vực địa lý chi tiết (Xã/Phường/Quận/Huyện)',
    `category_id` INT NOT NULL COMMENT 'Loại hình bất động sản',
    `type` VARCHAR(20) NOT NULL COMMENT 'Hình thức: Rent (Cho thuê), Sale (Bán)',
    `user_id` INT NULL COMMENT 'Người đăng tin (NULL nếu cào từ Facebook)',
    
    -- Các trường phục vụ cho Crawler Facebook
    `source` VARCHAR(50) NOT NULL DEFAULT 'Web' COMMENT 'Nguồn tin: Web (người dùng tự đăng), Facebook',
    `external_id` VARCHAR(100) NULL UNIQUE COMMENT 'ID bài viết gốc trên Facebook (để tránh cào trùng)',
    `external_url` VARCHAR(500) NULL COMMENT 'Link liên kết bài viết gốc trên Facebook',
    
    -- Thông tin liên hệ trực tiếp (đặc biệt hữu ích cho tin cào từ Facebook)
    `contact_name` VARCHAR(100) NULL COMMENT 'Tên người liên hệ',
    `contact_phone` VARCHAR(20) NULL COMMENT 'Số điện thoại liên hệ',
    
    `status` VARCHAR(20) NOT NULL DEFAULT 'Active' COMMENT 'Trạng thái hiển thị: Pending, Active, Rejected, Hidden',
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    CONSTRAINT `fk_posts_location` FOREIGN KEY (`location_id`) REFERENCES `locations` (`id`),
    CONSTRAINT `fk_posts_category` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`),
    CONSTRAINT `fk_posts_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 5. Bảng hình ảnh tin đăng (Post Images)
CREATE TABLE IF NOT EXISTS `post_images` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `post_id` INT NOT NULL COMMENT 'ID tin đăng liên kết',
    `image_url` VARCHAR(500) NOT NULL COMMENT 'URL hình ảnh (local storage hoặc link cào facebook)',
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_images_post` FOREIGN KEY (`post_id`) REFERENCES `posts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- THIẾT LẬP CÁC CHỈ MỤC (INDEXES) TỐI ƯU HOÁ TÌM KIẾM
-- Tăng tốc độ lọc theo khu vực, loại hình, giá và diện tích
CREATE INDEX `idx_posts_search` ON `posts` (`status`, `type`, `location_id`, `category_id`);
CREATE INDEX `idx_posts_price` ON `posts` (`price`);
CREATE INDEX `idx_posts_area` ON `posts` (`area`);
CREATE INDEX `idx_posts_created_at` ON `posts` (`created_at` DESC);
