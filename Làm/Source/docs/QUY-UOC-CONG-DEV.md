# Quy ước cổng dev

| Thành phần | URL |
|---|---|
| FE shell | http://localhost:3000 |
| BE API (HTTPS) | https://localhost:7000 |
| BE API (HTTP) | http://localhost:5000 |
| Swagger | https://localhost:7000/swagger |

**Công thức (Phase 1):** 1 FE + 1 BE — khác Digione (nhiều port theo phân hệ).

## CORS

BE cho phép origin `http://localhost:3000`.

## Tài khoản seed

Mật khẩu chung: `!Abc123`

| Username | Role chính | Ghi chú |
|---|---|---|
| `admin` | SUPER_ADMIN | Bypass mọi quyền |
| `ceo` | EXECUTIVE | Tổng giám đốc |
| `dceo` / `cfo` / `chro` / `cto` | Ban điều hành | Phó TGĐ / CFO / CHRO / CTO |
| `hr.manager` | HR_MANAGER | + DEPT_MANAGER · APPROVER |
| `it.manager` | IT_MANAGER | |
| `sales.manager` / `fin.manager` / `mkt.manager`… | Trưởng phòng | |
| `dev.lan` / `fin.acc1` / `sales.nam`… | STAFF / ACCOUNTANT | Nhân viên |
| `hr.intern` / `it.intern` / `dev.hung` | INTERN | Thực tập |

Full roster (~45 user): xem `DbSeeder` / `DbSeeder.DemoOrg.cs`.

## Biến môi trường

- BE: `src/Erp.Api/.env` (từ `.env.example`) hoặc `appsettings.json`
- FE: `frontend/.env.local` (từ `.env.example`) — `NEXT_PUBLIC_API_URL`

## RBAC tập trung

1. Chỉ schema `sys` quản lý Role / Permission / UserRole / UserDepartment / JobLevel.
2. User **1..N role** — kiểm tra quyền = **hợp (union)** RolePermission (có quyền nếu bất kỳ role nào có).
3. User **1..N phòng ban** — đúng **1 primary**; mỗi membership có **JobLevel** riêng.
4. **Permission catalog chỉ seed** (`DbSeeder` / khi ship chức năng) — UI/API không tạo/sửa/xóa quyền; chỉ xem + gắn vào role.
5. Module khác **không** tạo bảng quyền riêng.
6. FE dùng `usePermissions()`; BE dùng `[AuthorizePermission]`.
7. Chi tiết: `Làm/Nghiệp vụ/00. Tổng quan/Timeline/RBAC_DIGI_PARITY.md`.

## Realtime

1. **Cấm** poll / gọi API liên tục (`setInterval`, loop `useEffect` phụ thuộc hàm không ổn định).
2. Realtime dùng **SignalR**:
   | Hub | Event chính | Dùng cho |
   |---|---|---|
   | `/hubs/wf` | `inboxChanged` | Inbox phê duyệt WF |
   | `/hubs/msg` | `messageReceived` · `conversationUpdated` · `messageRecalled` | **Nhắn tin nội bộ** (SYS-13) |
3. HTTP chỉ load **một lần** khi vào trang / mở hội thoại; cập nhật tiếp theo từ SignalR.
4. Spec chat: [04_MSG_REALTIME.md](./04_MSG_REALTIME.md).

## FE theo module

1. Shell có **nút chuyển module** — chỉ liệt kê module user có quyền (license + menu/permission).
2. Sidebar **chỉ** hiện menu của module đang chọn.
3. Route theo module: `/app/{sys|hrm|wf}/…` · active module đồng bộ URL + `localStorage`.
4. Bán SKU / cắt source: [`MODULES.md`](../MODULES.md) · `python scripts/cut_modules.py --keep SYS,HRM,WF --dry-run`.

## FE form / chi tiết (Digione Sheet)

1. **Thêm mới / Chi tiết / Sửa** mở **panel phải** (`SideSheet`) — không nhảy trang mới.
2. Component dùng chung: `shared/ui/SideSheet.tsx`.
3. Danh sách ở lại; route `/new` / `/[id]` chỉ redirect về list (bookmark cũ).
