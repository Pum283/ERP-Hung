# INT-04-v1.0 — Luồng nghiệp vụ end-to-end liên module

> **Cross-Module End-to-End Journeys**
> Bộ tài liệu *Tích hợp liên module* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt nghiệp vụ / Solution**.
> Generic — không gắn khách hàng hay ngành cụ thể.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `INT-04-v1.0` |
| Tên | Luồng nghiệp vụ end-to-end liên module |
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
Mô tả các **hành trình nghiệp vụ xuyên module** để BA/QA/Dev thống nhất điểm nối, sự kiện và tiêu chí hoàn tất.

### 1.2. Cách đọc
- Mỗi journey: mục tiêu → module tham gia → các bước → sự kiện chính → tiêu chí xong.
- Chi tiết màn hình/UC nằm trong SRS module tương ứng.

---

## 2. E2E-01 — Lead to Cash (Bán hàng có giao & công nợ)

**Module:** SYS, CRM, INV, LOG, FIN, WF*(opt)*, PRT*(opt)*

| Bước | Module | Hành động | Event / tương tác |
|---:|---|---|---|
| 1 | CRM | Tạo Lead → Qualify | `CrmLeadQualified` |
| 2 | CRM + WF | Báo giá & duyệt chiết khấu | `CrmQuoteApproved` |
| 3 | CRM | Xác nhận Sales Order | `CrmSalesOrderConfirmed` |
| 4 | INV | Reserve / kiểm ATP | `InvStockReserved` (API đồng bộ) |
| 5 | LOG | Tạo & điều phối chuyến giao | `LogShipmentDispatched` |
| 6 | LOG | Giao thành công / COD | `LogShipmentDelivered` |
| 7 | FIN | Ghi nhận doanh thu & công nợ / thu tiền | `FinPaymentReceived` |
| 8 | PRT | KH theo dõi đơn & thanh toán | Read models |

**Hoàn tất khi:** Đơn Delivered/Closed; tồn khớp; công nợ phản ánh đúng; có audit xuyên suốt `correlationId`.

---

## 3. E2E-02 — Mua hàng đến thanh toán NCC

**Module:** SYS, PUR, INV, FIN, WF*(opt)*, PRT*(NCC opt)*

| Bước | Module | Hành động | Event / tương tác |
|---:|---|---|---|
| 1 | INV/PUR | Nhu cầu tồn min / tạo PR | — |
| 2 | WF/PUR | Duyệt PR | `PurPurchaseRequestApproved` |
| 3 | PUR | Tạo & xác nhận PO | `PurPurchaseOrderConfirmed` |
| 4 | INV/PUR | Nhận hàng GRN | `PurGoodsReceived` / `InvStockReceived` |
| 5 | FIN | Khớp hóa đơn NCC / AP | — |
| 6 | FIN | Thanh toán NCC | `FinPaymentMade` |

**Hoàn tất khi:** Tồn tăng đúng PO; AP phản ánh đúng; khớp 3 chứng từ (PO–GRN–Invoice) theo rule.

---

## 4. E2E-03 — Bán lẻ trong ca (POS)

**Module:** SYS, POS, INV, FIN, CRM*(opt)*

| Bước | Module | Hành động | Event / tương tác |
|---:|---|---|---|
| 1 | POS | Mở ca | — |
| 2 | POS+INV | Bán hàng / trừ tồn (đồng bộ) | `PosSaleCompleted` |
| 3 | CRM | Tích điểm / gắn KH | Subscribe sale |
| 4 | POS | Đóng ca, đối quỹ | `PosShiftClosed` |
| 5 | FIN | Post doanh thu / quỹ | Subscribe shift/sale |

**Hoàn tất khi:** Tồn khớp bán; ca đóng; sổ quỹ/doanh thu nhận đủ chứng từ.

---

## 5. E2E-04 — Sản xuất chuẩn (Make to Stock/Order)

**Module:** SYS, MFG, INV, PUR*(opt)*, FIN, CRM*(opt)*

| Bước | Module | Hành động | Event / tương tác |
|---:|---|---|---|
| 1 | CRM/MFG | Nhu cầu / kế hoạch | — |
| 2 | MFG | Release Work Order | `MfgWorkOrderReleased` |
| 3 | INV | Xuất NVL | `InvStockIssued` |
| 4 | PUR | PR thiếu NVL (nếu cần) | — |
| 5 | MFG | Hoàn thành + QC | `MfgWorkOrderCompleted` |
| 6 | INV | Nhập TP | `InvStockReceived` |
| 7 | FIN | Giá thành / bút toán | Subscribe WO completed |

**Hoàn tất khi:** NVL/TP khớp BOM & số lượng; giá thành có thể tái lập.

---

## 6. E2E-05 — Hire to Retire (Nhân sự)

**Module:** SYS, HRM, LMS, WF, FIN, AST

| Bước | Module | Hành động | Event / tương tác |
|---:|---|---|---|
| 1 | HRM+WF | Tuyển dụng & duyệt | — |
| 2 | HRM | Tạo hồ sơ / HĐ | `HrmEmployeeHired` |
| 3 | SYS | Cấp user & quyền | Subscribe hire |
| 4 | LMS | Gán đào tạo bắt buộc | Subscribe hire |
| 5 | HRM | Chấm công → khóa công | `HrmTimesheetLocked` |
| 6 | HRM | Payroll | `HrmPayrollPosted` → FIN |
| 7 | HRM | Nghỉ việc | `HrmEmployeeTerminated` |
| 8 | AST+SYS | Thu hồi tài sản & quyền | Subscribe terminate |

**Hoàn tất khi:** Hồ sơ Closed; user disabled; tài sản thu hồi; quyết toán lương/phép xong.

---

## 7. E2E-06 — Dịch vụ hiện trường & cổng KH

**Module:** SYS, FSM, CRM, INV, FIN, PRT

| Bước | Module | Hành động | Event / tương tác |
|---:|---|---|---|
| 1 | PRT/CRM | Tạo ticket | `PrtTicketCreated` |
| 2 | FSM | Điều phối kỹ thuật viên | — |
| 3 | INV | Xuất linh kiện | `InvStockIssued` |
| 4 | FSM | Đóng WO | `FsmWorkOrderClosed` |
| 5 | FIN | Phí dịch vụ / bảo hành | Subscribe |
| 6 | PRT | KH xem tiến độ & đánh giá | Read |

---

## 8. E2E-07 — Dự án đến quyết toán

**Module:** SYS, PJM, CRM, INV, HRM, FIN, WF, FSM*(BH)*

| Bước | Module | Hành động | Event / tương tác |
|---:|---|---|---|
| 1 | CRM→PJM | Ký HĐ / mở dự án | — |
| 2 | WF | Duyệt kickoff / CR | `WfTaskApproved` |
| 3 | INV/HRM | Cấp vật tư & nhân sự | — |
| 4 | PJM | Theo dõi tiến độ / chi phí | — |
| 5 | FIN | Ghi nhận doanh thu–chi phí | — |
| 6 | PJM | Đóng dự án | `PjmProjectClosed` |
| 7 | FSM | Bàn giao bảo hành | Subscribe |

---

## 9. E2E-08 — Khóa sổ tháng (FIN hub)

**Module:** FIN (+ mọi module phát sinh chứng từ tài chính)

| Bước | Module | Hành động |
|---:|---|---|
| 1 | CRM/POS/LOG/PUR/INV/HRM/AST/MFG | Hoàn tất chứng từ kỳ |
| 2 | FIN | Đối soát & xử lý treo |
| 3 | FIN | Khóa kỳ | `FinPeriodClosed` |
| 4 | Tất cả | Chặn post sai kỳ; điều chỉnh qua chứng từ điều chỉnh |

---

## 10. Ma trận journey ↔ module

| Journey | SYS | HRM | LMS | CRM | POS | PUR | INV | LOG | MFG | FSM | PJM | FIN | AST | WF | BI | PRT |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| E2E-01 Lead to Cash | ● | | | ● | | | ● | ● | | | | ● | | ○ | ○ | ○ |
| E2E-02 Procure to Pay | ● | | | | | ● | ● | | | | | ● | | ○ | | ○ |
| E2E-03 POS Shift | ● | | | ○ | ● | | ● | | | | | ● | | | | |
| E2E-04 Make | ● | ○ | | ○ | | ○ | ● | | ● | | | ● | | | | |
| E2E-05 Hire to Retire | ● | ● | ● | | | | | | | | | ● | ● | ○ | | |
| E2E-06 Field Service | ● | | | ● | | | ● | | | ● | | ● | | | | ● |
| E2E-07 Project | ● | ○ | | ● | | | ● | | | ○ | ● | ● | ○ | ○ | | |
| E2E-08 Period Close | ● | ○ | | ○ | ○ | ○ | ○ | ○ | ○ | ○ | ○ | ● | ○ | | ○ | |

> ● = tham gia chính · ○ = tùy gói / Phase

---

## 11. Tiêu chí nghiệm thu tích hợp (chung)

1. Mỗi journey chạy demo thành công với module hard dependency.
2. Tắt soft module → journey degrade đúng mô tả INT-02 (không 500 hàng loạt).
3. Event có `correlationId` xuyên suốt; đối soát được số lượng chứng từ.
4. Không nhân bản master; ID tham chiếu resolve được.
5. QA có test case truy vết INT-04 → SRS UC.

---

*Hết INT-04-v1.0.*
