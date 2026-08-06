# Catalog event M1 — Pum's ERP

| | |
|---|---|
| Mã | `EVENT-CATALOG-M1` |
| Cập nhật | 04/08/2026 |
| Bus Day-1 | In-process Outbox → Inbox (`sys.audit`) |

## Sự kiện Must M1

| EventType | Producer | Khi nào | Payload (JSON) |
|---|---|---|---|
| `hrm.leave.approved` | `WfRuntimeService` (WF Act Approve trên `leave_request`) | Đơn nghỉ Approved + trừ quỹ | `leaveRequestId`, `employeeId`, `days`, `actedBy` |
| `hrm.leave.rejected` | `WfRuntimeService` (WF Act Reject) | Đơn nghỉ Rejected · quỹ không trừ | cùng dạng |

## Consumer hiện tại

| Consumer | Event | Hành vi |
|---|---|---|
| `sys.audit` | mọi event Outbox | Ghi `inbox_message` (idempotent theo `eventId`+consumer) |

## Sự kiện backlog — Nhắn tin realtime (SYS-13)

| EventType | Producer | Khi nào | Ghi chú |
|---|---|---|---|
| `sys.msg.message.sent` | SYS-MSG (khi implement) | Sau persist tin | Optional Outbox → push/webhook; realtime chính vẫn SignalR `/hubs/msg` |

Chi tiết: [04_MSG_REALTIME.md](./04_MSG_REALTIME.md).

> Nghiệp vụ leave (status + balance) vẫn **đồng bộ** trong WF Act. Outbox phục vụ tích hợp / audit / Phase 2 (FIN, notify…).

## Trạng thái Outbox

`Pending` → `Published` · lỗi retry → `Dead` (sau 8 lần).

Xem ops: `GET /api/sys/outbox/recent` (`sys.license.manage`) · runbook [RUNBOOK_INT05_OUTBOX.md](./RUNBOOK_INT05_OUTBOX.md).
