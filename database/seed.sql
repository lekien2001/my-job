-- Dữ liệu mẫu ban đầu cho dự án Real Estate Platform
USE `real_estate_db`;

-- 1. Insert Categories
INSERT INTO `categories` (`id`, `name`, `slug`) VALUES
(1, 'Phòng trọ, Nhà trọ', 'phong-tro-nha-tro'),
(2, 'Nhà nguyên căn', 'nha-nguyen-can'),
(3, 'Căn hộ chung cư', 'can-ho-chung-cu'),
(4, 'Nhà mặt phố', 'nha-mat-pho'),
(5, 'Đất nền, Đất thổ cư', 'dat-nen-dat-tho-cu');

-- 2. Insert Locations (Cây phân cấp Tỉnh/Thành -> Quận/Huyện)
-- Tỉnh/Thành phố (Cấp 1)
INSERT INTO `locations` (`id`, `name`, `slug`, `parent_id`, `type`) VALUES
(1, 'TP. Hồ Chí Minh', 'tp-ho-chi-minh', NULL, 'Province'),
(2, 'Hà Nội', 'ha-noi', NULL, 'Province'),
(3, 'Đà Nẵng', 'da-nang', NULL, 'Province');

-- Quận/Huyện tại TP. Hồ Chí Minh (Cấp 2)
INSERT INTO `locations` (`id`, `name`, `slug`, `parent_id`, `type`) VALUES
(4, 'Quận 1', 'quan-1', 1, 'District'),
(5, 'Quận Bình Thạnh', 'quan-binh-thanh', 1, 'District'),
(6, 'Thành phố Thủ Đức', 'thanh-pho-thu-duc', 1, 'District'),
(7, 'Quận 7', 'quan-7', 1, 'District');

-- Quận/Huyện tại Hà Nội (Cấp 2)
INSERT INTO `locations` (`id`, `name`, `slug`, `parent_id`, `type`) VALUES
(8, 'Quận Cầu Giấy', 'quan-cau-giay', 2, 'District'),
(9, 'Quận Đống Đa', 'quan-dong-da', 2, 'District'),
(10, 'Quận Nam Từ Liêm', 'quan-nam-tu-liem', 2, 'District');

-- Phường/Xã tại Quận 1, TP. HCM (Cấp 3)
INSERT INTO `locations` (`id`, `name`, `slug`, `parent_id`, `type`) VALUES
(11, 'Phường Bến Nghé', 'phuong-ben-nghe', 4, 'Ward'),
(12, 'Phường Bến Thành', 'phuong-ben-thanh', 4, 'Ward'),
(13, 'Phường Phạm Ngũ Lão', 'phuong-pham-ngu-lao', 4, 'Ward');

-- Phường/Xã tại Quận Cầu Giấy, Hà Nội (Cấp 3)
INSERT INTO `locations` (`id`, `name`, `slug`, `parent_id`, `type`) VALUES
(14, 'Phường Dịch Vọng', 'phuong-dich-vong', 8, 'Ward'),
(15, 'Phường Mai Dịch', 'phuong-mai-dich', 8, 'Ward');


-- 3. Insert Người dùng mẫu (Mật khẩu mặc định là '123456' đã được băm bằng BCrypt - dùng tạm để test)
-- password_hash là bcrypt của 'password123'
INSERT INTO `users` (`id`, `email`, `phone_number`, `password_hash`, `full_name`, `avatar_url`, `role`, `status`) VALUES
(1, 'admin@realestate.com', '0999999999', '$2a$12$R.S/wZf.Fv2G3l9rJqXm3uH6qK3x6f8H3m9c4E7z5y2j6w7h3g1eS', 'Hệ Thống Admin', 'https://api.dicebear.com/7.x/adventurer/svg?seed=admin', 'Admin', 'Active'),
(2, 'nguyenvanan@gmail.com', '0912345678', '$2a$12$R.S/wZf.Fv2G3l9rJqXm3uH6qK3x6f8H3m9c4E7z5y2j6w7h3g1eS', 'Nguyễn Văn An', 'https://api.dicebear.com/7.x/adventurer/svg?seed=an', 'User', 'Active');


-- 4. Insert Tin đăng mẫu
-- Tin đăng từ người dùng tự đăng trên Web
INSERT INTO `posts` (`id`, `title`, `description`, `price`, `area`, `address`, `location_id`, `category_id`, `type`, `user_id`, `source`, `contact_name`, `contact_phone`, `status`) VALUES
(1, 'Cho thuê phòng trọ cao cấp Quận 1 gần hồ Con Rùa', 'Phòng trọ rộng rãi sạch sẽ, đầy đủ tiện nghi điều hòa, tủ lạnh, giường tủ gỗ cao cấp. Có ban công thoáng mát, chỗ để xe rộng rãi an ninh. Giờ giấc tự do không chung chủ.', 4500000.00, 25.50, 'Số 15/4 Phạm Ngọc Thạch, Phường Bến Nghé, Quận 1', 11, 1, 'Rent', 2, 'Web', 'Anh An', '0912345678', 'Active');

-- Hình ảnh tin đăng mẫu 1
INSERT INTO `post_images` (`post_id`, `image_url`) VALUES
(1, 'https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?auto=format&fit=crop&w=800&q=80'),
(1, 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=800&q=80');

-- Tin đăng cào về từ Facebook
INSERT INTO `posts` (`id`, `title`, `description`, `price`, `area`, `address`, `location_id`, `category_id`, `type`, `user_id`, `source`, `external_id`, `external_url`, `contact_name`, `contact_phone`, `status`) VALUES
(2, '[FB Cào] Cần nhượng lại căn hộ chung cư 2PN tại Cầu Giấy', 'Do chuyển công tác cần nhượng gấp căn hộ mini Cầu Giấy. Diện tích 55m2 gồm 2 phòng ngủ, 1 phòng khách bếp tách biệt. Giá thuê 7.5tr/tháng đóng 3 cọc 1. Full đồ chỉ việc xách vali đến ở. Liên hệ xem nhà ngay.', 7500000.00, 55.00, 'Ngõ 20 Trần Thái Tông, Phường Dịch Vọng, Cầu Giấy, Hà Nội', 14, 3, 'Rent', NULL, 'Facebook', 'fb_post_1029384756', 'https://facebook.com/groups/caugiayroom/posts/1029384756', 'Nguyễn Thị Hoa (FB)', '0987654321', 'Active');

-- Hình ảnh tin đăng mẫu 2
INSERT INTO `post_images` (`post_id`, `image_url`) VALUES
(2, 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?auto=format&fit=crop&w=800&q=80');
