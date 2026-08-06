# PKG-05-v1.0 — Playbook Presales → Delivery

> **Sales & Delivery Playbook for Packaged ERP**
> *Product Packaging & Commercial Catalog* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt Presales / Solution**.
> Generic — không gắn khách/ngành cứng; giá tiền theo bảng giá nội bộ.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `PKG-05-v1.0` |
| Tên | Playbook Presales → Delivery |
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

Playbook vận hành từ **discovery → báo giá → ký → kickoff → nghiệm thu Phase 1** theo đúng gói đã bán, tránh lệch scope giữa Sales và Delivery.

---

## 2. Luồng tổng thể

```text
 Discovery → Chọn Bundle/SKU → Xác nhận Must/Should
     → Demo đúng phạm vi → Báo giá / phụ lục
     → Ký HĐ → Kickoff Delivery
     → Cấu hình & UAT theo Phase 1
     → Go-live → Chăm sóc / Upsell Phase 2
```

---

## 3. Checklist Presales (trước báo giá)

| # | Hạng mục | Kết quả cần có |
|---|---|---|
| 1 | Ngành / mô hình kinh doanh | Template ngành (PKG-03 §6) |
| 2 | Hành trình Must | Map INT-04 (E2E-xx) |
| 3 | Bundle đề xuất | B1…B8 hoặc CUSTOM |
| 4 | Hard dependency | Đủ INV nếu có LOG/MFG… |
| 5 | Soft cắt bỏ | Ghi “không bao gồm” |
| 6 | Quy mô user / CN / store | Metric PKG-04 |
| 7 | Cần portal / BI Day-1? | Quyết định PRT/BI |
| 8 | Duyệt tập trung? | WF Phase 1 hay không |
| 9 | Master dữ liệu | Ai sở hữu KH/Hàng/NV |
| 10 | Wave rollout | Enterprise mới dùng B8 |

---

## 4. Checklist Demo

1. Demo **đúng module trong báo giá** — không demo Soft đã cắt như là đã mua.
2. Nêu rõ bước nào cần module Should (vd giao hàng cần LOG).
3. Login bằng role thật (không chỉ admin bypass).
4. Chỉ menu theo license giả lập gói đang bán.
5. Kết thúc demo: chốt danh sách Must/Should với khách (biên bản ngắn).

---

## 5. Hồ sơ bàn giao Sales → Delivery

| Tài liệu | Bắt buộc |
|---|---|
| Phụ lục module Phase 1 (mã SKU) | Yes |
| Phụ lục Phase 2 / option | Nếu có |
| Bundle code / plan_code | Yes |
| Danh sách “không bao gồm” | Yes |
| Biên bản hard dependency | Yes |
| Kỳ vọng journey E2E | Yes (mã INT-04) |
| Ước lượng user / CN | Yes |
| Đầu mối khách (IT/KT/Ops) | Yes |

---

## 6. Kickoff kỹ thuật (Delivery)

1. Tạo/gán tenant + license đúng phụ lục.
2. Enable đúng `license_module`.
3. Khởi tạo org_unit, department, job_level, admin, role mẫu gói.
4. Import master tối thiểu theo module Must.
5. Cấu hình WF (nếu có), sequence, setting.
6. UAT theo kịch bản journey Phase 1 — **không** UAT Soft đã cắt.
7. Đào tạo theo vai trò trong phạm vi gói.
8. Go-live checklist: INT-05 § go-live tích hợp (nếu đa module).

---

## 7. Tiêu chí nghiệm thu gói Phase 1

| # | Tiêu chí |
|---|---|
| 1 | Đủ module Must trên license & menu |
| 2 | Không còn phụ thuộc cứng thiếu |
| 3 | Journey Must chạy UAT đạt (theo biên bản) |
| 4 | Phân quyền mẫu (Role/Permission/Department/JobLevel) hoạt động |
| 5 | Tắt thử 1 soft module (nếu có) không làm sập Must path |
| 6 | Khách ký biên bản nghiệm thu phạm vi Phase 1 |

---

## 8. After go-live / Upsell

| Tín hiệu | Đề xuất add-on |
|---|---|
| KH hỏi tracking đơn / công nợ self-service | PRT |
| Cần dashboard C-level | BI |
| Duyệt chồng chéo nhiều chứng từ | WF |
| Có xuất kho giao khách | LOG (+ đủ INV) |
| Mở nhà máy | MFG |
| Chuỗi cửa hàng | POS |

Quy trình upsell tuân PKG-04 §5.1.

---

## 9. Rủi ro thường gặp & cách tránh

| Rủi ro | Cách tránh |
|---|---|
| Bán thiếu INV khi có LOG/MFG | Gate bắt buộc trên báo giá (PKG-02) |
| Demo vượt phạm vi → khách tưởng đã mua | Script demo theo license giả lập |
| Delivery làm Soft “cho đủ” ngoài HĐ | Scope freeze theo phụ lục |
| Downsell phá E2E đang chạy | Kiểm hard dependents (PKG-04) |
| Hai nguồn master (KH/Hàng) | Chốt owner theo INT-01 / kickoff |

---

## 10. Truy vết

| Liên quan | Tài liệu |
|---|---|
| Chiến lược | PKG-01 |
| SKU / Bundle / License | PKG-02…04 |
| Phụ thuộc & journey | INT-02, INT-04 |
| SRS chức năng | `../01. Modules` |

---

*Hết PKG-05-v1.0.*
