# RBAC — parity với Digi ERP

| | |
|---|---|
| Mẫu | `Mẫu/ERP Digi…/erp-corporation-api-v2` + `erp-corporation-api-tester` |
| Ngày | 04/08/2026 |

## Mô hình

```
User → UserRole (1..N · IsActive · RevokedAt · ValidFrom/To)
     → Role (BypassDataScope)
     → RolePermission → Permission.code   // check = UNION mọi role (quyền cao hơn thắng)

User → UserDepartment (1..N)
     · đúng 1 IsPrimary · các PB còn lại ngang hàng
     · mỗi membership có JobLevelId riêng
     · denorm primary → AppUser.DepartmentId / JobLevelId (compat)
```

- Check API: `[AuthorizePermission("code")]` · không nhét permission vào JWT.
- Super: `BypassDataScope=true` → mọi quyền + scope All.
- Data scope: lấy từ **JobLevel của phòng ban chính**; cây phòng ban truy cập = **hợp** mọi membership.
- Code: `{module}.{resource}.{action}` lowercase.

## Quyền (Permission)

| Thao tác | Cho phép? |
|---|---|
| Xem danh mục | Có (`GET /api/sys/permissions`) |
| Tạo / sửa / xóa | **Không** — catalog chỉ seed |
| Seed | `DbSeeder` khi ship chức năng mới (thêm vào `permDefs`) |

## Role

- Tạo role mới + gắn quyền: dễ (`POST /api/sys/roles`, `PUT /api/sys/roles/{id}/permissions`).
- UI: PermissionPicker trên `/app/sys/roles`.

## UI

| Màn | Path | Ghi chú |
|---|---|---|
| Vai trò + gắn quyền | `/app/sys/roles` | CRUD role · assign permissions |
| Catalog quyền | `/app/sys/permissions` | **Chỉ xem** |
| Người dùng | `/app/sys/users` | Multi-role · multi-dept (1 chính) · job level / PB |
| Gate UI | `<CanAccess>` · `usePermissions` | |

## Quyền granular (SYS)

`sys.role.read|update|assign` · `sys.permission.read`  
(`sys.permission.update|delete` đã ngưng — chỉ còn seed)  
(giữ `sys.role.manage` tương thích menu cũ)
