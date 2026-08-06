# INT-02-v1.0 — Ma trận phụ thuộc & đóng gói liên module

> **Module Dependency Matrix & Packaging**
> Bộ tài liệu *Tích hợp liên module* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt nghiệp vụ / Solution**.
> Generic — không gắn khách hàng hay ngành cụ thể.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `INT-02-v1.0` |
| Tên | Ma trận phụ thuộc & đóng gói liên module |
| Phiên bản | 1.0 |
| Ngày | 03/08/2026 |
| Phân loại | Tích hợp liên module (BA / Solution) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |
| Phụ thuộc | Bộ SRS module v1.1 |

| Ver | Ngày | Mô tả | Trạng thái |
|---|---|---|---|
| 1.0 | 03/08/2026 | Khởi tạo bộ tích hợp liên module | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Chốt **phụ thuộc bắt buộc / khuyến nghị** giữa module để Presales đóng gói đúng và Solution thiết kế không gãy E2E.

### 1.2. Ký hiệu
| Ký hiệu | Nghĩa |
|---|---|
| **H** | Hard — không bán/triển khai thiếu |
| **S** | Soft — module chạy được nhưng thiếu tính năng E2E |
| **R** | Read — chỉ đọc master/projection |
| **—** | Không phụ thuộc trực tiếp |

---

## 2. Phụ thuộc cứng theo module

| Module | Hard dependency | Ghi chú |
|---|---|---|
| SYS | — | Nền tảng |
| HRM | SYS | |
| LMS | SYS | Soft: HRM |
| CRM | SYS | Soft: INV, LOG, FIN, WF, PRT |
| POS | SYS | Soft: INV, FIN, CRM |
| PUR | SYS | Soft: INV, FIN, WF |
| INV | SYS | Soft: PUR, LOG, POS, MFG, FIN |
| LOG | SYS, **INV** | Soft: CRM, FIN |
| MFG | SYS, **INV** | Soft: PUR, FIN, HRM, CRM |
| FSM | SYS | Soft: CRM, INV, FIN, PRT |
| PJM | SYS | Soft: CRM, INV, FIN, HRM, WF |
| FIN | SYS | Soft: CRM, PUR, INV, POS, HRM, AST, LOG |
| AST | SYS | Soft: FIN, HRM, PUR |
| WF | SYS | Soft: mọi module phát sinh duyệt |
| BI | SYS | Soft: dataset theo license module nguồn |
| PRT | SYS | Soft: CRM, LOG, FIN, FSM, PUR |

---

## 3. Ma trận tương tác (hàng = nguồn / cột = đích tiêu biểu)

> Ô ghi kênh chính: `E` = Event, `A` = API đồng bộ, `R` = Read master, `W` = Workflow.

| Từ \ Đến | SYS | HRM | CRM | INV | FIN | WF | PRT |
|---|---|---|---|---|---|---|---|
| SYS | — | E/R | E/R | E/R | E/R | E/R | E/R |
| HRM | A/R | — | — | — | E | W | — |
| LMS | A/R | E | — | — | E | — | — |
| CRM | A/R | — | — | A/E | E | W | E |
| POS | A/R | — | E/R | A | E | — | — |
| PUR | A/R | — | — | A/E | E | W | E |
| INV | A/R | — | R | — | E | — | — |
| LOG | A/R | — | E/R | A/E | E | — | E |
| MFG | A/R | R | R | A/E | E | — | — |
| FSM | A/R | — | E/R | A | E | — | E |
| PJM | A/R | R | R | A | E | W | — |
| FIN | A/R | R | R | R | — | W | E |
| AST | A/R | E | — | — | E | — | — |
| WF | A/R | E | E | — | E | — | — |
| BI | A/R | R | R | R | R | — | — |
| PRT | A/R | — | A/E | — | A/E | — | — |

---

## 4. Gói bán E2E gợi ý (presales)

### 4.1. Gói Nhân sự số
| Thành phần | Vai trò |
|---|---|
| SYS + HRM | Bắt buộc |
| WF | Duyệt phép/công/tuyển |
| LMS | Đào tạo bắt buộc onboarding |
| FIN | Post chi phí lương |
| AST | Thu hồi tài sản khi nghỉ |

### 4.2. Gói Bán hàng & phân phối
| Thành phần | Vai trò |
|---|---|
| SYS + CRM | Bắt buộc |
| INV + LOG | ATP, giao hàng |
| FIN | Công nợ / doanh thu |
| WF | Duyệt chiết khấu / hạn mức |
| PRT | KH tự phục vụ |

### 4.3. Gói Bán lẻ
| Thành phần | Vai trò |
|---|---|
| SYS + POS | Bắt buộc |
| INV | Trừ tồn realtime |
| FIN | Doanh thu ca / quỹ |
| CRM | KM, loyalty, KH |

### 4.4. Gói Mua hàng – Kho – Sản xuất
| Thành phần | Vai trò |
|---|---|
| SYS + PUR + INV | Trục chính |
| MFG | Lệnh SX, NVL/TP |
| FIN | AP & giá thành |
| WF | Duyệt PR/PO |

### 4.5. Gói Dịch vụ & dự án
| Thành phần | Vai trò |
|---|---|
| SYS + FSM / PJM | Theo nghiệp vụ |
| CRM | KH / HĐ |
| INV | Linh kiện / vật tư |
| FIN | Doanh thu – chi phí |
| PRT | Ticket self-service |

---

## 5. Ma trận ảnh hưởng khi tắt license

| Module bị tắt | Ảnh hưởng tức thì | Hành vi bắt buộc |
|---|---|---|
| SYS | Toàn hệ thống | Không cho tắt khi còn module khác active |
| INV | POS/LOG/MFG/PUR E2E gãy phần tồn | Chặn API INV; cảnh báo dependency |
| FIN | Không post sổ | Module nguồn giữ chứng từ “Pending post” |
| WF | Duyệt treo | Fallback duyệt nội module hoặc escalation admin |
| CRM | PRT/LOG thiếu ngữ cảnh KH-đơn | Ẩn journey liên quan |
| HRM | LMS gán NV theo vị trí hạn chế | LMS vẫn chạy khóa public/internal khác |

---

## 6. Checklist chốt gói với khách

1. Liệt kê module Must theo nghiệp vụ cốt lõi.
2. Map soft dependency → quyết định mua kèm Phase 1 hay Phase 2.
3. Xác nhận có dùng WF tập trung hay duyệt nội module.
4. Xác nhận master hàng hóa / KH / NV lấy từ đâu (tránh 2 nguồn).
5. Ký biên bản phụ thuộc (hard) trước khi kickoff kỹ thuật.

---

*Hết INT-02-v1.0.*
