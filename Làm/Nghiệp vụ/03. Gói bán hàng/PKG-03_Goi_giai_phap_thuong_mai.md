# PKG-03-v1.0 — Gói giải pháp thương mại

> **Commercial Solution Bundles**
> *Product Packaging & Commercial Catalog* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt Presales / Solution**.
> Generic — không gắn khách/ngành cứng; giá tiền theo bảng giá nội bộ.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `PKG-03-v1.0` |
| Tên | Gói giải pháp thương mại |
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

Các **bundle** ghép sẵn để Presales đề xuất nhanh. Bundle = SYS + tập SKU Must + khuyến nghị Should. Có thể cắt bớt Should sang Phase 2 nếu khách chấp nhận giảm E2E.

---

## 2. Ký hiệu trong bảng bundle

| Ký hiệu | Nghĩa |
|---|---|
| ● | Must — có trong báo giá Phase 1 |
| ○ | Should — khuyến nghị Phase 1 hoặc ghi Phase 2 |
| ◐ | Chọn 1 trong nhóm (FSM **hoặc** PJM, hoặc cả hai) |
| — | Không thuộc bundle |

---

## 3. Ma trận Bundle × Module

| Bundle \ Module | SYS | HRM | LMS | CRM | POS | PUR | INV | LOG | MFG | FSM | PJM | FIN | AST | WF | BI | PRT |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| B1 Nhân sự số | ● | ● | ○ | — | — | — | — | — | — | — | — | ○ | ○ | ○ | ○ | — |
| B2 Bán hàng & phân phối | ● | — | — | ● | — | — | ● | ● | — | — | — | ● | — | ○ | ○ | ○ |
| B3 Bán lẻ | ● | — | — | ○ | ● | — | ● | — | — | — | — | ● | — | — | ○ | — |
| B4 Mua – Kho – SX | ● | ○ | — | ○ | — | ● | ● | ○ | ● | — | — | ● | — | ○ | ○ | — |
| B5 Procure-to-Pay (không SX) | ● | — | — | — | — | ● | ● | — | — | — | — | ● | — | ○ | ○ | ○ |
| B6 Dịch vụ & dự án | ● | ○ | — | ● | — | — | ○ | — | — | ◐ | ◐ | ● | — | ○ | ○ | ○ |
| B7 Tài chính lõi | ● | ○ | — | ○ | ○ | ○ | ○ | ○ | — | — | — | ● | ○ | ○ | ○ | — |
| B8 Enterprise starter | ● | ● | ○ | ● | ○ | ● | ● | ○ | ○ | ○ | ○ | ● | ○ | ● | ○ | ○ |

---

## 4. Mô tả từng bundle

### 4.1. B1 — Nhân sự số

| Hạng mục | Nội dung |
|---|---|
| Mục tiêu khách | Số hóa HSNS, công–phép, onboarding |
| Must | SYS, HRM |
| Should | WF (duyệt), LMS (đào tạo), FIN (chi phí lương), AST (thu hồi TS) |
| Journey INT | E2E-05 Hire to Retire |
| Không cam kết nếu thiếu | Post sổ lương (thiếu FIN); duyệt đa cấp (thiếu WF) |

### 4.2. B2 — Bán hàng & phân phối

| Hạng mục | Nội dung |
|---|---|
| Mục tiêu khách | Báo giá → đơn → xuất kho → giao → công nợ |
| Must | SYS, CRM, INV, LOG, FIN |
| Should | WF (chiết khấu/hạn mức), PRT, BI |
| Journey INT | E2E-01 Lead to Cash |
| Rủi ro cắt gói | Bỏ LOG → không giao vận; bỏ FIN → không AR chuẩn |

### 4.3. B3 — Bán lẻ

| Hạng mục | Nội dung |
|---|---|
| Mục tiêu khách | Bán quầy, tồn realtime, đối soát ca |
| Must | SYS, POS, INV, FIN |
| Should | CRM (loyalty/KM), BI |
| Journey INT | E2E-03 POS Shift |

### 4.4. B4 — Mua – Kho – Sản xuất

| Hạng mục | Nội dung |
|---|---|
| Mục tiêu khách | NVL → SX → TP → giá thành khung |
| Must | SYS, PUR, INV, MFG, FIN |
| Should | WF, LOG, HRM (nhân công), CRM (đơn làm SX theo đơn) |
| Journey INT | E2E-04 Make + một phần E2E-02 |

### 4.5. B5 — Procure-to-Pay

| Hạng mục | Nội dung |
|---|---|
| Mục tiêu khách | Mua hàng đến thanh toán NCC (không SX) |
| Must | SYS, PUR, INV, FIN |
| Should | WF, PRT (NCC portal — nếu product cho phép), BI |
| Journey INT | E2E-02 |

### 4.6. B6 — Dịch vụ & dự án

| Hạng mục | Nội dung |
|---|---|
| Mục tiêu khách | Ticket hiện trường và/hoặc dự án có doanh thu–chi phí |
| Must | SYS, CRM, FIN + (FSM và/hoặc PJM) |
| Should | INV, WF, PRT, HRM, BI |
| Journey INT | E2E-06 / E2E-07 |

### 4.7. B7 — Tài chính lõi

| Hạng mục | Nội dung |
|---|---|
| Mục tiêu khách | Sổ sách, AR/AP, khóa kỳ; nhận chứng từ từ module nguồn dần |
| Must | SYS, FIN |
| Should | Các module phát sinh chứng từ theo lộ trình; AST; WF; BI |
| Journey INT | E2E-08 (+ nguồn tùy Phase) |
| Lưu ý | Không bán FIN như “ERP đầy đủ” nếu thiếu module nguồn |

### 4.8. B8 — Enterprise starter

| Hạng mục | Nội dung |
|---|---|
| Mục tiêu khách | Nền tảng rộng, rollout theo wave |
| Must | SYS, HRM, CRM, PUR, INV, FIN, WF |
| Should / Wave 2 | POS, LOG, MFG, FSM, PJM, LMS, AST, BI, PRT |
| Cách bán | Ký khung Enterprise + phụ lục wave; license bật theo wave |

---

## 5. Bundle theo quy mô (gợi ý)

| Quy mô | Gợi ý bắt đầu | Tránh |
|---|---|---|
| SME nhỏ (<50 user) | 1 bundle lõi (B1 hoặc B3 hoặc B5) + SYS | B8 full ngay Day-1 |
| SME vừa | 1–2 bundle giao nhau (vd B2+B5) | Mua rời thiếu INV khi có LOG |
| Mid-market | B8 wave 1 + wave 2 theo roadmap | Hứa toàn bộ E2E khi Phase 1 thiếu Soft |

---

## 6. Template ngành (không phải SKU)

| Template | Bundle gốc | Cấu hình thêm (không tạo module mới) |
|---|---|---|
| Phân phối | B2 | Nhóm hàng, chính sách giá, đa kho |
| Bán lẻ chuỗi | B3 | Nhiều store, ca, KM |
| Sản xuất rời rạc | B4 | BOM, kho NVL/TP |
| Dịch vụ kỹ thuật | B6 (FSM) | SLA, kỹ năng KT |
| Nhân sự tập đoàn | B1 | Nhiều pháp nhân/org_unit |

> Ngành chỉ là **gói cấu hình + training**; mã license vẫn là module chuẩn.

---

## 7. Quy tắc chỉnh bundle cho khách

1. Giữ nguyên Must; chỉ đàm phán Should.
2. Mọi cắt Soft phải ghi **“không bao gồm”** trên phụ lục phạm vi.
3. Không đổi mã bundle trên hợp đồng nếu nội dung Must đã đổi — tạo biến thể (`B2-Lite`) có bảng module rõ.
4. Map lại INT-04 journey bị ảnh hưởng.

---

*Hết PKG-03-v1.0.*
