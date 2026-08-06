# PKG-04-v1.0 — License · quota · thương mại

> **Licensing, Quotas & Commercial Terms**
> *Product Packaging & Commercial Catalog* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt Presales / Solution**.
> Generic — không gắn khách/ngành cứng; giá tiền theo bảng giá nội bộ.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `PKG-04-v1.0` |
| Tên | License · quota · thương mại |
| Phiên bản | 1.0 |
| Ngày | 03/08/2026 |
| Phân loại | Gói bán hàng (Presales / BA / Delivery) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |
| Đầu vào | SRS module v1.1 · INT-02 · DDD license |

| Ver | Ngày | Mô tả | Trạng thái |
|---|---|---|---|
| 1.0 | 03/08/2026 | Khởi tạo bộ Gói bán hàng | Chờ duyệt |

---

## 1. Giới thiệu

Quy định **cách license kỹ thuật phản ánh hợp đồng thương mại**: module bật/tắt, hạn mức, gia hạn, upsell/downsell. Ánh xạ CSDL: `sys.license`, `sys.license_module` (DDD-02).

---

## 2. Mô hình license

```text
 Hợp đồng thương mại
        │
        ▼
 sys.license (plan, hiệu lực, max_users, status)
        │
        ├── license_module: SYS (always)
        ├── license_module: CRM = enabled
        ├── license_module: INV = enabled
        └── …
                │
                ▼
         Menu + API gateway kiểm tra
```

| Thuộc tính | Ý nghĩa thương mại |
|---|---|
| `plan_code` | Mã gói thương mại (SME / Standard / Enterprise / custom) |
| `valid_from` / `valid_to` | Thời hạn thuê bao |
| `max_users` | Hạn mức user active |
| `license_module.is_enabled` | Module có trong hợp đồng Phase hiện tại |
| `quota_json` | Hạn mức riêng (chi nhánh, store, API call…) — tùy plan |

---

## 3. Metric tính phí (khung — chốt bảng giá nội bộ)

| Metric | Áp dụng điển hình | Ghi chú |
|---|---|---|
| Named user / active user | Hầu hết module | Đồng bộ `max_users` |
| Module pack | Theo SKU / bundle | Đơn vị báo giá chính |
| Chi nhánh / pháp nhân | org_unit loại Company/Branch | Optional add-on |
| Điểm bán (store) | POS | Optional |
| Concurrent device / POS terminal | POS | Optional |
| API volume | Tích hợp nặng | Optional qua `api_key` policy |
| Portal external user | PRT | Có thể tách quota |

> Tài liệu này **không** ấn định đơn giá. Presales lấy số từ bảng giá / deal desk.

---

## 4. Gói plan khung (gợi ý đặt tên)

| Plan | Thành phần gợi ý | Đối tượng |
|---|---|---|
| `PLAN_SME_HR` | ≈ Bundle B1 | SME nhân sự |
| `PLAN_SME_RETAIL` | ≈ Bundle B3 | Cửa hàng / chuỗi nhỏ |
| `PLAN_DISTRIBUTION` | ≈ Bundle B2 | Phân phối |
| `PLAN_MANUFACTURING` | ≈ Bundle B4 | SX vừa |
| `PLAN_FIN_CORE` | ≈ Bundle B7 | Kế toán lõi |
| `PLAN_ENTERPRISE` | ≈ Bundle B8 + wave | Mid-market |
| `PLAN_CUSTOM` | SKU lựa chọn | Deal riêng |

`plan_code` trên license nên khớp một trong các mã trên (hoặc mã custom có phụ lục).

---

## 5. Upsell / Downsell / Gia hạn

### 5.1. Upsell (bật thêm module)
1. Ký phụ lục / SOW.
2. Cập nhật `license_module` = enabled.
3. Seed permission/sequence/menu module (job SYS).
4. Delivery cấu hình master tối thiểu + training.
5. Không xóa/không migrate phá dữ liệu cũ.

### 5.2. Downsell (tắt module)
1. Kiểm tra hard dependents còn bật không (INT-02 / PKG-02).
2. Nếu còn → **chặn downsell** hoặc bắt tắt cả chuỗi phụ thuộc.
3. Set `is_enabled = false`; UI/API module bị chặn.
4. Dữ liệu giữ; export nếu hợp đồng yêu cầu.
5. Audit + thông báo admin.

### 5.3. Gia hạn / hết hạn
| Trạng thái | Hành vi gợi ý (chốt khi triển khai) |
|---|---|
| Sắp hết hạn (T-30…T-7) | Cảnh báo admin |
| Hết hạn | Read-only hoặc block login theo chính sách plan |
| SYS hết hạn | Áp dụng toàn tenant |

---

## 6. Điều khoản thương mại khung (checklist hợp đồng)

1. Danh sách module Must Phase 1 (mã SKU).
2. Module Should Phase 2 (nếu có) + điều kiện kích hoạt.
3. Thời hạn, số user, số chi nhánh/store (nếu áp dụng).
4. Môi trường: Production (+ UAT?).
5. Chính sách dữ liệu khi chấm dứt: giữ / export / xóa sau N ngày.
6. Phụ thuộc cứng đã được khách xác nhận (ký).
7. Phạm vi **không bao gồm** (soft đã cắt).
8. SLA hỗ trợ (tham chiếu hợp đồng dịch vụ — ngoài PKG).

---

## 7. Ánh xạ vận hành kỹ thuật

| Sự kiện thương mại | Hành vi hệ thống |
|---|---|
| Ký mới | Tạo tenant + license + admin đầu tiên |
| Bật module | Enable license_module; hiện menu |
| Tắt module | Chặn API/UI; subscriber degrade (INT-05) |
| Tăng user | Tăng `max_users`; chặn tạo user khi vượt |
| Đổi plan | Cập nhật plan_code + module set |

---

## 8. Truy vết

| Liên quan | Tài liệu |
|---|---|
| Nguyên tắc đóng gói | PKG-01 |
| SKU | PKG-02 |
| Bundle | PKG-03 |
| Tắt license kỹ thuật | INT-02 §5, INT-05 |
| Bảng DB | DDD-02 / DDD-MASTER |

---

*Hết PKG-04-v1.0.*
