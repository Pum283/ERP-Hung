# Runbook INT-05 — Outbox / Inbox

| | |
|---|---|
| Mã | `RUNBOOK-INT05` |
| Cập nhật | 04/08/2026 |
| Schema | `erp_sys` |

## Triệu chứng thường gặp

| Hiện tượng | Kiểm tra |
|---|---|
| Event kẹt `Pending` | Dispatcher có chạy? log `OutboxDispatcherHostedService` |
| Status `Dead` | `LastError` + `AttemptCount` ≥ 8 |
| Client double-submit | Header `Idempotency-Key` · bảng `idempotency_record` |
| Thiếu correlation | Response/header `X-Correlation-Id` · cột Outbox `CorrelationId` |

## SQL nhanh (SQL Server)

```sql
-- Outbox gần đây
SELECT TOP 50 Id, EventType, Status, AttemptCount, CorrelationId, CreatedAt, PublishedAt, LastError
FROM erp_sys.outbox_message
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;

-- Inbox consumer
SELECT TOP 50 EventId, Consumer, EventType, Status, CreatedAt
FROM erp_sys.inbox_message
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;

-- Replay thủ công 1 message Dead → Pending
UPDATE erp_sys.outbox_message
SET Status = 'Pending', NextAttemptAt = NULL, LastError = NULL, AttemptCount = 0, UpdatedAt = SYSUTCDATETIME()
WHERE Id = '<guid>';
```

## API ops (admin)

| Method | Path | Permission |
|---|---|---|
| GET | `/api/sys/outbox/recent?take=20` | `sys.license.manage` |
| GET | `/api/sys/license-modules` | `sys.license.manage` |
| PUT | `/api/sys/license-modules/{code}` body `{ "isEnabled": false }` | `sys.license.manage` |

## Soft-disable module

Tắt module soft (CRM/FIN/…) → API `/api/{module}/...` trả **403**; `auth` + `sys` vẫn chạy (hard path).  
**Không** tắt `SYS`. Script kiểm: `scripts/smoke_g4_soft_disable.py`.

## Handover

1. Giữ secret DB / JWT ngoài git (`.env`).  
2. Khi scale nhiều instance API: cần claim Outbox (row lock) — Day-1 single host.  
3. DLQ: lọc `Status = 'Dead'` rồi replay hoặc bỏ.
