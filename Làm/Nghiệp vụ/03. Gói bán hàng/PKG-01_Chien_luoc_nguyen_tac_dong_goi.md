# PKG-01-v1.0 — Chiến lược & nguyên tắc đóng gói bán

> **Packaging Strategy & Commercial Rules**
> *Product Packaging & Commercial Catalog* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt Presales / Solution**.
> Generic — không gắn khách/ngành cứng; giá tiền theo bảng giá nội bộ.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `PKG-01-v1.0` |
| Tên | Chiến lược & nguyên tắc đóng gói bán |
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

### 1.1. Mục đích
Chốt **cách sản phẩm ERP được đóng gói và bán**: module đơn lẻ (SKU), gói giải pháp (bundle), vai trò bắt buộc của SYS, và ranh giới giữa “chạy được” với “có giá trị E2E”.

### 1.2. Đối tượng đọc
- Presales / Sales Engineer
- BA / Product Owner
- Solution Architect
- Delivery / Project Manager

### 1.3. Ngoài phạm vi
- Giá niêm yết chi tiết theo thị trường (thuộc bảng giá nội bộ / hợp đồng).
- Thiết kế kỹ thuật event (thuộc INT) và schema DB (thuộc DDD).

---

## 2. Mô hình sản phẩm

```text
                    ┌──────────────────────────┐
                    │   Gói giải pháp (Bundle) │
                    │  = tập SKU module + quy  │
                    │    tắc phụ thuộc Must    │
                    └────────────┬─────────────┘
                                 │ gồm
              ┌──────────────────┼──────────────────┐
              ▼                  ▼                  ▼
         SKU Module         SKU Module         SKU SYS
        (HRM/CRM/…)        (INV/FIN/…)      (luôn kèm)
              │                  │                  │
              └────────────┬─────┴──────────────────┘
                           ▼
                 License tenant (bật/tắt module
                 + quota user / chi nhánh / …)
```

| Tầng | Định nghĩa | Ví dụ |
|---|---|---|
| **SKU Module** | Đơn vị bán nhỏ nhất (trừ SYS) | `CRM`, `INV` |
| **Bundle** | Tập SKU ghép sẵn theo hành trình / ngành | Gói Bán hàng & phân phối |
| **License** | Hợp đồng kỹ thuật trên tenant | `sys.license` + `license_module` |
| **Add-on** | SKU bổ sung sau go-live | Thêm `PRT`, `BI` |

---

## 3. Nguyên tắc đóng gói (bắt buộc tuân thủ)

| # | Nguyên tắc | Mô tả |
|---|---|---|
| R1 | SYS luôn kèm | Mọi báo giá / hợp đồng đều có SYS; không bán module nghiệp vụ “trần”. |
| R2 | Hard dependency trong Must | Module có phụ thuộc cứng (vd LOG→INV) phải có trong Phase 1. |
| R3 | Soft ≠ hứa E2E | Thiếu soft dependency vẫn chạy module, nhưng **không cam kết** journey đầy đủ. |
| R4 | Một master một chủ | Ví dụ Item thuộc INV, Customer thuộc CRM — ghi rõ khi bán gói. |
| R5 | License gate | Tắt module ⇒ ẩn menu + chặn API; dữ liệu giữ theo chính sách lưu trữ. |
| R6 | Upsell không phá dữ liệu | Bật thêm module = enable license + cấu hình; không migration phá. |
| R7 | Downsell có cảnh báo | Tắt module phải kiểm tra hard dependents còn active. |
| R8 | Generic trước, ngành sau | Ngành (F&B, phân phối…) = **template cấu hình**, không SKU riêng trừ khi Product chốt. |
| R9 | WF & BI là tăng tốc | WF/BI bán độc lập được nhưng giá trị tăng khi có module nguồn. |
| R10 | Truy vết tài liệu | Mỗi bundle map INT-04 journey + SRS module liên quan. |

---

## 4. Phân loại phụ thuộc thương mại

| Loại | Ký hiệu | Ý nghĩa với khách | Xử lý trên báo giá |
|---|---|---|---|
| Bắt buộc nền | **SYS** | Không thể thiếu | Luôn liệt kê |
| Hard | **Must** | Module không triển khai/không vận hành đúng thiếu thành phần này | Bắt buộc cùng Phase |
| Soft E2E | **Should** | Thiếu thì mất một phần hành trình | Phase 1 hoặc Phase 2 ghi rõ |
| Nice-to-have | **Could** | Tiện ích / portal / BI nâng cao | Option |

Ánh xạ kỹ thuật chi tiết: **INT-02**. Tài liệu PKG dùng ngôn ngữ Must/Should/Could cho Presales.

---

## 5. Ai quyết định gì

| Vai trò | Quyết định |
|---|---|
| Presales | Chọn bundle / SKU phù hợp discovery |
| BA | Xác nhận journey Must vs Should theo nghiệp vụ khách |
| Solution | Xác nhận hard dependency & tích hợp |
| Commercial | Báo giá, hạn mức, thời hạn license |
| Delivery | Scope Phase 1 = đúng những gì đã ký |

---

## 6. Quan hệ với bộ tài liệu khác

| Nội dung | Tài liệu |
|---|---|
| Chức năng từng module | SRS-XXX-v1.1 |
| Phụ thuộc kỹ thuật | INT-02 |
| Journey E2E | INT-04 |
| Bảng license CSDL | DDD-02 / DDD-MASTER (`sys.license*`) |
| Catalog SKU | PKG-02 |
| Bundle | PKG-03 |
| License & quota | PKG-04 |
| Playbook bán/triển khai | PKG-05 |

---

*Hết PKG-01-v1.0.*
