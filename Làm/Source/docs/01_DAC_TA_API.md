# Đặc tả API — Pum's ERP

> **Living doc** — cập nhật **liên tục** trong suốt quá trình làm: mỗi lần thêm/sửa/xóa endpoint → sửa file này **cùng lượt code**, không để lệch `Erp.Api`.

| Thuộc tính    | Giá trị                                         |
| ------------- | ----------------------------------------------- |
| Mã            | `API-SPEC` (living)                             |
| Cập nhật lần  | 04/08/2026 — Tenant logo Cloudinary · multi-role/dept RBAC |
| Base URL      | `http://localhost:1111` (dev)                   |
| Swagger       | `/swagger` (đối chiếu runtime)                  |
| Auth          | JWT Bearer                                      |
| Content-Type  | `application/json` (trừ upload file)            |
| Nguồn sự thật | Controllers `Erp.Api` · file này phải khớp code |

> Prompt AI: `@docs/01_DAC_TA_API.md` khi thêm/sửa API. **Bắt buộc** cập nhật bảng endpoint trước khi kết thúc task.

---

## 1. Quy ước chung

### 1.1 Envelope

Hầu hết endpoint trả:

```json
{
  "success": true,
  "message": null,
  "data": {}
}
```

Lỗi nghiệp vụ / forbidden thường:

```json
{
  "success": false,
  "message": "…"
}
```

| HTTP | Nghĩa                                         |
| ---- | --------------------------------------------- |
| 200  | OK                                            |
| 400  | Request không hợp lệ                          |
| 401  | Chưa đăng nhập / token sai                    |
| 403  | Thiếu permission **hoặc** module chưa license |
| 404  | Không tìm thấy                                |
| 500  | Lỗi hệ thống (qua `ExceptionMiddleware`)      |

### 1.2 Auth header

```
Authorization: Bearer {accessToken}
```

Claim JWT quan trọng: `sub` / `NameIdentifier` = `userId`, `tenant_id`, `role`.

### 1.3 Permission

Attribute `[AuthorizePermission("code")]`.

Dạng code: `{module}.{resource}.{action}` — ví dụ `sys.user.read`.

| Code                   | Dùng cho                                                         |
| ---------------------- | ---------------------------------------------------------------- |
| `sys.user.read`        | Đọc user / org / dept / job-level / file / ping-secure           |
| `sys.user.manage`      | Upsert user / org / dept / job-level / upload file               |
| `sys.role.manage`      | Role + permission catalog                                        |
| `sys.license.manage`   | (seed) quản trị license — chưa có CRUD API riêng                 |
| `sys.org.manage`       | (seed) — hiện upsert org dùng `sys.user.manage`                  |
| `wf.task.read`         | Definitions · inbox `tasks/my` · menu WF                         |
| `wf.task.act`          | Approve / Reject task                                            |
| `hrm.employee.read`    | List/detail NV · job-titles · employee-types                     |
| `hrm.employee.manage`  | Upsert NV                                                        |
| `hrm.leave.read`       | leave-types · balances · requests                                |
| `hrm.leave.manage`     | Tạo / submit đơn nghỉ                                            |
| `hrm.contract.read`    | List HĐLĐ                                                        |
| `hrm.contract.manage`  | Upsert HĐLĐ                                                      |
| `sys.msg.read`         | (backlog) Đọc hội thoại / lịch sử / unread                       |
| `sys.msg.send`         | (backlog) Tạo hội thoại · gửi tin                                |

### 1.4 License middleware

Path `/api/{module}/...` — `module` phải có trong `license_module` (enabled) của tenant.

| Luôn cho qua | `auth`, `sys`, `health`           |
| ------------ | --------------------------------- |
| Cần license  | `wf`, `hrm`, … (mọi segment khác) |

### 1.5 Enum (JSON string)

| Enum         | Giá trị                               |
| ------------ | ------------------------------------- |
| `ScopeType`  | `Own` · `Team` · `Department` · `All` |
| `UserStatus` | `Active` · `Locked` · `Disabled`      |

### 1.6 Upsert

Nhiều POST master data: có `id` → update; `id` = `null` → create.

---

### Correlation · Outbox · Idempotency (G4)

| Header / cơ chế | Ghi chú |
| --- | --- |
| `X-Correlation-Id` | Client gửi hoặc server sinh · echo response · gắn Outbox · log scope |
| `Idempotency-Key` | Tuỳ chọn trên `POST /api/hrm/leave-requests` và `POST /api/wf/tasks/{id}/act` · replay + header `X-Idempotency-Replayed` |
| Outbox | `erp_sys.outbox_message` · dispatcher background |
| Inbox | `erp_sys.inbox_message` · unique (tenant, eventId, consumer) |
| Events M1 | `hrm.leave.approved` / `rejected` — xem [03_EVENT_CATALOG_M1.md](./03_EVENT_CATALOG_M1.md) |

## 2. Auth — `/api/auth`

### `POST /api/auth/login`

|            |           |
| ---------- | --------- |
| Auth       | Anonymous |
| Permission | —         |

**Body**

```json
{ "username": "admin", "password": "!Abc123" }
```

**`data`**

| Field              | Type      |
| ------------------ | --------- |
| accessToken        | string    |
| expiresAt          | datetime  |
| userId             | guid      |
| username           | string    |
| displayName        | string?   |
| roles              | string[]  |
| permissions        | string[]  |
| effectiveScopeType | ScopeType |
| bypassDataScope    | bool      |

### `GET /api/auth/me`

|            |     |
| ---------- | --- |
| Auth       | JWT |
| Permission | —   |

**`data`** = LoginResponse + thêm:

| Field          | Type     |
| -------------- | -------- |
| tenantId       | guid     |
| email          | string?  |
| departmentId   | guid?    |
| jobLevelId     | guid?    |
| enabledModules | string[] |

---

## 3. SYS — `/api/sys`

### 3.1 Health

| Method | Path                   | Auth      | Permission      | Ghi chú                                             |
| ------ | ---------------------- | --------- | --------------- | --------------------------------------------------- |
| GET    | `/api/sys/health`      | Anonymous | —               | `{ success, service, utc }` (không envelope `data`) |
| GET    | `/api/sys/ping-secure` | JWT       | `sys.user.read` | Kiểm tra JWT + permission                           |

### 3.2 Tổ chức

| Method | Path                   | Permission        | Body / `data`                                    |
| ------ | ---------------------- | ----------------- | ------------------------------------------------ |
| GET    | `/api/sys/org-units`   | `sys.user.read`   | `OrgUnitDto[]`                                   |
| POST   | `/api/sys/org-units`   | `sys.user.manage` | Body `OrgUnitUpsertRequest` → `OrgUnitDto`       |
| GET    | `/api/sys/departments` | `sys.user.read`   | `DepartmentDto[]`                                |
| POST   | `/api/sys/departments` | `sys.user.manage` | Body `DepartmentUpsertRequest` → `DepartmentDto` |
| GET    | `/api/sys/job-levels`  | `sys.user.read`   | `JobLevelDto[]`                                  |
| POST   | `/api/sys/job-levels`  | `sys.user.manage` | Body `JobLevelUpsertRequest` → `JobLevelDto`     |

**`OrgUnitUpsertRequest` / `OrgUnitDto`**

| Field    | Type   | Ghi chú         |
| -------- | ------ | --------------- |
| id       | guid?  | null = create   |
| code     | string |                 |
| name     | string |                 |
| parentId | guid?  |                 |
| unitType | string | ví dụ `Company` |
| isActive | bool   |                 |

**`DepartmentUpsertRequest` / `DepartmentDto`**

| Field         | Type   |
| ------------- | ------ |
| id            | guid?  |
| code          | string |
| name          | string |
| parentId      | guid?  |
| orgUnitId     | guid   |
| managerUserId | guid?  |
| isActive      | bool   |

**`JobLevelUpsertRequest` / `JobLevelDto`**

| Field            | Type      |
| ---------------- | --------- |
| id               | guid?     |
| code             | string    |
| name             | string    |
| levelOrder       | int       |
| defaultScopeType | ScopeType |
| isActive         | bool      |

### 3.3 Role / Permission

| Method | Path                                  | Permission        | Body / `data`                                 |
| ------ | ------------------------------------- | ----------------- | --------------------------------------------- |
| GET    | `/api/sys/roles`                      | `sys.role.manage` | `RoleDto[]`                                   |
| POST   | `/api/sys/roles`                      | `sys.role.manage` | `RoleUpsertRequest` → `RoleDto`               |
| PUT    | `/api/sys/roles/{roleId}/permissions` | `sys.role.manage` | Body: `guid[]` permissionIds → `{ ok: true }` |
| GET    | `/api/sys/permissions`                | `sys.role.manage` | `PermissionDto[]` (catalog toàn SP)           |

**`RoleUpsertRequest`**

| Field           | Type    |
| --------------- | ------- |
| id              | guid?   |
| code            | string  |
| name            | string  |
| description     | string? |
| bypassDataScope | bool    |
| isActive        | bool    |

**`RoleDto`** = trên + `isSystem`, `permissionIds`.

**`PermissionDto`:** `id`, `moduleCode`, `code`, `name`, `resource`, `action`.

### 3.4 User

| Method | Path                            | Permission        | Ghi chú                            |
| ------ | ------------------------------- | ----------------- | ---------------------------------- |
| GET    | `/api/sys/users`                | `sys.user.read`   | Lọc theo **data scope** của caller |
| POST   | `/api/sys/users`                | `sys.user.manage` | Upsert                             |
| PUT    | `/api/sys/users/{userId}/roles` | `sys.user.manage` | Body: `guid[]` roleIds             |

**Data scope trên `GET /users`**

| Scope        | Thấy                                  |
| ------------ | ------------------------------------- |
| Own          | Chỉ chính mình                        |
| Team         | Mình + user có `managerUserId` = mình |
| Department   | User thuộc các phòng trong phạm vi    |
| All / bypass | Tất cả trong tenant                   |

**`UserUpsertRequest`**

| Field            | Type       | Ghi chú                |
| ---------------- | ---------- | ---------------------- |
| id               | guid?      |                        |
| username         | string     |                        |
| displayName      | string?    |                        |
| email            | string?    |                        |
| phone            | string?    |                        |
| password         | string?    | null/empty = không đổi |
| status           | UserStatus |                        |
| primaryOrgUnitId | guid?      |                        |
| departmentId     | guid?      |                        |
| jobLevelId       | guid?      |                        |
| managerUserId    | guid?      |                        |

**`UserDto`:** không trả password; có `roleIds`.

### 3.5 Menu (shell FE)

| Method | Path            | Auth | Permission                                        |
| ------ | --------------- | ---- | ------------------------------------------------- |
| GET    | `/api/sys/menu` | JWT  | — (lọc theo license module + permission của user) |

**`MenuItemDto`:** `id`, `code`, `parentId`, `moduleCode`, `title`, `routePath`, `permissionCode`, `icon`, `sortOrder`.

### 3.6 File (local disk)

| Method | Path                            | Permission        | Ghi chú                                        |
| ------ | ------------------------------- | ----------------- | ---------------------------------------------- |
| POST   | `/api/sys/files/upload`         | `sys.user.manage` | `multipart/form-data` field `file` · max ~20MB |
| GET    | `/api/sys/files/{**storageKey}` | `sys.user.read`   | Stream file · chỉ trong tenant                 |

**Upload `data`**

```json
{ "storageKey": "…", "fileName": "…", "sizeBytes": 123 }
```

---

## 4. WF — `/api/wf` + SignalR

Cần JWT + license module `WF`.

| Method | Path                         | Permission     | `data` / body                                                             |
| ------ | ---------------------------- | -------------- | ------------------------------------------------------------------------- |
| GET    | `/api/wf/definitions`        | `wf.task.read` | `{ id, code, name, moduleCode, docType, isActive }[]`                     |
| GET    | `/api/wf/tasks/my`           | `wf.task.read` | `WfTaskDto[]` — task `Pending` của user + `docSummary`                    |
| POST   | `/api/wf/tasks/{taskId}/act` | `wf.task.act`  | Body `WfActRequest` → `{ ok: true }` · cập nhật leave nếu `leave_request` |

**`WfTaskDto`:** `id`, `instanceId`, `nodeId`, `nodeName`, `status`, `dueAt`, `sourceModule`, `sourceDocType`, `sourceDocId`, `docSummary`.

**`WfActRequest`:** `action` = `Approve` \| `Reject`, `comment?`.

> Engine Day-1: 1 bước duyệt · assignee = manager NV (fallback SUPER_ADMIN). Approve leave → trừ quỹ phép.

### 4.1 Realtime inbox — SignalR (**không poll**)

| | |
|---|---|
| Hub | `/hubs/wf` |
| Auth | JWT · WebSocket: `access_token` query |
| Event | `inboxChanged` → `{ reason, taskId? }` |
| Đẩy khi | Start WF gán task · Act Approve/Reject |
| FE | `shared/realtime/wf-hub.ts` — subscribe rồi GET `/tasks/my` **một lần** |

**Cấm** `setInterval` / gọi liên tục `/api/wf/tasks/my`.

### 4.2 Nhắn tin realtime — SYS-MSG

| Method | Path | Permission | Ghi chú |
| ------ | ---- | ---------- | ------- |
| GET | `/api/sys/msg/conversations` | `sys.msg.read` | List hội thoại + unread |
| POST | `/api/sys/msg/conversations` | `sys.msg.send` | Body: `{ peerUserId }` (1-1) hoặc `{ title, memberIds }` (nhóm) |
| GET | `/api/sys/msg/conversations/{id}/messages?before=&take=` | `sys.msg.read` | Lịch sử (mới→cũ rồi FE đảo) |
| POST | `/api/sys/msg/conversations/{id}/messages` | `sys.msg.send` | Body: `{ body, attachmentFileId? }` |
| POST | `/api/sys/msg/conversations/{id}/read` | `sys.msg.read` | Đánh dấu đã đọc |
| GET | `/api/sys/msg/unread-count` | `sys.msg.read` | Badge tổng |
| GET | `/api/sys/msg/directory` | `sys.msg.read` | Danh bạ user Active (bắt đầu chat) |

| | |
|---|---|
| Hub | `/hubs/msg` |
| Auth | JWT · `access_token` query |
| Events | `messageReceived` · `conversationUpdated` |
| FE | `/app/sys/messages` · `shared/realtime/msg-hub.ts` · badge AppShell |

**Cấm** poll unread / inbox chat.

Spec chi tiết: [04_MSG_REALTIME.md](./04_MSG_REALTIME.md).

---

## 5. HRM — `/api/hrm` (M1 Day-1 / E2E-05)

Cần JWT + license module `HRM`.

| Method | Path                                  | Permission            | Ghi chú                                        |
| ------ | ------------------------------------- | --------------------- | ---------------------------------------------- |
| GET    | `/api/hrm/employees?q=`               | `hrm.employee.read`   | List + data scope                              |
| GET    | `/api/hrm/employees/{id}`             | `hrm.employee.read`   | Chi tiết                                       |
| POST   | `/api/hrm/employees`                  | `hrm.employee.manage` | Upsert (id null = create)                      |
| GET    | `/api/hrm/job-titles`                 | `hrm.employee.read`   | Catalog                                        |
| GET    | `/api/hrm/employee-types`             | `hrm.employee.read`   | Catalog                                        |
| GET    | `/api/hrm/leave-types`                | `hrm.leave.read`      | Catalog                                        |
| GET    | `/api/hrm/leave-balances?employeeId=` | `hrm.leave.read`      | Mặc định = quỹ của NV gắn user                 |
| GET    | `/api/hrm/leave-requests?employeeId=` | `hrm.leave.read`      | Đơn của mình (hoặc filter)                     |
| POST   | `/api/hrm/leave-requests`             | `hrm.leave.manage`    | Tạo · `submit=true` → start WF `LEAVE_APPROVE` |
| GET    | `/api/hrm/contracts?employeeId=`      | `hrm.contract.read`   | List HĐLĐ                                      |
| POST   | `/api/hrm/contracts`                  | `hrm.contract.manage` | Upsert HĐ                                      |

**`LeaveRequestCreateRequest`:** `employeeId?`, `leaveTypeId`, `fromDate`, `toDate`, `days`, `reason?`, `submit`.

**`ContractUpsertRequest`:** `id?`, `employeeId`, `contractNo`, `contractType`, `startDate`, `endDate?`, `status`.

FE: `/app/hrm/employees` (SideSheet) · `/app/hrm/leaves` · `/app/hrm/contracts` (SideSheet) · `/app/wf/tasks` (SignalR) · `/app/sys/users` (SideSheet) · `/app/sys/org` · `/app/sys/roles`. Module switcher: logo → `/app`.

---

## 6. Luồng FE tối thiểu

```
POST /api/auth/login
  → lưu accessToken
GET  /api/auth/me
GET  /api/sys/menu          → render sidebar (license + permission)
GET  /api/sys/users         → trang quản trị (nếu can sys.user.read)
GET  /api/hrm/employees     → hồ sơ nhân sự
POST /api/hrm/leave-requests (submit=true) → WF
GET  /api/wf/tasks/my       → inbox phê duyệt
POST /api/wf/tasks/{id}/act → Approve/Reject
```

Seed: `admin` / `!Abc123` — xem [QUY-UOC-CONG-DEV.md](./QUY-UOC-CONG-DEV.md).
E2E demo: `hr.spec1` tạo đơn → `hr.manager` duyệt.

---

## 7. Chưa có API (cố ý / backlog)

| Hạng mục                      | Ghi chú                              |
| ----------------------------- | ------------------------------------ |
| **Nhắn tin realtime SYS-13**  | Spec [04_MSG_REALTIME.md](./04_MSG_REALTIME.md) · chưa REST/hub |
| Full CRUD plan/license tenant | Có list + toggle module; tạo plan sau |
| Refresh token / logout server | Day-1 chỉ JWT access                 |
| Full catalog Must HRM/SRS     | Sau slice E2E-05                     |
| LMS / AST Phase 2 đầy đủ      | Day-1 N1 masters/docs đã có · sâu Phase 2 |

---


## 8. Quy tắc cập nhật đặc tả (bắt buộc)

File này **không đóng** sau G2 — luôn sống cùng source.

Khi thêm / sửa / xóa endpoint:

1. Controller + permission đúng `{module}.{resource}.{action}`
2. **Sửa ngay** bảng trong file này (method · path · auth · permission · body/`data`)
3. Đổi enum / envelope / middleware → cập nhật mục quy ước (§1)
4. FE gọi qua `shared/api` — không hardcode URL rải rác
5. Dòng “Cập nhật lần” ở đầu file = ngày thay đổi API gần nhất

---

## 9. Catalog endpoint (tóm tắt — khớp Erp.Api)

| Method | Path | Permission / Auth |
| --- | --- | --- |
| POST | /api/auth/login | Anonymous |
| GET | /api/auth/me | JWT |
| GET | /api/sys/health | Anonymous |
| GET | /api/sys/ping-secure | sys.user.read |
| GET/POST | /api/sys/org-units | read / manage |
| GET/POST | /api/sys/departments | read / manage |
| GET/POST | /api/sys/job-levels | read / manage |
| GET | /api/sys/roles | sys.role.read |
| POST | /api/sys/roles | sys.role.update |
| PUT | /api/sys/roles/{roleId}/permissions | sys.role.assign |
| POST | /api/sys/roles/{roleId}/copy | sys.role.update |
| GET | /api/sys/permissions?includeInactive | sys.permission.read · **chỉ xem** (catalog seed) |
| GET | /api/sys/tenant | sys.user.read · gồm `logoUrl` |
| PUT | /api/sys/tenant | sys.license.manage |
| POST | /api/sys/tenant/logo | sys.license.manage · multipart file · Cloudinary · PNG/JPEG/WebP/SVG ≤2MB · khuyến nghị 512×512 |
| DELETE | /api/sys/tenant/logo | sys.license.manage |
| GET/POST | /api/sys/users | read / manage · `departments[]` (`departmentId`, `jobLevelId`, `isPrimary`) |
| PUT | /api/sys/users/{userId}/roles | sys.user.manage |
| GET | /api/sys/menu | JWT (lọc license+perm) |
| GET | /api/sys/outbox/recent | sys.license.manage |
| GET | /api/sys/license-modules | sys.license.manage |
| PUT | /api/sys/license-modules/{code} | sys.license.manage · body `{ isEnabled }` |
| POST | /api/sys/files/upload | sys.user.manage |
| GET | /api/sys/files/{**storageKey} | sys.user.read |
| GET | /api/wf/definitions | wf.task.read |
| GET | /api/wf/tasks/my | wf.task.read |
| POST | /api/wf/tasks/{taskId}/act | wf.task.act |
| WS | /hubs/wf | JWT (`access_token`) · event inboxChanged |
| GET/POST | /api/sys/msg/conversations · /messages · /read · /mute · /members | sys.msg.read / send |
| PUT | /api/sys/msg/conversations/{id}/messages/{messageId} | sys.msg.send (edit Digi) |
| POST | …/messages/{messageId}/recall | sys.msg.send |
| POST | …/messages/{messageId}/reactions | sys.msg.send (toggle emoji) |
| WS | /hubs/msg | JWT · messageReceived · messageEdited · conversationUpdated · ReceiveTypingStatus |
| GET | /api/hrm/employees · /{id} | hrm.employee.read |
| GET | /api/hrm/employees/export.csv | hrm.employee.read |
| GET | /api/hrm/employees/trial-expiring | hrm.employee.read |
| POST | /api/hrm/employees | hrm.employee.manage |
| POST | /api/hrm/employees/{id}/status | hrm.employee.manage |
| GET | /api/hrm/employees/{id}/status-history | hrm.employee.read |
| GET | /api/hrm/job-titles · /employee-types | hrm.employee.read |
| GET | /api/hrm/leave-types · /leave-balances · /leave-requests | hrm.leave.read |
| POST | /api/hrm/leave-requests | hrm.leave.manage |
| GET/POST | /api/hrm/contracts | read / manage |
| GET | /api/hrm/contracts/expiring | hrm.contract.read |
| POST | /api/hrm/contracts/{id}/renew · /terminate | hrm.contract.manage |
| GET/POST | /api/wf/work-types · work-projects · work-items | wf.task.read / act |
| GET | /api/wf/workload | wf.task.read |
| GET/POST | /api/{module}/masters · /documents | JWT · license module (LMS…PRT, HRM, WF) |
| POST | /api/{module}/documents/{id}/transition | JWT · license module |

**Realtime / tích hợp (không REST):** Outbox dispatcher · hrm.leave.approved|rejected · header X-Correlation-Id.  
**Chat realtime:** đã ship Must slice — xem §4.2 và [04_MSG_REALTIME.md](./04_MSG_REALTIME.md).  
**Day-1 module N1:** masters/docs dùng chung `mod_master` / `mod_document` · FE `/app/{lms|crm|…}`.

