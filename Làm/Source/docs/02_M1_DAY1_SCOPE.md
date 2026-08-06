# M1 Day-1 — Phạm vi triển khai (slice)

| | |
|---|---|
| Bundle | **B1 — Nhân sự số** (SYS + HRM + WF) |
| Journey | **E2E-05** Hire → Org/Dept/JobLevel → Phép có duyệt WF |
| Không gồm Day-1 slice | FIN post lương · LMS · AST · full SRS Must catalog |
| Backlog M1 (sau Day-1) | **Nhắn tin realtime SYS-13** — xem [04_MSG_REALTIME.md](./04_MSG_REALTIME.md) |

## DoD M1 (code) — Day-1 đã có

1. SYS: login · RBAC · org/dept/job · users · menu license · audit · file
2. HRM: hồ sơ NV CRUD · hợp đồng list/upsert · quỹ phép · đơn nghỉ tạo/gửi duyệt
3. WF: start instance từ đơn nghỉ · inbox duyệt · Approve/Reject · cập nhật trạng thái đơn
4. FE shell đủ thao tác trên · chạy SQL Server hosted

## DoD M1 bổ sung — Nhắn tin realtime (chưa làm)

5. SYS-MSG: hội thoại 1-1 · gửi/nhận SignalR · lịch sử · unread badge (`UC_SYS_095/097/098/099/100`)
6. Hub `/hubs/msg` · API `/api/sys/msg/*` · permission `sys.msg.*` · FE panel chat trên shell

*Full catalog Must còn lại trong SRS = backlog Phase 2+.*
