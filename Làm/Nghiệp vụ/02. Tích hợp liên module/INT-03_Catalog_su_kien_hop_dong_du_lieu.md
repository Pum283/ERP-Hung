# INT-03-v1.0 — Catalog sự kiện & hợp đồng dữ liệu liên module

> **Event Catalog & Logical Data Contracts**
> Bộ tài liệu *Tích hợp liên module* — ERP bán theo module.
> Phiên bản **1.0** · Ngày 03/08/2026 · Trạng thái: **Chờ duyệt nghiệp vụ / Solution**.
> Generic — không gắn khách hàng hay ngành cụ thể.

---

## 0. Thông tin tài liệu

| Thuộc tính | Giá trị |
|---|---|
| Mã tài liệu | `INT-03-v1.0` |
| Tên | Catalog sự kiện & hợp đồng dữ liệu liên module |
| Phiên bản | 1.0 |
| Ngày | 03/08/2026 |
| Phân loại | Tích hợp liên module (BA / Solution) |
| Định dạng bàn giao | Microsoft Word (`.docx`) |
| Phụ thuộc | Bộ SRS module v1.1 |

| Ver | Ngày | Mô tả | Trạng thái |
|---|---|---|---|
| 1.0 | 03/08/2026 | Khởi tạo bộ tích hợp liên module | Chờ duyệt |
| 1.0.1 | 04/08/2026 | Thêm `SysMsgMessageSent` (chat realtime SYS-13) | Chờ duyệt |

---

## 1. Giới thiệu

### 1.1. Mục đích
Chuẩn hóa **tên sự kiện, publisher, subscriber và payload logic** để các module tích hợp thống nhất — mức BA/Solution (chưa phải schema JSON kỹ thuật cuối).

### 1.2. Quy ước đặt tên
- Format: `{BoundedContext}{Entity}{PastAction}` — ví dụ `HrmEmployeeTerminated`, `CrmSalesOrderConfirmed`.
- Dùng **quá khứ** (đã xảy ra), không dùng mệnh lệnh.
- Version payload: `payloadVersion` (int), tăng khi breaking change.

### 1.3. Envelope sự kiện (chung)

| Trường | Bắt buộc | Mô tả |
|---|---|---|
| `eventId` | Có | UUID duy nhất |
| `eventType` | Có | Tên sự kiện |
| `occurredAt` | Có | UTC timestamp |
| `tenantId` | Có | Tenant |
| `correlationId` | Có | Trace xuyên suốt journey |
| `causationId` | Không | Event/command gốc |
| `producer` | Có | Mã module (HRM, CRM…) |
| `payloadVersion` | Có | Phiên bản hợp đồng |
| `payload` | Có | Dữ liệu nghiệp vụ tối thiểu |

---

## 2. Catalog sự kiện theo miền

### 2.1. SYS (nền)
| Event | Publisher | Subscriber tiêu biểu | Payload tối thiểu |
|---|---|---|---|
| `SysUserCreated` | SYS | HRM, PRT, tất cả | userId, orgIds, roles |
| `SysUserDisabled` | SYS | Tất cả | userId, reason |
| `SysRoleChanged` | SYS | Audit, BI | userId, rolesBefore/After |
| `SysLicenseChanged` | SYS | Tất cả module | modules[], effectiveFrom |
| `SysOrgUnitChanged` | SYS | HRM, INV, CRM | orgUnitId, path |
| `SysNotificationRequested` | Mọi module→SYS | Email/SMS gateway | template, to, data |
| `SysMsgMessageSent` | SYS-MSG (chat) | Push/webhook (optional) | conversationId, messageId, fromUserId, toUserIds[] — realtime chính = SignalR `/hubs/msg` |

### 2.2. HRM ↔ liên module
| Event | Publisher | Subscriber | Mục đích |
|---|---|---|---|
| `HrmEmployeeHired` | HRM | LMS, SYS(user link), AST | Onboarding, cấp tài khoản |
| `HrmEmployeeTerminated` | HRM | SYS, AST, LMS, FIN | Thu hồi quyền, tài sản, quyết toán |
| `HrmTimesheetLocked` | HRM | FIN (payroll prep) | Khóa công sẵn sàng tính lương |
| `HrmPayrollPosted` | HRM | FIN | Chi phí lương / phải trả |
| `HrmLeaveApproved` | HRM | INV/POS?(optional staffing) | Cập nhật lịch vắng |
| `LmsCertificateIssued` | LMS | HRM | Ghi chứng chỉ vào hồ sơ |

### 2.3. CRM / POS / PRT
| Event | Publisher | Subscriber | Mục đích |
|---|---|---|---|
| `CrmLeadQualified` | CRM | WF, BI | Pipeline |
| `CrmQuoteApproved` | CRM/WF | CRM | Cho phép tạo SO |
| `CrmSalesOrderConfirmed` | CRM | INV, LOG, FIN, PRT | Reserve / giao / công nợ |
| `CrmSalesOrderCancelled` | CRM | INV, LOG, FIN | Nhả reserve, hủy giao |
| `PosSaleCompleted` | POS | INV, FIN, CRM | Trừ tồn, doanh thu, loyalty |
| `PosShiftClosed` | POS | FIN | Đối soát ca |
| `PrtTicketCreated` | PRT | FSM/CRM | Tiếp nhận CSKH |

### 2.4. PUR / INV / LOG / MFG
| Event | Publisher | Subscriber | Mục đích |
|---|---|---|---|
| `PurPurchaseRequestApproved` | PUR/WF | PUR | Cho tạo PO |
| `PurPurchaseOrderConfirmed` | PUR | INV, PRT(NCC), FIN | Kỳ vọng nhận hàng / AP |
| `PurGoodsReceived` | PUR/INV | INV, FIN | Nhập kho, tạm nhập AP |
| `InvStockReserved` | INV | CRM/POS | Giữ hàng |
| `InvStockIssued` | INV | FIN, LOG, MFG | Xuất kho |
| `InvStockReceived` | INV | FIN, MFG | Nhập kho |
| `InvStockCountPosted` | INV | FIN | Điều chỉnh tồn |
| `LogShipmentDispatched` | LOG | CRM, PRT, FIN | Tracking, COD |
| `LogShipmentDelivered` | LOG | CRM, FIN, PRT | Hoàn tất giao / thu COD |
| `MfgWorkOrderReleased` | MFG | INV | Xuất NVL |
| `MfgWorkOrderCompleted` | MFG | INV, FIN | Nhập TP, giá thành |

### 2.5. FIN / AST / FSM / PJM / WF / BI
| Event | Publisher | Subscriber | Mục đích |
|---|---|---|---|
| `FinPeriodClosed` | FIN | Tất cả chứng từ nguồn | Chặn post sai kỳ |
| `FinPaymentReceived` | FIN | CRM, PRT, LOG | Cập nhật công nợ |
| `FinPaymentMade` | FIN | PUR | Thanh toán NCC |
| `AstAssetAssigned` | AST | HRM | Gắn tài sản NV |
| `AstAssetReturned` | AST | HRM | Offboarding |
| `AstDepreciationPosted` | AST | FIN | Bút toán KH |
| `FsmWorkOrderClosed` | FSM | FIN, CRM, PRT | Phí dịch vụ / CSAT |
| `PjmProjectClosed` | PJM | FIN, FSM, AST | Quyết toán / BH |
| `WfTaskApproved` | WF | Module nguồn | Tiếp tục vòng đời |
| `WfTaskRejected` | WF | Module nguồn | Trả về chỉnh sửa |
| `BiDatasetRefreshRequested` | BI/SYS | ETL | Làm mới dữ liệu |

---

## 3. Hợp đồng dữ liệu logic (tham chiếu chéo)

### 3.1. CustomerRef
| Trường | Nguồn | Ghi chú |
|---|---|---|
| `customerId` | CRM | Khóa chính |
| `code`, `name` | CRM | Snapshot cho chứng từ |
| `taxCode` | CRM | Hóa đơn |

### 3.2. ItemRef
| Trường | Nguồn | Ghi chú |
|---|---|---|
| `itemId` | INV | Khóa chính |
| `sku`, `name`, `uom` | INV | Snapshot |
| `itemType` | INV | Goods/Service/Recipe… |

### 3.3. EmployeeRef
| Trường | Nguồn | Ghi chú |
|---|---|---|
| `employeeId` | HRM | Khóa chính |
| `userId` | SYS | Liên kết đăng nhập |
| `orgUnitId` | SYS/HRM | Phạm vi |

### 3.4. MoneyAmount
| Trường | Mô tả |
|---|---|
| `amount`, `currency` | Số tiền |
| `exchangeRate` | Nếu đa tiền tệ |
| `taxAmount` | Thuế (nếu tách) |

---

## 4. Quyền publish / subscribe

| Vai trò | Được phép |
|---|---|
| Module sở hữu aggregate | Publish event của aggregate đó |
| Module khác | Subscribe; **không** giả mạo publisher |
| Integration account | Chỉ event/API trong scope API Key |
| BI | Subscribe/read projection; không publish nghiệp vụ |

---

## 5. Phiên bản & tương thích

1. Thêm field optional → tăng `payloadVersion` minor (cùng major tương thích).
2. Đổi nghĩa / xóa field → major; chạy dual-publish tạm thời.
3. Mọi breaking change ghi vào lịch sử INT-03 và thông báo Tech Lead các module subscribe.

---

## 6. Truy vết SRS
Mỗi event nên ánh xạ ít nhất một UC nguồn (ví dụ `HrmPayrollPosted` ← UC tính/chốt lương HRM).

---

*Hết INT-03-v1.0.*
