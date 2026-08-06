# INT-05-v1.0 — Đồng bộ, xử lý lỗi & vận hành tích hợp

> **Sync, Failure Handling & Integration Operations**
> Bộ tài liệu *Tích hợp liên module* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt nghiệp vụ / Solution**.
> Generic — không gắn khách hàng hay ngành cụ thể.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `INT-05-v1.0` |
| Tên | Đồng bộ, xử lý lỗi & vận hành tích hợp |
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

Tài liệu quy định cách hệ thống **đồng bộ dữ liệu liên module**, xử lý lỗi, đảm bảo tính nhất quán và vận hành khi sự cố hoặc thay đổi license.

---

## 2. Mô hình Outbox / Inbox

```text
 [Module A DB Txn] --write--> Outbox --> (Dispatcher SYS) --> Bus
                                                      |
                                                 Subscriber
                                                      |
                                                 Inbox / Processed
                                                      |
                                               [Module B handler]
```

| Thành phần | Trách nhiệm |
|---|---|
| Outbox | Cùng transaction với ghi nghiệp vụ nguồn |
| Dispatcher | Publish ít nhất một lần |
| Inbox | Chống xử lý trùng (`eventId`) |
| Dead Letter | Lưu event thất bại quá số lần retry |

---

## 3. Idempotency

1. Consumer **bắt buộc** kiểm tra `eventId` đã xử lý chưa.
2. Thao tác tạo chứng từ từ event dùng `idempotencyKey` = `eventId` hoặc `producer+businessKey`.
3. API đồng bộ (reserve tồn, thu tiền…) nhận header/key idempotency từ client.

---

## 4. Chiến lược retry

| Loại lỗi | Hành vi |
|---|---|
| Lỗi tạm (timeout, 5xx, DB lock) | Retry exponential backoff (ví dụ 15s → 1m → 5m → 30m) |
| Lỗi nghiệp vụ (validate, thiếu master) | Không retry vô hạn; đưa DLQ + thông báo admin module |
| Module đích tắt license | Park event / skip có log; không spam retry |
| Sai schema version | DLQ + alert Solution |

---

## 5. Nhất quán dữ liệu

### 5.1. Strong consistency (đồng bộ)
Dùng khi sai lệch ngay gây mất tiền/hàng: trừ tồn POS, reserve SO, ghi nhận thanh toán tại quầy.

### 5.2. Eventual consistency (bất đồng bộ)
Dùng khi chấp nhận trễ vài giây–phút: post sổ, thông báo, cập nhật portal, BI.

### 5.3. Đối soát định kỳ
| Cặp | Đối soát |
|---|---|
| POS sale ↔ INV issue | Số lượng theo ca/ngày |
| SO confirmed ↔ Shipment ↔ AR | Trạng thái đơn vs công nợ |
| Payroll posted ↔ FIN journals | Tổng tiền kỳ |
| GRN ↔ AP invoice | 3-way match ngoại lệ |

---

## 6. Hành vi khi tắt / hết hạn module

| Tình huống | Hành vi chuẩn |
|---|---|
| Tắt module đích | Publisher vẫn ghi outbox; dispatcher đánh dấu `blocked_by_license`; UI nguồn cảnh báo |
| Bật lại module | Replay / reprocess hàng đợi theo policy |
| Hết hạn SYS | Toàn hệ thống theo chính sách license SYS (read-only / block) |
| Xóa dữ liệu | **Không** xóa tự động khi tắt module; chỉ theo job purge có phê duyệt |

---

## 7. Quan sát & vận hành

| Chỉ số | Ngưỡng gợi ý |
|---|---|
| Lag Outbox (p95) | < 30s môi trường chuẩn |
| Tỷ lệ DLQ / ngày | ≈ 0; mọi bản ghi có owner xử lý |
| Thời gian xử lý consumer p95 | Theo SLA từng journey |
| Số event poison | Alert ngay |

**Log bắt buộc:** `eventId`, `eventType`, `tenantId`, `correlationId`, `consumer`, `result`, `errorCode`.

---

## 8. Bảo mật tích hợp

- Service-to-service: JWT nội bộ hoặc mTLS (quyết định kỹ thuật sau).
- Webhook ngoài: chữ ký HMAC, rotate secret.
- Không đưa PII/lương vào event Broad-cast nếu subscriber không cần; dùng reference + pull có quyền.
- Mọi thay đổi subscription event ghi audit.

---

## 9. Sự cố mẫu & cách xử lý

| Sự cố | Triệu chứng | Xử lý |
|---|---|---|
| Đơn đã giao nhưng AR không tạo | Thiếu consumer FIN / DLQ | Reprocess event `LogShipmentDelivered` / `CrmSalesOrderConfirmed` |
| Bán POS tồn âm | Race reserve | Khóa lạc quan / serialize theo item-warehouse; bù chứng từ |
| Payroll post trùng bút toán | Thiếu idempotency | Chặn bằng `eventId`; đảo bút toán thủ công nếu đã lệch |
| Portal không thấy đơn | Projection trễ | Kiểm tra lag; rebuild read model theo SO id |

---

## 10. Checklist go-live tích hợp

1. Outbox/Inbox bật trên mọi module Phase 1.
2. Danh sách event Phase 1 đã đăng ký (theo INT-03).
3. Test tắt soft module không làm sập hard path.
4. Có dashboard lag + DLQ.
5. Runbook sự cố (mục 9) đã handover vận hành.
6. Đối soát ngày đầu tiên sau go-live có chữ ký kế toán/ops.

---

## 11. Truy vết

| Liên quan | Tài liệu |
|---|---|
| Kiến trúc | INT-01 |
| Phụ thuộc | INT-02 |
| Event | INT-03 |
| Journey | INT-04 |
| SRS module | `../01. Modules` |

---

*Hết INT-05-v1.0.*
